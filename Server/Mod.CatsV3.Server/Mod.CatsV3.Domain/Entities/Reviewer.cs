using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class Reviewer : FullAuditedEntity
    {
        private bool _IsCurrentRound;
        public string RoundName { get; set; }

        public bool? IsCurrentRound
        {
            get
            {
                if (Collaboration != null)
                {
                    if( Collaboration.Correspondence.CurrentReview.Trim() == RoundName.Trim())
                        return true;
                    else 
                        return false;
                }
                else
                {
                    return _IsCurrentRound;
                }
            }
            set
            {
                _IsCurrentRound = value.GetValueOrDefault();
            }
        }
        public int EmailControlId { get; set; }
        public string ReviewerUPN { get; set; }
        public int SharepointId { get; set; }
        public string ReviewerName { get; set; }
        public string Office { get; set; }
        public DateTime? RoundStartDate { get; set; }
        public DateTime? RoundEndDate { get; set; }
        public DateTime? RoundCompletedDate { get; set; }
        public string RoundCompletedBy { get; set; }
        public string RoundCompletedByUpn { get; set; }
        public bool? IsRoundCompletedBySurrogate { get; set; }
        public string RoundActivity { get; set; }
        public string DraftBy { get; set; }
        public string DraftByUpn { get; set; }
        public string DraftReason { get; set; }
        public DateTime? DraftDate { get; set; }
        public bool? IsEmailSent { get; set; }
        public string SurrogateFullName { get; set; }
        public string SurrogateUpn { get; set; }
        [NotMapped]
        public ICollection<SurrogateReviewer> SurrogateReviewers { get; set; }

        [ForeignKey("Collaboration")]
        public int CollaborationId { get; set; }
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
        //    if (this.GetType().Name == "ReviewerProxy")
        //        return false;
        //    else
        //        return true;
        //}


    }
}
