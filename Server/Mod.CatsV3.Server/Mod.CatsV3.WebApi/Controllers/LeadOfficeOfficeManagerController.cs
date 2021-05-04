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
    public class LeadOfficeOfficeManagerController : CrudControllerBase<LeadOfficeOfficeManagerDto, LeadOfficeOfficeManager>
    {
        public LeadOfficeOfficeManagerController(ILogger<LeadOfficeOfficeManager> logger, ILeadOfficeOfficeManagerAppService service) : base(logger, service)
        {
        }
    }
}