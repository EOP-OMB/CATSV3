using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Email.Interface;
using Mod.CatsV3.Email.Models;
using Mod.CatsV3.Sharepoint.Services;
using Mod.Framework.WebApi.Controllers;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class ReviewerController : CrudControllerBase<ReviewerDto, Reviewer>
    {
        IOptions<CATSEmailAPIConfiguration> _config;
        ICatsAppService _catsAppService;
        IReviewerAppService _service;
        ICatsDocumentAppService _catsDocumentAppService;
        ILogger<Reviewer> _logger;

        CorrespondenceController _correspondenceController;
        IServiceScopeFactory ServiceProvider;

        public ReviewerController(
            ILogger<Reviewer> logger,
            ICatsAppService catsAppService,
            ICatsEmailAppService catsEmailAppService,
            IReviewerAppService service,
            IOptions<CATSEmailAPIConfiguration> catConfig,
            ILogger<Correspondence> loggerCorrespondence,
            ICatsDocumentAppService catsDocumentAppService,
            IWebHostEnvironment environment,
            IServiceScopeFactory ServiceProvider
            ) : base(logger, service)
        {
            _config = catConfig;
            _service = service;
            _catsAppService = catsAppService;
            _logger = logger;
            this.ServiceProvider = ServiceProvider;
            _catsDocumentAppService = catsDocumentAppService;
            _correspondenceController = new CorrespondenceController(catConfig, environment, loggerCorrespondence, _catsAppService.getCorrespondenceAppService(), _catsAppService, catsEmailAppService, catsDocumentAppService, this.ServiceProvider);
        }

        [HttpGet]
        [Route("filter")]
        public virtual ActionResult<ReviewerDto> Get(string upn, string option, string round)
        {
            upn = string.IsNullOrEmpty(upn) ? "" : upn;
            round = string.IsNullOrEmpty(round) ? "" : round;
            option = string.IsNullOrEmpty(option) ? "" : option;

            //get all the DLs for this user
            var userDlGroups = _catsAppService.getDLGroupAppService().GetAll(upn).ToList().Select(dl => dl.Name).ToList();

            //get all the records based on the user upn and the DL groups in which the user belong
            var list = option != "" ? _service.GetAll(upn, userDlGroups, option, round) : _service.GetAll(upn, round); //base.Service.GetAll(); 

            //group by round and get the latest for each
            var groups = list
                .Where(a => a.CollaborationId != null)
                //.Where(a => a.Collaboration.Correspondence != null)
                //.Where(a => string.IsNullOrEmpty(a.Collaboration.Correspondence.CATSID) == false)
                .GroupBy(a => (a.CollaborationId))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();


            return Json(groups.OrderByDescending(x => x.CreatedTime).ThenByDescending(x => x.ModifiedTime));
        }

        [HttpGet]
        [Route("filterbystatus")]
        public virtual ActionResult<ReviewerDto> GetCompletedItems(string catsid,string status, string round)
        {
            dynamic item = new ExpandoObject();
            item.CATSID = catsid != null ? catsid : "";
            item.Status = status != null ?  status : "";
            item.Round = round != null ? round : "";

            var list = (IEnumerable<ReviewerDto>)_service.GetAll(item) ;

            //group by the user upn
            var groups = list
                .GroupBy(a => (a.ReviewerUPN))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

            return Json(groups.OrderBy(x => x.Id));
        }
    }
}
