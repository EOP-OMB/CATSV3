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
    public class FYIUserAppService : CrudAppServiceCATS<FYIUserDto, FYIUser>, IFYIUserAppService
    {
        public FYIUserAppService(IFYIUserRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }

        public IEnumerable<FYIUserDto> GetAll(string filter, string round)
        {
            var entities = Repository.GetAll(x => (x.IsDeleted == false) && EF.Functions.Like(x.FYIUpn, "%" + filter + "%") && EF.Functions.Like(x.RoundName, "%" + round + "%"));
            return entities.Select(MapToDto).ToList();
        }
        IEnumerable<FYIUserDto> IFYIUserAppService.GetAll(string filter)
        {

            var entities = Repository.GetAll(x => (x.IsDeleted == false) && EF.Functions.Like(x.FYIUpn, "%" + filter + "%"));
            return entities.Select(MapToDto).ToList();


        }
        public IEnumerable<FYIUserDto> GetAll(dynamic item)
        {
            string catsid = item.CATSID;
            string roundname = item.Round;

            var entities = Repository.GetAll(x =>  x.Collaboration.Correspondence.CATSID == catsid && EF.Functions.Like(x.RoundName, "%" + roundname + "%"));
            return entities.Select(MapToDto).ToList();
        }
    }
}