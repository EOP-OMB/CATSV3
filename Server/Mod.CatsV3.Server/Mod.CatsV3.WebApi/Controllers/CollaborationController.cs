
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Email.Interface;
using Mod.CatsV3.Email.Models;
using Mod.CatsV3.Sharepoint;
using Mod.CatsV3.Sharepoint.Services;
using Mod.CATSV3.Logs;
using Mod.Framework.WebApi.Controllers;
using Newtonsoft.Json;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class CollaborationController : CrudControllerBase<CollaborationDto, Collaboration>
    {
        private readonly IOptions<CATSEmailAPIConfiguration> _config;
        
        ILogger<Correspondence> _logger;
        IWebHostEnvironment _webHostEnvironment;
        ICollaborationAppService _service;
        CorrespondenceController _correspondenceController;
        ICatsAppService _catsAppService;
        ICatsEmailAppService _catsEmailAppService;
        ICatsDocumentAppService _catsDocumentAppService;
        CurrentBrowserUser currentBrowserUser;
        int _CatsNotificationId = 0;
        IServiceScopeFactory ServiceProvider;
        EmailLogs emailLogs;

        public CollaborationController(ILogger<Collaboration> logger, ICollaborationAppService service,
            IOptions<CATSEmailAPIConfiguration> catConfig,
            ILogger<Correspondence> loggerCorrespondence,
            IWebHostEnvironment environment,
            ICatsAppService catsAppService, ICatsEmailAppService catsEmailAppService, ICatsDocumentAppService catsDocumentAppService, IServiceScopeFactory ServiceProvider) : base(logger, service)
        {
            _config = catConfig;
            _service = service;
            _catsAppService = catsAppService;
            _catsEmailAppService = catsEmailAppService;
            _catsDocumentAppService = catsDocumentAppService;
            _webHostEnvironment = environment;
            this.ServiceProvider = ServiceProvider;
            this.emailLogs = new EmailLogs(this.ServiceProvider);
            _correspondenceController = new CorrespondenceController(catConfig, environment, loggerCorrespondence, _catsAppService.getCorrespondenceAppService(), _catsAppService, catsEmailAppService, _catsDocumentAppService, this.ServiceProvider);

        }

        private static CollaborationDto getTheMostCurrent(CollaborationDto collaboration)
        {
            collaboration.Originators = collaboration.Originators
                .GroupBy(a => (a.OriginatorUpn))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

            collaboration.Reviewers = collaboration.Reviewers
                .GroupBy(a => (a.ReviewerUPN))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

            collaboration.fYIUsers = collaboration.fYIUsers
                .GroupBy(a => (a.FYIUpn))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

            return collaboration;
        }

        [HttpGet("{id}")]
        public override ActionResult<CollaborationDto> Get(int id)
        {
            var list = Service.Get(id);

            return Json(list);
        }

        [HttpGet]
        [Route("getCollaborations")]
        public virtual ActionResult<CollaborationDto> getCollaborations(string filter, string search, string offices)
        {
            filter = string.IsNullOrEmpty(filter) ? "" : filter;
            search = string.IsNullOrEmpty(search) ? "" : search;
            offices = string.IsNullOrEmpty(offices) ? "" : offices;
            //Get by the Search Key words
            var listBySearch = _service.GetAll(filter, search, offices.Split(";"), false);

            return Json(listBySearch.OrderByDescending(x => x.Id));
        }


        [HttpGet]
        [Route("getCorrespondence")]
        public virtual ActionResult<CollaborationDto> getCorrespondence(int correspondenceId)
        {
            //Get by the Search Key words
            var collaboration = _service.GetCorrespondence(correspondenceId).FirstOrDefault();

            return Json(collaboration);
        }

        [HttpGet]
        [Route("getPendingsAndCopies")]
        public virtual ActionResult<CollaborationDto> GetAllPendings(string filter, string search, string offices)
        {
            filter = string.IsNullOrEmpty(filter) ? "" : filter;
            search = string.IsNullOrEmpty(search) ? "" : search;
            offices = string.IsNullOrEmpty(offices) ? "" : offices;
            //Get by the Search Key words
            var listBySearch = _service.GetAll(filter, search, offices.Split(";"));

            return Json(listBySearch.OrderByDescending(x => x.Id));
        }


        [HttpPost("update")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> Post()
        {
            CorrespondenceDto tempDto = new CorrespondenceDto();
            try
            {

                var formdata = Request.Form;

                var reviewDocuments = formdata.Files.Count() > 0 ? formdata.Files.Where(f => f.Name.Contains("review")) : Array.Empty<FormFile>();
                var referenceDocuments = formdata.Files.Count() > 0 ? formdata.Files.Where(f => f.Name.Contains("reference")) : Array.Empty<FormFile>();
                var finalDocuments = formdata.Files.Count() > 0 ? formdata.Files.Where(f => f.Name.Contains("final")) : Array.Empty<FormFile>();

                var correspondence = formdata.Select(x => x).FirstOrDefault(x => x.Key == "correspondence").Value;
                var collaboration = formdata.Select(x => x).FirstOrDefault(x => x.Key == "collaboration").Value;
                var originators = formdata.Select(x => x).FirstOrDefault(x => x.Key == "originators").Value;
                var reviewers = formdata.Select(x => x).FirstOrDefault(x => x.Key == "reviewers").Value;
                var fyiusers = formdata.Select(x => x).FirstOrDefault(x => x.Key == "fyiusers").Value;

                var correspondenceDto = JsonConvert.DeserializeObject<CorrespondenceDto>(correspondence);
                var collaborationDto = JsonConvert.DeserializeObject<CollaborationDto>(collaboration);
                var originatorDto = JsonConvert.DeserializeObject<OriginatorDto[]>(originators);
                var reviewerDto = JsonConvert.DeserializeObject<ReviewerDto[]>(reviewers);
                var fyiuserDto = JsonConvert.DeserializeObject<FYIUserDto[]>(fyiusers);

                string fileType = "";// correspondenceDto.IsFinalDocument == true ? "Final Document" : "Reference Document";
                string currentUserUPN = correspondenceDto.CurrentUserUPN;

                //send the attached email archived
                if (correspondenceDto.CatsNotificationId == 10 && formdata.Files.Count > 0)
                {
                    Task.Run(() => _catsEmailAppService.SenArchivedEmailToMailbox(HttpContext)).ContinueWith((t) =>
                    {
                        emailLogs.logTransactions(t, correspondenceDto);
                    });
                }

                currentBrowserUser = new CurrentBrowserUser()
                {
                    UserUpn = correspondenceDto.CurrentUserUPN,
                    Email = correspondenceDto.CurrentUserEmail,
                    UserFullName = correspondenceDto.CurrentUserFullName,
                    PreferredName = correspondenceDto.CurrentUserFullName
                };


                tempDto = correspondenceDto;
                correspondenceDto.IsReopen = !correspondenceDto.IsReopen.HasValue ? false : correspondenceDto.IsReopen;
                _CatsNotificationId = correspondenceDto.CatsNotificationId;

                if (correspondenceDto.Id == 0)
                {
                    try
                    {
                        //create new correspondence first and upload the review document
                        fileType = "Review Document";

                        //create new correspondence and upload review documents
                        correspondenceDto = await _correspondenceController.CreateNewCorrespondence(reviewDocuments.ToList(), correspondenceDto, fileType);
                        correspondenceDto = _catsAppService.getCorrespondenceAppService().Get(correspondenceDto.Id);

                        //create collaboration
                        collaborationDto.CorrespondenceId = correspondenceDto.Id;
                        collaborationDto.CATSID = correspondenceDto.CATSID;
                        collaborationDto = _catsAppService.getCollaborationAppService().Create(collaborationDto);

                        //Add Originators/Reviewers/FYIUsers //update the correspondence
                        collaborationDto.Originators = originatorDto;
                        collaborationDto.Reviewers = reviewerDto;
                        collaborationDto.fYIUsers = fyiuserDto;
                        correspondenceDto.Collaboration = collaborationDto;
                        correspondenceDto = _catsAppService.getCorrespondenceAppService().UpdateWithCollaboration(correspondenceDto);

                        //refresh the letter
                        correspondenceDto = _catsAppService.getCorrespondenceAppService().Get(correspondenceDto.Id);

                        //upload reference document if any
                        if (referenceDocuments.Count() > 0)
                        {
                            //Apply permission and eventually upload documents
                            fileType = "Reference Document";
                            correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "New Letter", correspondenceDto);
                            correspondenceDto = _catsAppService.getCorrespondenceAppService().Get(correspondenceDto.Id);
                        }
                        correspondenceDto.CatsNotificationId = _CatsNotificationId;
                    }
                    catch (Exception ex)
                    {
                        emailLogs.logTransactions(null, tempDto, ex);
                        return BadRequest(ex.InnerException);
                    }
                }
                else
                {
                    if (correspondenceDto.CatsNotificationId != 14) // 14 means sending only review reminder
                    {

                        //CREATE COLLABORATION FOR PENDING RECORDS: collaborationDto.iD = 0
                        if (collaborationDto.Id == 0)
                        {
                            collaborationDto.CorrespondenceId = correspondenceDto.Id;
                            collaborationDto.CATSID = correspondenceDto.CATSID;
                            collaborationDto = _catsAppService.getCollaborationAppService().Create(collaborationDto);
                        }

                        //update the correspondence
                        collaborationDto.Originators= originatorDto;
                        collaborationDto.Reviewers = reviewerDto;
                        collaborationDto.fYIUsers = fyiuserDto;
                        correspondenceDto.Collaboration = collaborationDto;
                        correspondenceDto = _catsAppService.getCorrespondenceAppService().UpdateWithCollaboration(correspondenceDto);

                        if (reviewDocuments.ToList().Count == 0 && referenceDocuments.ToList().Count == 0 && finalDocuments.ToList().Count == 0)
                        {
                            //Apply permission only
                            fileType = "Review Document";
                            correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "Collaboration", correspondenceDto);
                        }
                        else
                        {

                            //upload files if any
                            //Apply permission and eventually upload documents
                            if (reviewDocuments.ToList().Count > 0)
                            {
                                fileType = "Review Document";
                                correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "Collaboration", correspondenceDto);
                            }

                            //Apply permission and eventually upload documents
                            if (referenceDocuments.ToList().Count > 0)
                            {
                                fileType = "Reference Document";
                                correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "Collaboration", correspondenceDto);
                            }

                            //Apply permission and eventually upload documents
                            if (finalDocuments.ToList().Count > 0)
                            {
                                fileType = "Final Document";
                                correspondenceDto = await _catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, "Collaboration", correspondenceDto);
                            }
                        }

                        correspondenceDto.CatsNotificationId = _CatsNotificationId;
                    }

                }

                correspondenceDto.CatsNotificationId = _CatsNotificationId;

                //Send email only when submitting
                if ((bool)correspondenceDto.IsEmailElligible == true)
                {
                    Task.Run(() => _catsEmailAppService.sendEmailnotificationAsync(correspondenceDto, collaborationDto, originatorDto, reviewerDto, fyiuserDto, currentBrowserUser, formdata.Files.ToList())).ContinueWith((t) =>
                    {
                        emailLogs.logTransactions(t, correspondenceDto);
                    }); 

                    return Ok(correspondenceDto);
                    //sendEmailnotificationAsync(correspondenceDto, collaborationDto, originatorDto, reviewerDto, fyiuserDto, currentUserUPN);
                }

                return Ok(correspondenceDto);
            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, tempDto, ex);
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }


        }
    }
}
