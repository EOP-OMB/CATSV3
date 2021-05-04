using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public interface IEmailNotificationLogAppService : ICrudAppService<EmailNotificationLogDto, EmailNotificationLog>
    {
        IEnumerable<EmailNotificationLogDto> GetAll(string search, string currentRound);
    }
}
