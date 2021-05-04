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
    public class DLGroupAppService : CrudAppService<DLGroupDto, DLGroup>, IDLGroupAppService
    {
        public DLGroupAppService(IDLGroupRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }
        public IEnumerable<DLGroupDto> GetAll(string[] filters)
        {
            var predicate = PredicateBuilder.False<DLGroup>();
            foreach (var filter in filters)
            {
                predicate = predicate.Or(x => x.Name == filter);
            }

            var entities = Repository.GetAll(predicate);
            return entities.Select(MapToDto).ToList();
        }

        public IEnumerable<DLGroupDto> GetAll(string userUpn)
        {
            var entities = Repository.GetAll(x => x.IsDeleted == false && x.DLGroupMembers.Where(m => m.UserUPN.Contains(userUpn)).Count() > 0);
            return entities.Select(MapToDto).ToList();
        }
    }
}