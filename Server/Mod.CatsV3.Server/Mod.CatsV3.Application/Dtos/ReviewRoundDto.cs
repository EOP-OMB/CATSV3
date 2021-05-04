using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class ReviewRoundDto : DtoBase
    {
        public string Name { get; set; }
        public string ReviewRoundAcronym { get; set; }
        public string Description { get; set; }
        public bool? IsCombinedRounds { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
        public string CreatedTime { get; set; }
        public string ModifiedTime { get; set; }
        public string DeletedTime { get; set; }
        public string IsDeleted { get; set; }
    }
}
