using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class CorrespondenceCopiedArchivedDto : AuditedDtoBase
    {
        public int CorrespondenceId { get; set; }
        //public virtual Correspondence Correspondence { get; set; }
        public string ArchivedUserUpn { get; set; }
        public string ArchivedUserFullName { get; set; }
        public bool? IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
