using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Sharepoint;
using Mod.CatsV3.Sharepoint.Services;
using Mod.Framework.WebApi.Controllers;
using Newtonsoft.Json;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class FolderController : CrudControllerBase<FolderDto, Folder>
    {
        ICatsAppService catsAppService;
        ICatsDocumentAppService catsDocumentAppService;
        IWebHostEnvironment _webHostEnvironment;
        ILogger<Folder> logger;
        public FolderController(
            IWebHostEnvironment environment,
            ILogger<Folder> logger, 
            IFolderAppService service, 
            ICatsAppService catsAppService, ICatsDocumentAppService catsDocumentAppService) : base(logger, service)
        {
            this.catsAppService = catsAppService;
            this.logger = logger;
            this.catsDocumentAppService = catsDocumentAppService;
            _webHostEnvironment = environment;
        }

        [HttpPost]
        public override ActionResult<FolderDto> Create(FolderDto dto)
        {
            return Service.Create(dto);
        }

        [HttpPut("{id}")]
        public override ActionResult<FolderDto> Update(int id, FolderDto dto)
        {
            try {

                //dto.Documents.ToList().ForEach(doc => {
                //    if (doc.Id == 0)
                //    {
                //        catsAppService.getDocumentAppService().Create(doc);
                //    }
                //    else if (doc.Id > 0 && (bool)doc.IsModified)
                //    {
                //        catsAppService.getDocumentAppService().Update(doc);
                //    }
                //});

                dto = Service.Update(dto);

                dto = Service.GetBy(x => x.Id == id).FirstOrDefault();

                return dto;
            }
            catch(Exception ex)
            {
                dto = Service.GetBy(x => x.Id == id).FirstOrDefault();
                return dto;
            }
            
        }

        [HttpPost("addnewdocuments")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<FolderDto>> Post()
        {
            var formdata = Request.Form;

            var reviewDocuments = formdata.Files.Count() > 0 ? formdata.Files.Where(f => f.Name.Contains("review")) : Array.Empty<FormFile>();
            var referenceDocuments = formdata.Files.Count() > 0 ? formdata.Files.Where(f => f.Name.Contains("reference")) : Array.Empty<FormFile>();
            var finalDocuments = formdata.Files.Count() > 0 ? formdata.Files.Where(f => f.Name.Contains("final")) : Array.Empty<FormFile>();

            string fileType = "";

            var tempfolder = formdata.Select(x => x).FirstOrDefault(x => x.Key == "folder").Value;
            var dto = JsonConvert.DeserializeObject<FolderDto>(tempfolder);
            var correspondenceDto = catsAppService.getCorrespondenceAppService().Get(dto.CorrespondenceId);

            //upload review document if any
            if (reviewDocuments.Count() > 0)
            {
                //Apply permission and eventually upload documents
                fileType = "Review Document";
                correspondenceDto = await catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "New Letter", correspondenceDto);
            }


            //upload Reference document if any
            if (referenceDocuments.Count() > 0)
            {
                //Apply permission and eventually upload documents
                fileType = "Reference Document";
                correspondenceDto = await catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "New Letter", correspondenceDto);
            }


            //upload final document if any
            if (finalDocuments.Count() > 0)
            {
                //Apply permission and eventually upload documents
                fileType = "Final Document";
                correspondenceDto = await catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "New Letter", correspondenceDto);
            }

            dto.Documents = dto.Documents.Where(doc => doc.Id > 0).ToList();
            dto = Service.Update(dto);

            dto = Service.GetBy(x => x.Id == dto.Id).FirstOrDefault();

            return Ok(dto);
        }
    }
}

