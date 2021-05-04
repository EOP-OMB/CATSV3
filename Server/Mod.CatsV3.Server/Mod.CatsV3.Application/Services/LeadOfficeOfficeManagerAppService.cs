using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class LeadOfficeOfficeManagerAppService : CrudAppService<LeadOfficeOfficeManagerDto, LeadOfficeOfficeManager>, ILeadOfficeOfficeManagerAppService
    {
        public LeadOfficeOfficeManagerAppService(ILeadOfficeOfficeManagerRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {

        }
        public IEnumerable<LeadOfficeOfficeManagerDto> GetAll(string upn, int officeId)
        {
            var entities = Repository.GetAll(x => x.IsDeleted == false && (x.UserUPN == upn && x.LeadOfficeId == officeId || EF.Functions.Like(x.externalLeadOfficeIds, "%" + officeId.ToString() + "%")));
            
            return entities.Select(MapToDto).ToList();
        }
        public IEnumerable<LeadOfficeOfficeManagerDto> GetAll(string upn, int itemId, bool isByRole = false)
        {
            //itemId can be officeid or roleid
            List<LeadOfficeOfficeManager> entities;
            if (isByRole)
            {
                entities = Repository.GetAll(x => x.IsDeleted == false && (x.UserUPN == upn && x.RoleId == itemId));
            }
            else
            {
                entities = Repository.GetAll(x => x.IsDeleted == false && (x.UserUPN == upn && x.LeadOfficeId == itemId || EF.Functions.Like(x.externalLeadOfficeIds, "%" + itemId.ToString() + "%")));
            }
            return entities.Select(MapToDto).ToList();
        }
    }
}