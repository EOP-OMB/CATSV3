using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class Role : FullAuditedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
