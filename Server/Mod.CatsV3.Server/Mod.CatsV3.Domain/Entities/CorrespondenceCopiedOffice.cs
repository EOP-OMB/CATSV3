using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class CorrespondenceCopiedOffice : FullAuditedEntity
    {
        [ForeignKey("Correspondence")]
        public int? CorrespondenceId { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string CATSID { get; set; }
        public string CopiedUserUpn { get; set; }
        public string CopiedUserFullName { get; set; }
    }
}
