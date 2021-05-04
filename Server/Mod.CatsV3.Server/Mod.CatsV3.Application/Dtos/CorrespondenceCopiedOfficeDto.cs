using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
   public class CorrespondenceCopiedOfficeDto : AuditedDtoBase
    {
        public int? CorrespondenceId { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string CATSID { get; set; }
        public string CopiedUserUpn { get; set; }
        public string CopiedUserFullName { get; set; }
        public bool? IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
