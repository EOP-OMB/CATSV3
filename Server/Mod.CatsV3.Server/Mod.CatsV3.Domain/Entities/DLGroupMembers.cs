using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class DLGroupMembers : FullAuditedEntity
    {
        [ForeignKey("DLGroup")]
        public int DLGroupId { get; set; }
        public virtual DLGroup DLGroup  { get; set; }
        public string UserUPN { get; set; }
        public string UserFullName { get; set; }
        public int? RoleId { get; set; }
        public bool ShouldSerializeDLGroup()
        {
            //to prevent self loop error
            if (this.GetType().Name == "DLGroupMembersProxy")
                return false;
            else
                return true;
        }
    }
}
