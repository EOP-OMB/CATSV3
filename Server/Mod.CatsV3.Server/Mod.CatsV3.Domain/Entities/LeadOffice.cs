using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class LeadOffice : FullAuditedEntity
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsHidden { get; set; }
        public virtual ICollection<LeadOfficeMember> LeadOfficeMembers { get; set; }
        public virtual ICollection<LeadOfficeOfficeManager> LeadOfficeOfficeManagers { get; set; }

    }
}
