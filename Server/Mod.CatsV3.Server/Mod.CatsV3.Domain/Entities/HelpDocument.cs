using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class HelpDocument : FullAuditedEntity
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool? IsExternal { get; set; }
        public bool? IsDevider { get; set; }
    }
}
