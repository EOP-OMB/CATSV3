
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Sharepoint;
using Mod.CatsV3.Sharepoint.Services;
using Mod.Framework.WebApi.Controllers;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class SurrogateOriginatorController : CrudControllerBase<SurrogateOriginatorDto, SurrogateOriginator>
    {
        ISurrogateOriginatorAppService _service;
        private readonly ILogger<SurrogateOriginator> _logger;
        ICorrespondenceAppService _correspondenceAppService;
        IWebHostEnvironment _webHostEnvironment;
        ICatsAppService _catsAppService;
        ICatsDocumentAppService _catsDocumentAppService;
        IServiceScopeFactory ServiceProvider;

        public SurrogateOriginatorController(ILogger<SurrogateOriginator> logger, ISurrogateOriginatorAppService service, ICorrespondenceAppService correspondenceAppService, IWebHostEnvironment environment, ICatsAppService catsAppService, ICatsDocumentAppService catsDocumentAppService, IServiceScopeFactory ServiceProvider) : base(logger, service)
        {
            _service = service;
            _logger = logger;
            _correspondenceAppService = correspondenceAppService;
            _webHostEnvironment = environment;
            _catsAppService = catsAppService;
            _catsDocumentAppService = catsDocumentAppService;
            this.ServiceProvider = ServiceProvider;
        }

        [HttpPost]
        public override ActionResult<SurrogateOriginatorDto> Create(SurrogateOriginatorDto dto)
        {
            try
            {
                if (dto.Id == 0)
                {
                    //check if exists. if not then insert or just update
                    var d = _service.GetAll(dto.CATSUserUPN,true);

                    if (d == null)
                    {
                        dto = Service.Create(dto);
                        Task.Run(() => SetSPPermissions(dto).ContinueWith((t) =>
                        {
                            //emailLogs.logTransactions(t, tempDto);
                        }));
                        return dto;
                    }
                    else
                    {
                        
                        if (d.Any(x => x.CATSUserUPN == dto.CATSUserUPN && x.SurrogateUPN == dto.SurrogateUPN))
                        {
                            dto.Id = d.Where(x => x.CATSUserUPN == dto.CATSUserUPN && x.SurrogateUPN == dto.SurrogateUPN).ToList().OrderByDescending(x => x.Id).FirstOrDefault().Id;
                            dto = Service.Update(dto);
                            Task.Run(() => SetSPPermissions(dto).ContinueWith((t) =>
                            {
                                //emailLogs.logTransactions(t, tempDto);
                            }));
                            return dto;
                        }
                        else
                        {
                            dto = Service.Create(dto);
                            Task.Run(() => SetSPPermissions(dto).ContinueWith((t) =>
                            {
                                //emailLogs.logTransactions(t, tempDto);
                            }));
                            return dto;
                        }
                        
                    }
                    
                }
                else
                {
                    dto = Service.Update(dto);
                    Task.Run(() => SetSPPermissions(dto).ContinueWith((t) =>
                    {
                        //emailLogs.logTransactions(t, tempDto);
                    }));
                    return dto;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            
            
        }

        [HttpGet]
        [Route("GetSurrogate")]
        public virtual ActionResult<SurrogateOriginatorDto> GetAll(string search)
        {
            //Get by the Search Key words
            var list = string.IsNullOrEmpty(search) ? null : _service.GetAll(search,false);

            return Json(list.Where(x => x.Surrogate != "" && x.SurrogateUPN != "" && x.CATSUser != "" && x.CATSUserUPN != "").OrderByDescending(x => x.CATSUser));
        }

        private async Task<string> SetSPPermissions(SurrogateOriginatorDto dto)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                //update permissions
                var correspondenceDtos = anotherService.getCorrespondenceAppService().GetAllForSurrogates(dto.CATSUserUPN, dto.SurrogateUPN, true).ToList();
                for (int i = 0; i <= correspondenceDtos.Count() - 1; i++)
                {
                    //Apply permission only
                    var correspondenceDto = correspondenceDtos.ToList()[i];
                    var fileType = "Review Document";
                    correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(new List<IFormFile>(), fileType, correspondenceDto.IsUnderReview == true ? "Collaboration" : "New Letter", correspondenceDto);
                }

                return "";
            }
        }
    }
}
