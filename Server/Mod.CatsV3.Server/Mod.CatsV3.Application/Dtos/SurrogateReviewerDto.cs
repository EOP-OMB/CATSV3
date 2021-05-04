using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class SurrogateReviewerDto : DtoBase
    {

        public string CATSUserSPID { get; set; }
        public string CATSUser { get; set; }
        public string CATSUserUPN { get; set; }
        public string SurrogateSPUserID { get; set; }
        public string Surrogate { get; set; }
        public string SurrogateUPN { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
