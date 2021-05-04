using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
   public class DLGroup : FullAuditedEntity
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<DLGroupMembers> DLGroupMembers { get; set; }
        public bool ShouldSerializeDLGroupMembers()
        {
            //to prevent self loop error
            if (this.GetType().Name == "DLGroupProxy")
                return false;
            else
                return true;
        }
    }

}

