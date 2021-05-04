using Microsoft.Extensions.Options;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class FYIUser : FullAuditedEntity
    {
        public int CollaborationId { get; set; }
        public int? SharepointId { get; set; }
        public int? EmailControlId { get; set; }
        public string FYIUpn { get; set; }
        public string FYIUserName { get; set; }
        public string RoundName { get; set; }
        public string Office { get; set; }
        public bool? IsEmailSent { get; set; }
        public virtual Collaboration Collaboration { get; set; }
        public new string CreatedBy { get; set; }
        public new string ModifiedBy { get; set; }
        public new DateTime ModifiedTime { get; set; }
        public new DateTime CreatedTime { get; set; }
        public new string DeletedBy { get; set; }
        public new DateTime? DeletedTime { get; set; }
        public new bool IsDeleted { get; set; }
        //public bool ShouldSerializeCollaboration()
        //{
        //    // don't serialize the Manager property if an employee is their own manager
        //    if (this.GetType().Name == "FYIUserProxy")
        //        return false;
        //    else
        //        return true;
        //}
    }
}
