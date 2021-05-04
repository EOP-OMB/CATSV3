using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Sharepoint;
using Mod.CatsV3.Sharepoint.Services;
using Newtonsoft.Json;
using SP = Microsoft.SharePoint.Client;

namespace Mod.CatsV3.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SPMaintenanceController
    {
        ICatsAppService _catsAppService;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<Correspondence> _logger;
        IOldIQMasterListAppService _oldIQMasterService;
        ICatsDocumentAppService _catsDocumentAppService;
        public SPMaintenanceController(ICatsAppService catsAppService, IWebHostEnvironment environment, ILogger<Correspondence> logger, IOldIQMasterListAppService oldIQMasterService, ICatsDocumentAppService catsDocumentAppService)
        {
            _catsAppService = catsAppService;
            _webHostEnvironment = environment;
            _logger = logger;
            _oldIQMasterService = oldIQMasterService;
            _catsDocumentAppService = catsDocumentAppService;
        }
        [HttpGet]
        public async Task<ActionResult<string[]>> Get()
        {
            List<string> res = new List<string>();
            var correspondenceDtos = _catsAppService.getCorrespondenceAppService().GetAllForMaintenance();
            var oldMasterdata = _oldIQMasterService.GetAll();

            var listItems = correspondenceDtos.Select(x => x.CATSID).ToArray();
            correspondenceDtos = correspondenceDtos.OrderByDescending(x => x.Id);
            SPMaintenance sPMaintenance = new SPMaintenance();
            List<CATSItem> lstFilesPaths = await sPMaintenance.getAllFilesAsync(listItems);

            for (int i = 0; i <= correspondenceDtos.Count() - 1; i++)
            {
                var dto = correspondenceDtos.ToList()[i];
                var item = lstFilesPaths.Where(f => f.CATSID == dto.CATSID).FirstOrDefault();
                //List<IFormFile> files = await sPMaintenance.getIFormFilesAsync(item, dto, _webHostEnvironment.ContentRootPath, _logger, _catsAppService, oldMasterdata);
                var d = await sPMaintenance.processDocumentsUpload(dto,  oldMasterdata);
                res.Add(d);
            }

            for (int i = 0; i <= correspondenceDtos.Count() - 1; i++)
            {
                //Apply permission only
                var correspondenceDto = correspondenceDtos.ToList()[i];
                var fileType = "Review Document";
                correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(new List<IFormFile>(), fileType, correspondenceDto.IsUnderReview == true ? "Collaboration" : "New Letter", correspondenceDto);
            }

            return res.ToArray();

        }

        private Task<string> DoAsyncResult(Application.Dtos.CorrespondenceDto dto, IEnumerable<Application.Dtos.OldIQMasterListDto> oldMasterdata)
        {
            Task.Delay(4000);

            SPMaintenance sPMaintenance = new SPMaintenance();
            return  sPMaintenance.processDocumentsUpload(dto, oldMasterdata);

            //return Task.CompletedTask;
        }

        public async Task<Tuple<int, bool>> DoWorkAsync(int i)
        {
            Console.WriteLine("working..{0}", i);
            await Task.Delay(1000);
            return Tuple.Create(i, true);
        }


    }
}
