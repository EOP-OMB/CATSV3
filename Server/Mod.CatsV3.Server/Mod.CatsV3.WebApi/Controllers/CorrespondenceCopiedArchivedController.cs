using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.WebApi.Controllers;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class CorrespondenceCopiedArchivedController : CrudControllerBase<CorrespondenceCopiedArchivedDto, CorrespondenceCopiedArchived>
    {
        ICorrespondenceCopiedArchivedAppService service;
        public CorrespondenceCopiedArchivedController(ILogger<CorrespondenceCopiedArchived> logger, ICorrespondenceCopiedArchivedAppService service) : base(logger, service)
        {
            this.service = service;
        }


        [HttpGet]
        [Route("filter")]
        public virtual ActionResult<CorrespondenceCopiedArchivedController> Get(string upn)
        {
            var list = this.service.GetBy(x => x.ArchivedUserUpn == upn && x.IsDeleted == false);
            return Json(list.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpPost]
        public override ActionResult<CorrespondenceCopiedArchivedDto> Create(CorrespondenceCopiedArchivedDto dto)
        {

            if (dto.Id > 0)
            {
                this.service.Update(dto);               
            }
            else
            {
                this.service.Create(dto);
            }

            

            return Ok(dto);
        }
    }
}
