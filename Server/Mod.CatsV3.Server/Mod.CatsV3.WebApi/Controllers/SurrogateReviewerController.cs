
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
using Mod.Framework.WebApi.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Mod.CatsV3.Email.Models;
using Mod.CatsV3.Sharepoint;
using Mod.CatsV3.Sharepoint.Services;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class SurrogateReviewerController : CrudControllerBase<SurrogateReviewerDto, SurrogateReviewer>
    {
        ISurrogateReviewerAppService _service;
        private readonly ILogger<SurrogateReviewer> _logger;
        ICorrespondenceAppService _correspondenceAppService;
        IWebHostEnvironment _webHostEnvironment;
        ICatsAppService _catsAppService;
        ICatsDocumentAppService _catsDocumentAppService;
        IServiceScopeFactory ServiceProvider;
        public SurrogateReviewerController(ILogger<SurrogateReviewer> logger, ISurrogateReviewerAppService service, ICorrespondenceAppService correspondenceAppService, IWebHostEnvironment environment, ICatsAppService catsAppService, ICatsDocumentAppService catsDocumentAppService, IServiceScopeFactory ServiceProvider) : base(logger, service)
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
        public override ActionResult<SurrogateReviewerDto> Create(SurrogateReviewerDto dto)
        {
            try
            {
                if (dto.Id == 0)
                {
                    //check if exists. if not then insert or just update
                    var d = _service.GetAll(dto.CATSUserUPN, true);

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
            catch (Exception ex)
            {
                throw;
            }


        }

        [HttpPost("updatecreate")]
        [DisableRequestSizeLimit]
        public ActionResult<List<SurrogateReviewerDto>> Post(SurrogateReviewerDto dto)
        {
            try
            {
                if (dto.Id == 0)
                {
                    //check if exists. if not then insert or just update
                    var d = _service.GetAll(dto.CATSUserUPN, true);

                    if (d == null)
                    {
                        dto = Service.Create(dto);
                        Task.Run(() => SetSPPermissions(dto).ContinueWith((t) =>
                        {
                            //emailLogs.logTransactions(t, tempDto);
                        }));
                        return _service.GetAll().ToList();
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
                            return _service.GetAll().ToList();
                        }
                        else
                        {
                            dto = Service.Create(dto);
                            Task.Run(() => SetSPPermissions(dto).ContinueWith((t) =>
                            {
                                //emailLogs.logTransactions(t, tempDto);
                            }));
                            return _service.GetAll().ToList();
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
                    return _service.GetAll().ToList();
                }



            }
            catch (Exception ex)
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

            return Json(list.OrderByDescending(x => x.CATSUser));
        }

        private async Task<string> SetSPPermissions(SurrogateReviewerDto dto)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                //update permissions
                var correspondenceDtos = anotherService.getCorrespondenceAppService().GetAllForSurrogates(dto.CATSUserUPN, dto.SurrogateUPN).ToList();
                for (int i = 0; i <= correspondenceDtos.Count() - 1; i++)
                {
                    //Apply permission only
                    var correspondenceDto = correspondenceDtos.ToList()[i];
                    var fileType = "Review Document";
                    correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(new List<IFormFile>(), fileType, correspondenceDto.Collaboration == null ? "New Letter" : "Collaboration", correspondenceDto);
                }

                return "";
            }
        }
    }
}
