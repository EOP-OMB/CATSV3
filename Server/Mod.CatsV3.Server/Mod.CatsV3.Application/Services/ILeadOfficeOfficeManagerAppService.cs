using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public interface ILeadOfficeOfficeManagerAppService : ICrudAppService<LeadOfficeOfficeManagerDto, LeadOfficeOfficeManager>
    {
        public IEnumerable<LeadOfficeOfficeManagerDto> GetAll(string upn, int officeId);
        public IEnumerable<LeadOfficeOfficeManagerDto> GetAll(string upn, int itemId, bool isByRole = false);
    }
}

