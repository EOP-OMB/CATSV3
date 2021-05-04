using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.WebApi.Controllers;
using Newtonsoft.Json;

namespace Mod.CatsV3.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        ICatsAppService _service;
        public AdminController(ILogger<CatsAppService> logger, ICatsAppService service) 
        {
            _service = service;
        }

        [HttpPost("addSupports")]
        public ActionResult addSupports()
        {
            try
            {
                var formdata = Request.Form;
                var members = formdata.Select(x => x).FirstOrDefault(x => x.Key == "members").Value;
                var roles = formdata.Select(x => x).FirstOrDefault(x => x.Key == "roles").Value;
                var offices = formdata.Select(x => x).FirstOrDefault(x => x.Key == "offices").Value;

                var LeadOfficeMembers = JsonConvert.DeserializeObject<LeadOfficeMemberDto[]>(members);
                var rolesIds = JsonConvert.DeserializeObject<int[]>(roles);
                var officeIds = JsonConvert.DeserializeObject<int[]>(offices);

                LeadOfficeMembers.ToList().ForEach(m =>
                {
                    officeIds.ToList().ForEach(id =>
                    {
                        rolesIds.ToList().ForEach(r => {
                            m.LeadOfficeId = id;
                            m.RoleId = r;
                            var d = _service.getLeadOfficeMemberAppService().Create(m);
                        });
                    });
                });

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }
        }

        [HttpPost("addcatsusers")]
        public ActionResult Post()
        {
            try
            {
                var formdata = Request.Form;               
                var members = formdata.Select(x => x).FirstOrDefault(x => x.Key == "members").Value;
                var managers = formdata.Select(x => x).FirstOrDefault(x => x.Key == "managers").Value;
                var offices = formdata.Select(x => x).FirstOrDefault(x => x.Key == "offices").Value;

                var LeadOfficeMembers = JsonConvert.DeserializeObject<LeadOfficeMemberDto[]>(members);
                var LeadOfficeManagers = JsonConvert.DeserializeObject<LeadOfficeOfficeManagerDto[]>(managers);
                var officeIds = JsonConvert.DeserializeObject<int[]>(offices);

                LeadOfficeMembers.ToList().ForEach(m =>
                {
                    officeIds.ToList().ForEach(id =>
                    {
                        m.LeadOfficeId = id;
                        m.RoleId = 4;
                        var d = _service.getLeadOfficeMemberAppService().Create(m);
                    });
                });

                LeadOfficeManagers.ToList().ForEach(m =>
                {
                    officeIds.ToList().ForEach(id =>
                    {
                        m.LeadOfficeId = id;
                        m.RoleId = 8;
                        var d =_service.getLeadOfficeOfficeManagerAppService().Create(m);
                    });
                });

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }
        }

        [HttpPost("addDlusers")]
        public ActionResult AddDlusers()
        {
            try
            {
                var formdata = Request.Form;
                var members = formdata.Select(x => x).FirstOrDefault(x => x.Key == "members").Value;
                var offices = formdata.Select(x => x).FirstOrDefault(x => x.Key == "dlIds").Value;

                var DlMembers = JsonConvert.DeserializeObject<DLGroupMembersDto[]>(members);
                var officeIds = JsonConvert.DeserializeObject<int[]>(offices);

                DlMembers.ToList().ForEach(m =>
                {
                    officeIds.ToList().ForEach(id =>
                    {
                        m.DLGroupId = id;
                        m.RoleId = 4;
                        var d = _service.getDLGroupMembersAppServicee().Create(m);
                    });
                });

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }
        }

        [HttpPost("removesupportusers")]
        [DisableRequestSizeLimit]
        public ActionResult DeleteSupportUsers()
        {
            try
            {
                var formdata = Request.Form;
                var upn = formdata.Select(x => x).FirstOrDefault(x => x.Key == "upn").Value;
                var role = formdata.Select(x => x).FirstOrDefault(x => x.Key == "roleId").Value;

                var userUpn = JsonConvert.DeserializeObject<string>(upn);
                var roleid = JsonConvert.DeserializeObject<int>(role);
                //leadmembers
                var items =_service.getLeadOfficeMemberAppService().GetAll(userUpn, roleid, true);
                items.ToList().ForEach(i => {
                    _service.getLeadOfficeMemberAppService().Delete(i.Id);
                });

                //leadmanagers
                var items2 = _service.getLeadOfficeOfficeManagerAppService().GetAll(userUpn, roleid, true);
                items2.ToList().ForEach(i => {
                    _service.getLeadOfficeOfficeManagerAppService().Delete(i.Id);
                });

                return Ok();
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }
        }

        [HttpPost("removecatsusers")]
        [DisableRequestSizeLimit]
        public ActionResult DeleteCatsUsers()
        {
            try
            {
                var formdata = Request.Form;
                var userId = formdata.Select(x => x).FirstOrDefault(x => x.Key == "userId").Value;
                var ismanager = formdata.Select(x => x).FirstOrDefault(x => x.Key == "ismanager").Value;
                var leadOfficeId = formdata.Select(x => x).FirstOrDefault(x => x.Key == "leadOfficeId").Value;

                var id = JsonConvert.DeserializeObject<int>(userId);
                var checkIfManager = JsonConvert.DeserializeObject<bool>(ismanager);
                var officeId = JsonConvert.DeserializeObject<int>(leadOfficeId);

                if (checkIfManager) {
                    //var m = _service.getLeadOfficeOfficeManagerAppService().Get(id);
                    //m.IsDeleted = true;
                    //_service.getLeadOfficeOfficeManagerAppService().Update(m);
                    _service.getLeadOfficeOfficeManagerAppService().Delete(id); 
                }
                else
                {
                    //var m = _service.getLeadOfficeMemberAppService().Get(id);
                    //m.IsDeleted = true;
                    //_service.getLeadOfficeMemberAppService().Update(m);
                    _service.getLeadOfficeMemberAppService().Delete(id);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }
        }

        [HttpPost("removecatDlsusers")]
        [DisableRequestSizeLimit]
        public ActionResult DeleteCatsDlUsers()
        {
            try
            {
                var formdata = Request.Form;
                var userId = formdata.Select(x => x).FirstOrDefault(x => x.Key == "userId").Value;
                var dlId = formdata.Select(x => x).FirstOrDefault(x => x.Key == "dlId").Value;

                var id = JsonConvert.DeserializeObject<int>(userId);
                var officeId = JsonConvert.DeserializeObject<int>(dlId);

                _service.getDLGroupMembersAppServicee().Delete(id);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException != null ? ex.InnerException : ex);
            }
        }
    }
}
