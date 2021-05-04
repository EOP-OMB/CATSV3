using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class OriginatorDto : DtoBase
    {
        public string RoundName { get; set; }
        public int? CollaborationId { get; set; }
        public int? EmailControlId { get; set; }
        public string OriginatorUpn { get; set; }
        public int? SharepointId { get; set; }
        public string OriginatorName { get; set; }
        public string Office { get; set; }
        public bool? IsEmailSent { get; set; }
        public bool? IsMarkForDeletion { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string DeletedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public ICollection<SurrogateOriginatorDto> SurrogateOriginators { get; set; }
        //public CollaborationDto Collaboration { get; set; }
        //public bool shouldserializecollaboration()
        //{
        //    // don't serialize the manager property if an employee is their own manager
        //    if (this.GetType().Name == "OriginatorDto")
        //        return false;
        //    else
        //        return true;
        //}


    }
}
