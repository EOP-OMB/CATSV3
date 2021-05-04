using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class ExternalUserAppService : CrudAppService<ExternalUserDto, ExternalUser>, IExternalUserAppService
    {
        public ExternalUserAppService(IExternalUserRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }
    }
}
