using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class LeadOfficeDto : DtoBase
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsHidden { get; set; }
        public ICollection<LeadOfficeMemberDto> LeadOfficeMembers { get; set; }
        public ICollection<LeadOfficeOfficeManagerDto> LeadOfficeOfficeManagers { get; set; }
    }
}
