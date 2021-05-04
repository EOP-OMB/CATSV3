using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class LeadOfficeAppService : CrudAppService<LeadOfficeDto, LeadOffice>, ILeadOfficeAppService
    {
        public LeadOfficeAppService(ILeadOfficeRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }

        public IEnumerable<LeadOfficeDto> GetAll(string officename)
        {
            var entities = Repository.GetAll(x => x.IsDeleted == false && x.Name.Trim() == officename);
            return entities.Select(MapToDto).ToList();
        }

        public IEnumerable<LeadOfficeDto> GetAllByUser(string upn, bool isLeadMemberManager = false)
        {
            //var entities = Repository.GetAll(x => x.IsDeleted == false && (isLeadMemberManager == false ? x.LeadOfficeMembers.Any(m => m.UserUPN == upn) : x.LeadOfficeOfficeManagers.Any(m => m.UserUPN == upn)));
            var entities = Repository.GetAll(x => x.IsDeleted == false && (x.LeadOfficeMembers.Any(m => EF.Functions.Like(m.UserUPN, "%" + upn.Trim() + "%")) || x.LeadOfficeOfficeManagers.Any(m => EF.Functions.Like(m.UserUPN, "%" + upn.Trim() + "%"))));
            return entities.Select(MapToDto).ToList(); 
        }
    }
}
