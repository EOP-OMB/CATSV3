
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class FYIUserController : CrudControllerBase<FYIUserDto, FYIUser>
    {
        IFYIUserAppService _service;
        public FYIUserController(ILogger<FYIUser> logger, IFYIUserAppService service) : base(logger, service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("filterbystatus")]
        public virtual ActionResult<FYIUserDto> GetCompletedItems(string catsid, string status, string round)
        {
            dynamic item = new ExpandoObject();
            item.CATSID = catsid;
            item.Round = round;

            var list = (IEnumerable<FYIUserDto>)_service.GetAll(item);

            //list.ToList().ForEach(x =>
            //{
            //    x.Collaboration.Originators.ToList().ForEach(d => { d.Collaboration = null; });
            //    x.Collaboration.Reviewers.ToList().ForEach(d => { d.Collaboration = null; });
            //    x.Collaboration.fYIUsers.ToList().ForEach(d => { d.Collaboration = null; });
            //});

            //group by the user upn
            var groups = list
                .GroupBy(a => (a.FYIUpn))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

            return Json(groups.OrderBy(x => x.Id));
        }
    }
}
