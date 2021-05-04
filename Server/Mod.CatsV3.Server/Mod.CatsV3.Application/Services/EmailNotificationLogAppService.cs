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
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class EmailNotificationLogAppService : CrudAppServiceCATS<EmailNotificationLogDto, EmailNotificationLog>, IEmailNotificationLogAppService
    {
        IEmailNotificationLogRepository _repository;
        public EmailNotificationLogAppService(IEmailNotificationLogRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
            _repository = repository;
        }

        IEnumerable<EmailNotificationLogDto> IEmailNotificationLogAppService.GetAll(string search, string  currentRound)
        {
            var entities = Repository.GetAll(x => (x.CATSID.Contains(search)  && EF.Functions.Like(x.CurrentRound, "%" + currentRound + "%") && x.IsError != true && x.IsCurrentRound == true));
            return entities.Select(MapToDto).ToList();
        }
    }
}