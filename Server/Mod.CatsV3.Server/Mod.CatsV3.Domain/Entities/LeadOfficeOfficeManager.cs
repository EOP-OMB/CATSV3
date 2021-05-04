using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class LeadOfficeOfficeManager : FullAuditedEntity
    {
        public int LeadOfficeId { get; set; }
        private string ExternalLeadOfficeIds { get; set; }
        public string externalLeadOfficeIds
        {
            get => ExternalLeadOfficeIds ?? "";
            set => ExternalLeadOfficeIds = value ?? ExternalLeadOfficeIds;
        }

        public string UserUPN { get; set; }
        public string UserUPN_SP { get; set; }
        public string UserFullName { get; set; }

        [ForeignKey("Role")]
        public int? RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
