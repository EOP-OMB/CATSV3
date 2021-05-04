using Microsoft.Extensions.Options;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class CorrespondenceCopiedArchived : FullAuditedEntity
    {
        [ForeignKey("Correspondence")]
        public int? CorrespondenceId { get; set; }
        //public virtual Correspondence Correspondence { get; set; }
        public string ArchivedUserUpn { get; set; }
        public string ArchivedUserFullName { get; set; }
    }
}
