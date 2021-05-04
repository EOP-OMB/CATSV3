using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Configuration;
using Microsoft.AspNetCore.Hosting;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;

namespace Mod.CatsV3.Sharepoint.Services
{
    public class CatsDocumentAppService : AppService, ICatsDocumentAppService
    {
        IWebHostEnvironment _webHostEnvironment;
        ICatsAppService _catsAppService;
        ICorrespondenceRepository _correspondenceRepository;
        ILogger<IAppService> _logger;
        public CatsDocumentAppService(IObjectMapper objectMapper, ILogger<IAppService> logger, ICatsAppService catsAppService, ICorrespondenceRepository correspondenceRepository,  IModSession session, IServiceScopeFactory serviceProvider, IWebHostEnvironment webHostEnvironment) : base(objectMapper, null, session)
        {
            _webHostEnvironment = webHostEnvironment;
            _catsAppService = catsAppService;
            _logger = logger;
            _correspondenceRepository = correspondenceRepository;
        }

        public async Task<CorrespondenceDto> uploadDocumentsAsync(List<IFormFile> formFiles, string fileType, string processType, CorrespondenceDto dto){
            var sPUploadDocuments = new SPUploadDocuments(_webHostEnvironment.ContentRootPath, _logger, formFiles, dto.CATSID, _catsAppService, fileType, dto.Collaboration == null ? "New Letter" : "Collaboration");
            var result = await sPUploadDocuments.DocumentsProcessing(dto, dto.Collaboration == null);
            dto = result.Item1;
            List<Document> uploadedDocuments = result.Item2;
            
            // and Insert the uploaded documents info to Document table in SQL
            if (uploadedDocuments.Count > 0)
            {
                foreach (var doc in uploadedDocuments)
                {
                    _catsAppService.getDocumentAppService().Create(new DocumentDto()
                    {
                        FolderId = (int)dto.FolderId,
                        Name = doc.Name,
                        Path = doc.Path,
                        Type = doc.Type
                    });
                }
            }
            //Refresh the correspondence data
            var entity = _correspondenceRepository.Get(dto.Id);
            entity.FolderId = dto.FolderId;
            dto = ObjectMapper.Map<CorrespondenceDto>(entity);
            _correspondenceRepository.Update(entity);
            //_catsAppService.getCorrespondenceAppService().Update(dto);

            return dto;
        }
    }
}
