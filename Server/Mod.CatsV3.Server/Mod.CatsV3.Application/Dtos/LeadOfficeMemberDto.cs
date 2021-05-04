using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class LeadOfficeMemberDto : DtoBase
    {
        public int? LeadOfficeId { get; set; }
        public string ExternalLeadOfficeIds { get; set; }
        public virtual LeadOffice LeadOffice { get; set; }
        public string UserUPN { get; set; }

        public string UserUPN_SP { get; set; }
        public string UserFullName { get; set; }
        public int? RoleId { get; set; }
        public virtual Role Role { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
