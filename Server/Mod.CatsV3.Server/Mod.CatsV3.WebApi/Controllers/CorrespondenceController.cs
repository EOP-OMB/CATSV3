using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Services;
using Mod.CatsV3.Email.Interface;
using Mod.CatsV3.Email.Models;
using Mod.CatsV3.Sharepoint;
using Mod.CatsV3.Sharepoint.Services;
using Mod.CATSV3.Logs;
using Mod.Framework.Configuration;
using Mod.Framework.WebApi.Controllers;
using Newtonsoft.Json;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class CorrespondenceController : CrudControllerBase<CorrespondenceDto, Correspondence>
    {
        private readonly IOptions<CATSEmailAPIConfiguration> _config;
        ICorrespondenceAppService _service;
        private readonly ILogger<Correspondence> _logger;
        private IWebHostEnvironment _webHostEnvironment;
        ICatsAppService _catsAppService;
        ICatsDocumentAppService _catsDocumentAppService;
        ICatsEmailAppService _catsEmailAppService;
        IServiceScopeFactory ServiceProvider;
        CurrentBrowserUser currentBrowserUser;
        EmailLogs emailLogs;

        int _CatsNotificationId = 0;
        public CorrespondenceController(
            IOptions<CATSEmailAPIConfiguration> catConfig,
            IWebHostEnvironment environment,
            ILogger<Correspondence> logger,
            ICorrespondenceAppService service, ICatsAppService catsAppService, ICatsEmailAppService catsEmailAppService, ICatsDocumentAppService catsDocumentAppService, IServiceScopeFactory ServiceProvider) : base(logger, service)
        {
            _config = catConfig;
            _service = service;
            _catsAppService = catsAppService;
            _catsDocumentAppService = catsDocumentAppService;
            _catsEmailAppService = catsEmailAppService;
            _logger = logger;
            _webHostEnvironment = environment;
            this.ServiceProvider = ServiceProvider;
            this.emailLogs = new EmailLogs(this.ServiceProvider);
        }


        [HttpGet("{id}")]
        public override ActionResult<CorrespondenceDto> Get(int id)
        {
            var list = _service.GetById(id);

            return Json(list);
        }

        [HttpGet]
        [Route("filter")]
        public virtual ActionResult<CorrespondenceDto> Get(string filter, string search)
        {
            var list = _service.GetAll(filter, search); //base.Service.GetAll(); 
           
            return Json(list.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpGet]
        [Route("filterpagination")]
        public virtual ActionResult<Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>> Get(string filter, string search, int offset, int limit)
        {
            var list = _service.GetAllWithPagination(filter, search, offset, limit);  
            return Json(list);
            //return Json(list.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }



        [HttpGet]
        [Route("cdashboard")]
        public virtual ActionResult<Page<CorrespondenceDto>> cdashboard(string filter, int pageNumber, int pageSize, string sortOrder, string sortProperty, string search, string registration)
        {
            PaginationType<CorrespondenceDto> paginationType;
            if(filter != "Deleted")
            {
                paginationType = _service.GetAllWithPagination(filter, search, pageNumber, pageSize, sortProperty, sortOrder);
            }
            else
            {
                paginationType = _service.GetAllDeletedWithPagination(filter, search, pageNumber, pageSize, sortProperty, sortOrder);
            }

            Page<CorrespondenceDto> page = new Page<CorrespondenceDto>(paginationType.data, paginationType.total, pageSize, pageNumber);
            return Json(page);
            //return Json(list.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpGet]
        [Route("odashboard")]
        public virtual ActionResult<Page<CorrespondenceDto>> odashboard(string roles, string dataoption, string offices, string userid, string officemanageroffices, string filter, int pageNumber, int pageSize, string sortOrder, string sortProperty, string search, string registration)
        {
            CollaborationParam collaborationParam = new CollaborationParam();
            collaborationParam.dataOption = dataoption == null ? "" : dataoption;
            collaborationParam.roles = roles == null ? "" : roles;
            collaborationParam.userlogin = userid == null ? "" : userid.Replace("i:0e.t|adfs|","");
            collaborationParam.filter = filter == null ? filter : filter;
            collaborationParam.offices = offices == null ? "" : offices;
            collaborationParam.officemMnagerOffices = officemanageroffices == null ? "" : officemanageroffices;
            collaborationParam.dlgroups = "";
            collaborationParam.pageNumber = pageNumber;
            collaborationParam.pageSize = pageSize;
            collaborationParam.registration = registration;
            collaborationParam.search = search == null ? "" : search;
            collaborationParam.size = pageSize;
            collaborationParam.sortOrder = sortOrder == null ? "desc" : sortOrder;
            collaborationParam.sortProperty = sortProperty == null ? "" : sortProperty;

            //get all the DLs or Offices for the current user
            if (collaborationParam.dataOption != "reviewer")
            {
                var myoffices = _catsAppService.getLeadOfficeAppService().GetAllByUser(collaborationParam.userlogin).Select(o => o.Name).ToArray();
                collaborationParam.offices = string.Join(",", myoffices);
            }
            else
            {
                collaborationParam.dlgroups = string.Join(",", _catsAppService.getDLGroupAppService().GetAll(collaborationParam.userlogin).ToList().Select(dl => dl.Name).ToList());
            }            

            PaginationType<CorrespondenceDto> paginationType;

            paginationType = _service.GetAllWithPagination(collaborationParam);

            Page<CorrespondenceDto> page = new Page<CorrespondenceDto>(paginationType.data, paginationType.total, pageSize, pageNumber);
            return Json(page); 
            //return Json(list.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpGet]
        [Route("rdashboard")]
        public virtual ActionResult<Page<CorrespondenceDto>> rdashboard(string filter, int pageNumber, int pageSize, string sortOrder, string sortProperty, string search, string registration)
        {
            var list = _service.GetAllWithPagination(filter, search, pageNumber, pageSize, sortProperty, sortOrder);
            Page<CorrespondenceDto> page = new Page<CorrespondenceDto>(list.data, list.total, pageSize, pageNumber);
            return Json(page);
            //return Json(list.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpGet]
        [Route("GetOriginatorData")]
        public virtual ActionResult<CorrespondenceDto> GetAll(string filter, string search, string offices)
        {
            //Get by the Search Key words
            var listBySearch = _service.GetAll(filter, search, offices.Split(";"));
            
            return Json(listBySearch.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpPost("restore")]
        public ActionResult<CorrespondenceDto> Restore()
        {
            var formdata = Request.Form;
            var correspondence = formdata.Select(x => x).FirstOrDefault(x => x.Key == "correspondence").Value;
            var dto = JsonConvert.DeserializeObject<CorrespondenceDto>(correspondence);

            return _service.RestoreDeleted(dto);
        }

        [HttpPost("update")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> Post()
        {
            CorrespondenceDto tempDto = new CorrespondenceDto();
            try
            {

                var formdata = Request.Form;

                var correspondence = formdata.Select(x => x).FirstOrDefault(x => x.Key == "correspondence").Value;
                var dto = JsonConvert.DeserializeObject<CorrespondenceDto>(correspondence);
                string fileType = dto.IsFinalDocument == true ? "Final Document" : "Reference Document";

                //send the attached email archived
                if (dto.CatsNotificationId == 10 && formdata.Files.Count > 0)
                {
                    Task.Run(() => _catsEmailAppService.SenArchivedEmailToMailbox(HttpContext)).ContinueWith((t) =>
                    {
                        emailLogs.logTransactions(t, dto);
                    });
                }

                string currentUserUPN = dto.CurrentUserUPN;

                currentBrowserUser = new CurrentBrowserUser()
                {
                    UserUpn = dto.CurrentUserUPN,
                    Email = dto.CurrentUserEmail,//employees.Where(e => e.Upn == upn).FirstOrDefault().EmailAddress,// SetUsersEmailsFromAD(new List<string>() { upn }).FirstOrDefault(),//LoggedUser.AdUserInfo.OfficeEmail,
                    UserFullName = dto.CurrentUserFullName,//employees.Where(e => e.Upn == upn).FirstOrDefault().DisplayName//LoggedUser.AdUserInfo.DisplayName
                    PreferredName = dto.CurrentUserFullName
                };

                dto.IsReopen = !dto.IsReopen.HasValue ? false : dto.IsReopen;
                if ((bool)dto.Rejected)
                {
                    _CatsNotificationId = 2;
                }
                else if (dto.LetterStatus.Contains("Closed"))
                {
                    _CatsNotificationId = 10;
                }
                else
                {
                    _CatsNotificationId = 1;
                }

                tempDto = dto;

                if (dto.Id == 0)
                {
                    try
                    {
                        dto = await CreateNewCorrespondence(formdata.Files.ToList(), dto, fileType);
                    }
                    catch (Exception ex)
                    {
                        emailLogs.logTransactions(null, tempDto, ex);
                        return BadRequest(ex.InnerException);

                    }
                }
                else
                {

                    //update the record in SQL
                    dto = _service.UpdateWithCollaboration(dto);
                    //Apply permission and eventually upload documents
                    dto = await _catsDocumentAppService.uploadDocumentsAsync(formdata.Files.ToList(), fileType, dto.Collaboration == null ? "New Letter" : "Collaboration", dto);
                }

                tempDto = dto;
                dto.CatsNotificationId = _CatsNotificationId;
                //Send email only when submitting
                if ((bool)dto.IsEmailElligible == true)
                {
                    Task.Run(() => _catsEmailAppService.sendEmailnotificationAsync(dto, currentUserUPN, currentBrowserUser, formdata.Files.ToList())).ContinueWith((t) =>
                    {
                        emailLogs.logTransactions(t, tempDto);
                    });                    

                    return Ok(dto);
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, tempDto, ex); ;
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }


        }

        [HttpPost("archiveEmailAttachment")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> ArchiveEmailAttachment()
        {
            CorrespondenceDto tempDto = new CorrespondenceDto();
            try
            {
                var formdata = Request.Form;

                var correspondence = formdata.Select(x => x).FirstOrDefault(x => x.Key == "correspondence").Value;
                var dto = JsonConvert.DeserializeObject<CorrespondenceDto>(correspondence);

                string currentUserUPN = dto.CurrentUserUPN;

                currentBrowserUser = new CurrentBrowserUser()
                {
                    UserUpn = dto.CurrentUserUPN,
                    Email = dto.CurrentUserEmail,//employees.Where(e => e.Upn == upn).FirstOrDefault().EmailAddress,// SetUsersEmailsFromAD(new List<string>() { upn }).FirstOrDefault(),//LoggedUser.AdUserInfo.OfficeEmail,
                    UserFullName = dto.CurrentUserFullName,//employees.Where(e => e.Upn == upn).FirstOrDefault().DisplayName//LoggedUser.AdUserInfo.DisplayName
                    PreferredName = dto.CurrentUserFullName
                };

                _catsEmailAppService.sendEmailnotificationAsync(dto, currentBrowserUser, formdata.Files.ToList());

                return Ok(dto);
            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, tempDto, ex); ;
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }


        }

        [HttpPost]
        public override ActionResult<CorrespondenceDto> Create(CorrespondenceDto dto)
        {
            string currentUserUPN = dto.CurrentUserUPN;

            currentBrowserUser = new CurrentBrowserUser()
            {
                UserUpn = dto.CurrentUserUPN,
                Email = dto.CurrentUserEmail,//employees.Where(e => e.Upn == upn).FirstOrDefault().EmailAddress,// SetUsersEmailsFromAD(new List<string>() { upn }).FirstOrDefault(),//LoggedUser.AdUserInfo.OfficeEmail,
                UserFullName = dto.CurrentUserFullName,//employees.Where(e => e.Upn == upn).FirstOrDefault().DisplayName//LoggedUser.AdUserInfo.DisplayName
                PreferredName = dto.CurrentUserFullName
            };

            bool isEmailElligible = (bool)dto.IsEmailElligible;
            if (dto.Id == 0) {
                dto = Service.Create(dto);
            }
            else
            {
                dto = Service.Update(dto);
            }

            if (isEmailElligible)
            {
                _catsEmailAppService.sendEmailnotificationAsync(dto, currentUserUPN, currentBrowserUser, null);
            }
            

            ////wait for two senconds to allow the post insert/update sql triggers to run
            //System.Threading.Thread.Sleep(2000);
            //dto = Service.Get(dto.Id);

            return Ok(dto);
        }

        public async Task<CorrespondenceDto> CreateNewCorrespondence(List<IFormFile> files, CorrespondenceDto dto, string fileType)
        {
            //create new record in SQL first
            dto.FolderId = null;
            bool isFinalDocument = (bool)dto.IsFinalDocument;
            CorrespondenceDto newLetter = Service.Create(dto);
            dto = Service.Get(newLetter.Id);
            dto.IsFinalDocument = isFinalDocument;

            //Apply permission and eventually upload documents
            dto = await _catsDocumentAppService.uploadDocumentsAsync(files, fileType, dto.Collaboration == null ? "New Letter" : "Collaboration", dto);

            //update to make sure all fields such as folderid are inserted
            dto = Service.Update(dto);
            return dto;
        }

        private void setCopiedArchived(CorrespondenceDto dto)
        {

            if (dto.CorrespondenceCopiedArchiveds != null)
            {
                var deleted = dto.CorrespondenceCopiedArchiveds.Where(x => x.IsDeleted == true && x.Id > 0).FirstOrDefault();
                var created = dto.CorrespondenceCopiedArchiveds.Where(x => x.Id == 0).FirstOrDefault();

                if (deleted != null)
                {
                    deleted.CorrespondenceId = dto.Id;
                    _catsAppService.getCorrespondenceCopiedArchivedAppService().Delete(deleted);
                }
                else if (created != null)
                {
                    _catsAppService.getCorrespondenceCopiedArchivedAppService().Create(created);
                }
            }
        }
    }
}
