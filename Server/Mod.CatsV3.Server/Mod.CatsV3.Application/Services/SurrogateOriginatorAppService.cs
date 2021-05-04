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
    public class SurrogateOriginatorAppService : CrudAppServiceCATS<SurrogateOriginatorDto, SurrogateOriginator>, ISurrogateOriginatorAppService
    {
        ISurrogateOriginatorRepository _repository;
        public SurrogateOriginatorAppService(ISurrogateOriginatorRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
            _repository = repository;
        }

        IEnumerable<SurrogateOriginatorDto> ISurrogateOriginatorAppService.GetAll(string search, bool isDeleteIncluded)
        {
            if (!isDeleteIncluded)
            {
                var entities = Repository.GetAll(x => (x.IsDeleted == false) &&
                       (
                            EF.Functions.Like(x.CATSUserUPN, "%" + search + "%") ||
                            EF.Functions.Like(x.SurrogateUPN, "%" + search + "%")
                        )
                    );
                return entities.Select(MapToDto).ToList();
            }
            else
            {
                var entities = Repository.GetAll(x =>
                        EF.Functions.Like(x.CATSUserUPN, "%" + search + "%") ||
                        EF.Functions.Like(x.SurrogateUPN, "%" + search + "%") 
                    );
                return entities.Select(MapToDto).ToList();
            }

        }

    }
}
