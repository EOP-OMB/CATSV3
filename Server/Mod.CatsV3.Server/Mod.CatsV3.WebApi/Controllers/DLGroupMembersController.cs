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
    public class DLGroupMembersController : CrudControllerBase<DLGroupMembersDto, DLGroupMembers>
    {
        public DLGroupMembersController(ILogger<DLGroupMembers> logger, IDLGroupMembersAppService service) : base(logger, service)
        {
        }
    }
}
