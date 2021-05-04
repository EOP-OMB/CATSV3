using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Email.Interface;
using Mod.CatsV3.Email.Models;
using Mod.CatsV3.Sharepoint.Services;
using Mod.Framework.WebApi.Controllers;
using Newtonsoft.Json;

namespace Mod.CatsV3.WebApi.Controllers
{
    public class OriginatorController : CrudControllerBase<OriginatorDto, Originator>
    {
        private readonly IOptions<CATSEmailAPIConfiguration> _config;
        ICatsAppService _catsAppService;
        IOriginatorAppService _service;
        IMapper _mapper;

        ILogger<Originator> _logger;
        IWebHostEnvironment _webHostEnvironment;
        ICatsDocumentAppService _catsDocumentAppService;
        CorrespondenceController _correspondenceController;
        public OriginatorController(
            ICatsAppService catsAppService,
            ICatsEmailAppService catsEmailAppService,
            IOptions<CATSEmailAPIConfiguration> catConfig,
            ILogger<Originator> logger,
            ILogger<Correspondence> loggerCorrespondence,
            IOriginatorAppService service,
            ICatsDocumentAppService catsDocumentAppService,
            IWebHostEnvironment environment,
            IMapper mapper
            ) : base(logger,  service)
        {
            _config = catConfig;
            _catsAppService = catsAppService;
            _catsDocumentAppService = catsDocumentAppService;
            _service = service;
            _logger = logger;
            _webHostEnvironment = environment;
            _mapper = mapper;

            _correspondenceController = new CorrespondenceController(catConfig, environment, loggerCorrespondence, _catsAppService.getCorrespondenceAppService(), _catsAppService, catsEmailAppService, _catsDocumentAppService, null);
        }



        [HttpGet]
        [Route("filterbystatus")]
        public virtual ActionResult<OriginatorDto> GetCompletedItems(string catsid, string status, string round)
        {
            dynamic item = new ExpandoObject();
            item.CATSID = catsid;
            item.Round = round;

            var list = (IEnumerable<OriginatorDto>)_service.GetAll(item);

            //list.ToList().ForEach(x =>
            //{
            //    x.Collaboration.Originators.ToList().ForEach(d => { d.Collaboration = null; });
            //    x.Collaboration.Reviewers.ToList().ForEach(d => { d.Collaboration = null; });
            //    x.Collaboration.fYIUsers.ToList().ForEach(d => { d.Collaboration = null; });
            //});

            //group by the user upn
            var groups = list
                .GroupBy(a => (a.OriginatorUpn))
                .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

            return Json(groups.OrderBy(x => x.Id));
        }


    }
} 