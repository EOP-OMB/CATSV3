using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mod.CatsV3.Application.Dtos
{
    public class CollaborationDto : DtoBase
    {


        public int CorrespondenceId { get; set; }
        public string CATSID { get; set; }
        public DateTime? CurrentRoundStartDate { get; set; }
        public DateTime? CurrentRoundEndDate { get; set; }
        public string CompletedRounds { get; set; }
        public string BoilerPlate { get; set; }
        public string CurrentOriginators { get; set; }
        public string CurrentOriginatorsIds { get; set; }
        public string CurrentReviewers { get; set; }
        public string CurrentReviewersIds { get; set; }
        public string CurrentFYIUsers { get; set; }
        public string CurrentFYIUsersIds { get; set; }
        public string SurrogateFullName { get; set; }
        public string SurrogateUpn { get; set; }
        public string CompletedReviewers { get; set; }
        public string CompletedReviewersIds { get; set; }
        public string DraftReviewers { get; set; }
        public string DraftReviewersIds { get; set; }
        public string CurrentActivity { get; set; }
        public string SummaryMaterialBackground { get; set; }
        public string ReviewInstructions { get; set; }
        public int? DraftReviewersIdsCount { get; set; }
        public int? CurrentOriginatorsIdsCount { get; set; }
        public int? CurrentReviewersCount { get; set; }
        public int? CurrentFYIUsersIdsCount { get; set; }
        public int? CompletedReviewersIdsCount { get; set; }
        //[JsonIgnore]
        //public virtual CorrespondenceDto Correspondence { get; set; }
        
        public ICollection<OriginatorDto> Originators { get; set; }
        public ICollection<ReviewerDto> Reviewers { get; set; }
        public ICollection<FYIUserDto> fYIUsers { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }

        //public bool ShouldSerializeCorrespondence()
        //{
        //    if (this.GetType().Name == "CollaborationDto")
        //        return true;
        //    else
        //        return false;
        //}
        //public bool ShouldSerializeReviewers()
        //{
        //    if (this.GetType().Name == "CollaborationDto" || Reviewers == null)
        //        return false;
        //    else
        //        return true;
        //}
        //public bool ShouldSerializeOriginators()
        //{
        //    if (this.GetType().Name == "CollaborationDto" || Originators == null)
        //        return false;
        //    else
        //        return true;
        //}
        //public bool ShouldSerializefYIUsers()
        //{
        //    if (this.GetType().Name == "CollaborationDto" || fYIUsers == null)
        //        return false;
        //    else
        //        return true;
        //}

    }
}
