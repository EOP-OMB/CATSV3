
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

namespace Mod.CatsV3.WebApi.Controllers
{
    public class ExternalUserController : CrudControllerBase<ExternalUserDto, ExternalUser>
    {
        public ExternalUserController(ILogger<ExternalUser> logger, IExternalUserAppService service) : base(logger, service)
        {
        }
    }
}
