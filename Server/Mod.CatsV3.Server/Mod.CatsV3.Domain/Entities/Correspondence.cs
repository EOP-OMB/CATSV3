using Microsoft.Extensions.Options;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    //[Table("dbo.Correspondence")]
    public class Correspondence : FullAuditedEntity
    {
        //[Key]
        
        //public override int Id { get; set; }
        private string _CorrespondentName;
        private string _ReviewStatus;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string CATSID { get; set; }
        public string LeadOfficeName { get; set; }
        public string CorrespondentName {
            get
            {
                if (string.IsNullOrEmpty(_CorrespondentName))
                {
                    return "N/A";
                }
                else
                {
                    return _CorrespondentName;
                }
            }
            set
            {
                _CorrespondentName = value;
            }
        }
        public string LetterTypeName { get; set; }
        public string OtherSigners { get; set; }
        public string LetterStatus { get; set; }
        public string ReviewStatus {
            get
            {
                if (this.Collaboration != null)
                {
                    var completedcount = (int)this.Collaboration.CompletedReviewersIdsCount;
                    var draftcount = (int)this.Collaboration.DraftReviewersIdsCount;
                    if ((_ReviewStatus == "Draft" && draftcount == 0 && LetterStatus == "Open") || (_ReviewStatus == "Completed" && completedcount == 0 && LetterStatus == "Open"))
                        return "In Progress";
                    else
                        return _ReviewStatus;
                }
                else
                {
                    return _ReviewStatus;
                }
            }
            set {
                _ReviewStatus = value;
            }
        }
        public string CurrentReview { get; set; }
        public string LetterSubject { get; set; }
        public string LetterCrossReference { get; set; }
        public bool? WhFyi  { get; set; }
        public bool? IsLetterRequired { get; set; }
        public string NotRequiredReason { get; set; }
        public string ExternalAgencies { get; set; }
        public string LeadOfficeUsersIds { get; set; }
        public string LeadOfficeUsersDisplayNames { get; set; }
        public string CopiedOfficeName { get; set; }
        public string[] CopiedOffices
        {
            //get
            //{
            //    return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.DraftReviewers);
            //}
            get
            {
                if (this.CopiedOfficeName != null)
                {
                    return this.CopiedOfficeName.Split(',');
                }
                else
                {
                    return Array.Empty<string>();
                }
            }
        }
        public string CopiedUsersIds { get; set; }
        public string CopiedUsersDisplayNames { get; set; }
        public DateTime? LetterDate { get; set; }
        public DateTime? LetterReceiptDate { get; set; }
        public DateTime? DueforSignatureByDate { get; set; }
        public DateTime? PADDueDate { get; set; }
        public bool? Rejected { get; set; }
        public string RejectionReason { get; set; }
        public string RejectedLeadOffices { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FiscalYear { get; set; }
        public bool? IsPendingLeadOffice { get; set; }
        public bool? IsUnderReview { get; set; }
        public bool? IsSaved { get; set; }
        public bool? IsEmailElligible { get; set; }
        public bool? IsArchived { get; set; }
        public string Notes { get; set; }
        public string ReasonsToReopen { get; set; }
        public bool? IsReopen { get; set; }
        public bool? IsAdminReOpen { get; set; }
        public string AdminReOpenReason { get; set; }
        public DateTime? AdminReOpenDate { get; set; }
        public DateTime? AdminClosureDate { get; set; }
        public bool? IsAdminClosure { get; set; }
        public string AdminClosureReason { get; set; }
        public string SurrogateFullName { get; set; }
        public string SurrogateUpn { get; set; }
        [ForeignKey("Folder")]
        public int? FolderId { get; set; }        
        public virtual Folder Folder { get; set; }
        public int? AdministrationId { get; set; }
        public virtual Administration Administration { get; set; }
        public string Icon
        {
            get
            {
                IsPendingLeadOffice = IsPendingLeadOffice.HasValue ? IsPendingLeadOffice : (bool)false;
                Rejected = Rejected.HasValue ? Rejected : (bool)false;
                IsSaved = IsSaved.HasValue ? IsSaved : (bool)false;
                return Utilities.getIcon((bool)IsDeleted, (bool)Rejected, LetterStatus, ReviewStatus, (bool)IsPendingLeadOffice, (bool)IsSaved);
            }
        }
        public string DocumentsCount
        {
            get
            {
                return Utilities.getDocumentCount(Folder);
            }
        }
        public int? FinalDocumentsCount
        {
            get
            {
                return Utilities.getDocumentFinalCount(Folder);
            }
        }
        public int? ReferenceDocumentsCount
        {
            get
            {
                return Utilities.getDocumentReferenceCount(Folder);
            }
        }
        public int? ReviewDocumentsCount
        {
            get
            {
                return Utilities.getDocumentReviewCount(Folder);
            }
        }
        public string OtherSignersFormated
        {
            get
            {
                return Utilities.getAssignedUsers(OtherSigners);
            }
        }
        public string ReferenceDocuments
        {
            get
            {
                return Utilities.getDocuments(Folder, CATSID, "Reference Document");
            }
        }
        public string ReviewDocuments
        {

            get
            {
                return Utilities.getDocuments(Folder, CATSID, "Review Document");
            }
        }
        public string FinalDocuments
        {
            get
            {
                return Utilities.getDocuments(Folder, CATSID, "Final Document");
            }
        }
        public string Originators
        {
            get
            {
                if (this.Collaboration != null)
                {
                    if (this.Collaboration.Originators != null)
                        return Utilities.getAssignedUsers(string.Join(";", this.Collaboration.Originators.Where(x => x.IsDeleted == false && x.RoundName.ToLower().Trim() == CurrentReview.ToLower().Trim() && x.Collaboration.CorrespondenceId == Id).Select(x => x.OriginatorName).ToArray()));
                    else return "";
                }
                else
                {
                    return "";
                }
            }
        }
        public int OriginatorsCount
        {
            get
            {
                if (this.Collaboration != null)
                    return (int)this.Collaboration.CurrentOriginatorsIdsCount;
                else
                    return 0;
            }
        }
        public string Reviewers
        {
            get
            {
                if (this.Collaboration != null)
                {
                    if (this.Collaboration.Reviewers != null)
                        return Utilities.getAssignedUsers(string.Join(";", this.Collaboration.Reviewers.Where(x => x.IsDeleted == false && x.RoundName.ToLower().Trim() == CurrentReview.ToLower().Trim() && x.Collaboration.CorrespondenceId == Id).Select(x => x.ReviewerName).ToArray()));
                    else
                        return "";
                }
                else
                {
                    return "";
                }
            }
        }
        public int ReviewersCount
        {
            get
            {
                if (this.Collaboration != null)
                    return (int)this.Collaboration.CurrentReviewersCount;
                else return 0;
            }
        }
        public string FYIUsers
        {
            get
            {
                if (this.Collaboration != null)
                {
                    if (this.Collaboration.FYIUsers != null)
                        return Utilities.getAssignedUsers(string.Join(";", this.Collaboration.FYIUsers.Where(x => x.IsDeleted == false && x.RoundName.ToLower().Trim() == CurrentReview.ToLower().Trim() && x.Collaboration.CorrespondenceId == Id).Select(x => x.FYIUserName).ToArray()));
                    else
                        return "";
                }
                else
                {
                    return "";
                }
            }
        }
        public int FYIUsersCount
        {
            get
            {
                if (this.Collaboration != null)
                    return (int)this.Collaboration.CurrentFYIUsersIdsCount;
                else
                    return 0;
            }
        }
        public string CompletedUsers
        {
            //get
            //{
            //    return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.CompletedReviewers);
            //}
            get
            {
                if (this.Collaboration != null)
                {
                    if (this.Collaboration.Reviewers != null)
                        return Utilities.getAssignedUsers(string.Join(";", this.Collaboration.Reviewers.Where(x => x.IsDeleted == false && x.RoundName.ToLower().Trim() == CurrentReview.ToLower().Trim() && x.Collaboration.CorrespondenceId == Id && string.IsNullOrEmpty(x.RoundCompletedBy) == false && string.IsNullOrEmpty(x.RoundCompletedByUpn) == false).Select(x => x.RoundCompletedBy).ToArray()));
                    else
                        return "";
                }
                else
                {
                    return "";
                }
            }
        }

        public int CompletedCount
        {
            get
            {
                if (this.Collaboration != null)
                {
                    return (int)this.Collaboration.CompletedReviewersIdsCount;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string DraftUsers
        {
            //get
            //{
            //    return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.DraftReviewers);
            //}
            get
            {
                if (this.Collaboration != null)
                {
                    if (this.Collaboration.Reviewers != null)
                        return Utilities.getAssignedUsers(string.Join(";", this.Collaboration.Reviewers.Where(x => x.IsDeleted == false && x.RoundName.ToLower().Trim() == CurrentReview.ToLower().Trim() && x.Collaboration.CorrespondenceId == Id && string.IsNullOrEmpty(x.DraftBy) == false && string.IsNullOrEmpty(x.DraftByUpn) == false).Select(x => x.DraftBy).ToArray()));
                    else
                        return "";
                }
                else
                {
                    return "";
                }
            }
        }
        public int DraftCount
        {
            get
            {
                if (this.Collaboration != null)
                {
                    return (int)this.Collaboration.DraftReviewersIdsCount;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string CompletedRounds
        {
            get
            {
                return Utilities.getCompletedRounds(Collaboration == null ? "" : Collaboration.CompletedRounds);
            }
        }
        public virtual ICollection<CorrespondenceCopiedArchived> CorrespondenceCopiedArchiveds { get; set; }
        public virtual ICollection<CorrespondenceCopiedOffice> CorrespondenceCopiedOffices { get; set; }
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
    //    if (this.GetType().Name == "CorrespondenceProxy")
    //        return false;
    //    else
    //        return true;
    //}
}
}
