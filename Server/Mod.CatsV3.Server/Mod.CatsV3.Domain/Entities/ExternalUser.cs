using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class ExternalUser : FullAuditedEntity
    {
        public string Title { get; set; }
        public string Name { get; set; }
    }
}
