using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class FYIUserDto : DtoBase
    {
        public int CollaborationId { get; set; }
        public int SharepointId { get; set; }
        public int EmailControlId { get; set; }
        public string FYIUpn { get; set; }
        public string FYIUserName { get; set; }
        public string RoundName { get; set; }
        public string Office { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsEmailSent { get; set; }
        public bool? IsMarkForDeletion { get; set; }
        //public CollaborationDto Collaboration { get; set; }
        //public bool shouldserializecollaboration()
        //{
        //    // don't serialize the manager property if an employee is their own manager
        //    if (this.GetType().Name == "FYIUserDto")
        //        return false;
        //    else
        //        return true;
        //}
    }
}
