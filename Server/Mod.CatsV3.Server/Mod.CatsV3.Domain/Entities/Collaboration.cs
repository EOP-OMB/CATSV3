using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class Collaboration : FullAuditedEntity
    {

        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int Id { get; set; }

        private string _CurrentOriginators;
        private string _CurrentOriginatorsIds;
        private string _CurrentReviewers;
        private string _CurrentReviewersIds;
        private string _CurrentFYIUsers;
        private string _CurrentFYIUsersIds;
        private string _CompletedReviewers;
        private string _CompletedReviewersIds;
        private string _CompletedRounds;
        private string _DraftReviewers;
        private string _DraftReviewersIds;
        private DateTime? _CurrentRoundStartDate;
        private DateTime? _CurrentRoundEndDate;
        private string _CATSID;

        //public string CATSID { get; set; }

        public string CATSID
        {
            get
            {
                if (Correspondence != null)
                {
                    string value = Correspondence.CATSID;
                    return value;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _CATSID = value;
            }
        }
        public DateTime? CurrentRoundStartDate
        {
            get
            {
                if (Reviewers != null)
                {
                    DateTime? value = Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (Correspondence == null ? "" : Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.RoundStartDate).ToList().OrderByDescending(x => x.GetValueOrDefault()).FirstOrDefault();
                    return value;
                }
                else
                {
                    return _CurrentRoundStartDate;
                }
            }
            set
            {
                _CurrentRoundStartDate = value;
            }
        }
        public DateTime? CurrentRoundEndDate
        {
            get
            {
                if (Reviewers != null)
                {
                    DateTime? value = Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.RoundEndDate).OrderByDescending(x => x.GetValueOrDefault()).FirstOrDefault();
                    return value;
                }
                else
                {
                    return _CurrentRoundEndDate;
                }
            }
            set
            {
                _CurrentRoundEndDate = value;
            }
        }
        public string CompletedRounds { get; set; }
        public string BoilerPlate { get; set; }
        //public string CurrentOriginators { get; set; }

        public string CurrentOriginators
        {
            get
            {
                if (Originators != null)
                {
                    Originators = Originators
                    .GroupBy(a => (a.OriginatorUpn))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Originators.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.OriginatorName).ToArray());
                }
                else
                {
                    return _CurrentOriginators;
                }
            }
            set
            {
                _CurrentOriginators = value;
            }
        }

        public string CurrentOriginatorsIds
        {
            get
            {
                if (Originators != null)
                {
                    Originators = Originators
                    .GroupBy(a => (a.OriginatorUpn))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Originators.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.OriginatorUpn).ToArray());
                }
                else
                {
                    return _CurrentOriginatorsIds;
                }
            }
            set
            {
                _CurrentOriginatorsIds = value;
            }
        }
        public int? CurrentOriginatorsIdsCount
        {
            get
            {
                if (Originators != null)
                {
                    Originators = Originators
                    .GroupBy(a => (a.OriginatorUpn))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return Originators.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.OriginatorUpn).Count();
                }
                else
                {
                    return 0;
                }
            }
        }
        public string CurrentReviewers {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.ReviewerName).ToArray());
                }
                else
                {
                    return _CurrentReviewers;
                }
            }
            set
            {
                _CurrentReviewers = value;
            }
        }
        public string CurrentReviewersIds {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.ReviewerUPN).ToArray());
                }
                else
                {
                    return _CurrentReviewersIds;
                }
            }
            set
            {
                _CurrentReviewersIds = value;
            }
        }

        public int? CurrentReviewersCount
        {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.ReviewerUPN).Count();
                }
                else
                {
                    return 0;
                }
            }
        }
        public string CurrentFYIUsers {
            get
            {
                if (FYIUsers != null)
                {
                    FYIUsers = FYIUsers
                    .GroupBy(a => (a.FYIUpn))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", FYIUsers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.FYIUserName).ToArray());
                }
                else
                {
                    return _CurrentFYIUsers;
                }
            }
            set
            {
                _CurrentFYIUsers = value;
            }
        }
        public string CurrentFYIUsersIds
        {
            get
            {
                if (FYIUsers != null)
                {
                    FYIUsers = FYIUsers
                    .GroupBy(a => (a.FYIUpn))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", FYIUsers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.FYIUpn).ToArray());
                }
                else
                {
                    return _CurrentFYIUsersIds;
                }
            }
            set
            {
                _CurrentFYIUsersIds = value;
            }
        }
        public int? CurrentFYIUsersIdsCount
        {
            get
            {
                if (FYIUsers != null)
                {
                    FYIUsers = FYIUsers
                    .GroupBy(a => (a.FYIUpn))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return FYIUsers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId).Select(x => x.FYIUpn).Count();
                }
                else
                {
                    return 0;
                }
            }
        }
        public string SurrogateFullName { get; set; }
        public string SurrogateUpn { get; set; }
        public string CompletedReviewers {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId && string.IsNullOrEmpty(x.RoundCompletedBy) == false && string.IsNullOrEmpty(x.RoundCompletedByUpn) == false).Select(x => x.RoundCompletedBy).ToArray());
                }
                else
                {
                    return _CompletedReviewers;
                }
            }
            set
            {
                _CompletedReviewers = value;
            }
        }
        public string CompletedReviewersIds
        {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId && string.IsNullOrEmpty(x.RoundCompletedBy) == false && string.IsNullOrEmpty(x.RoundCompletedByUpn) == false).Select(x => x.RoundCompletedByUpn).ToArray());
                }
                else
                {
                    return _CompletedReviewersIds;
                }
            }
            set
            {
                _CompletedReviewersIds = value;
            }
        }
        public int? CompletedReviewersIdsCount
        {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId && string.IsNullOrEmpty(x.RoundCompletedBy) == false && string.IsNullOrEmpty(x.RoundCompletedByUpn) == false).Select(x => x.RoundCompletedByUpn).Count();
                }
                else
                {
                    return 0;
                }
            }
        }
        public string DraftReviewers
        {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId && string.IsNullOrEmpty(x.DraftBy) == false && string.IsNullOrEmpty(x.DraftByUpn) == false).Select(x => x.DraftBy).ToArray());
                }
                else
                {
                    return _DraftReviewers;
                }
            }
            set
            {
                _DraftReviewers = value;
            }
        }
        public string DraftReviewersIds
        {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return string.Join(";", Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId && string.IsNullOrEmpty(x.DraftBy) == false && string.IsNullOrEmpty(x.DraftByUpn) == false).Select(x => x.DraftByUpn).ToArray());
                }
                else
                {
                    return _DraftReviewersIds;
                }
            }
            set
            {
                _DraftReviewersIds = value;
            }
        }

        public int? DraftReviewersIdsCount
        {
            get
            {
                if (Reviewers != null)
                {
                    Reviewers = Reviewers
                    .GroupBy(a => (a.ReviewerUPN))
                    .SelectMany(a => a.Where(b => b.Id == a.Max(c => c.Id))).ToList();

                    return Reviewers.Where(x => x.IsDeleted == false && x.RoundName == (x.Collaboration.Correspondence == null ? "" : x.Collaboration.Correspondence.CurrentReview) && x.Collaboration.CorrespondenceId == CorrespondenceId && string.IsNullOrEmpty(x.DraftBy) == false && string.IsNullOrEmpty(x.DraftByUpn) == false).Select(x => x.DraftByUpn).Count();
                }
                else
                {
                    return 0;
                }
            }
        }
        public string CurrentActivity { get; set; }
        public string SummaryMaterialBackground { get; set; }
        public string ReviewInstructions { get; set; }
        [ForeignKey("Correspondence")]
        public int CorrespondenceId { get; set; }
        public virtual Correspondence Correspondence { get; set; }
        public virtual ICollection<Originator> Originators { get; set; }
        public virtual ICollection<Reviewer> Reviewers { get; set; }
        public virtual ICollection<FYIUser> FYIUsers { get; set; }
        public new string CreatedBy { get; set; }
        public new string ModifiedBy { get; set; }
        public new DateTime ModifiedTime { get; set; }
        public new DateTime CreatedTime { get; set; }
        public new string DeletedBy { get; set; }
        public new DateTime? DeletedTime { get; set; }
        public new bool IsDeleted { get; set; }
        //public bool ShouldSerializeCorrespondence()
        //{
        //    // don't serialize the Manager property if an employee is their own manager
        //    if (this.GetType().Name == "CollaborationProxy" || this.GetType().Name == "ReviewerProxy")
        //        return true;
        //    else
        //        return true;
        //}
    }
}
