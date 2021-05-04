using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.User.Entities;
using Mod.Framework.WebApi.Controllers;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class EmployeeController : CrudControllerBase<EmployeeDto, Employee>
    {
        new readonly IEmployeeAppService Service;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeAppService service) : base(logger, service)
        {
            Service = service;
        }

        [HttpGet("MyPortfolio")]
        public virtual ActionResult<EmployeeDto> MyPortfolio()
        {
            return Ok();
        }

        [HttpGet("GetByDepartment/{id}")]
        public virtual ActionResult<List<EmployeeDto>> GetByDepartment(int id)
        {
            var list = Service.GetByDepartment(id);

            return Json(list);
        }
    }
}