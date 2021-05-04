using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class LetterType : FullAuditedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string HtmlContent { get; set; }
    }
}
