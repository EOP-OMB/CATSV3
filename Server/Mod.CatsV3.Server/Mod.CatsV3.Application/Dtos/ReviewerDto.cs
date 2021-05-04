using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class ReviewerDto : DtoBase
    {
        public int CollaborationId { get; set; }
        public string RoundName { get; set; }
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
        public bool? IsMarkForDeletion { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string DeletedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public string SurrogateFullName { get; set; }
        public string SurrogateUpn { get; set; }
        public ICollection<SurrogateReviewerDto> surrogateReviewers { get; set; }
        //public CollaborationDto Collaboration { get; set; }
        //public bool shouldserializecollaboration()
        //{
        //    // don't serialize the manager property if an employee is their own manager
        //    if (this.GetType().Name == "ReviewerDto")
        //        return false;
        //    else
        //        return true;
        //}
    }
}
