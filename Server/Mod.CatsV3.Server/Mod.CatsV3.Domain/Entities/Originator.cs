using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class Originator : FullAuditedEntity
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int Id { get; set; }
        public string RoundName { get; set; }
        public int? CollaborationId { get; set; }
        public int? EmailControlId { get; set; }
        public string OriginatorUpn { get; set; }
        public int? SharepointId { get; set; }
        public string OriginatorName { get; set; }
        public string Office { get; set; }
        public bool? IsEmailSent { get; set; }
        public virtual Collaboration Collaboration { get; set; }
        [NotMapped]
        public ICollection<SurrogateOriginator> SurrogateOriginators { get; set; }
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
        //    if (this.GetType().Name == "OriginatorProxy")
        //        return false;
        //    else
        //        return true;
        //}
    }
}
