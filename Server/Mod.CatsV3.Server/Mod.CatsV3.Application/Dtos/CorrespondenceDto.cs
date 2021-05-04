using System;
using Mod.Framework.Application;
using System.Collections.Generic;
using System.Text;
using Mod.CatsV3.Domain.Entities;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Mod.CatsV3.Application.Dtos
{
    public class CorrespondenceDto : AuditedDtoBase
    {
        public string CATSID { get; set; }
        public string LeadOfficeName { get; set; }
        public string CorrespondentName { get; set; }
        public string OtherSigners { get; set; }
        public string OtherSignersFormated { get; set; }
        public string LetterStatus { get; set; }
        public string LetterTypeName { get; set; }
        public string ReviewStatus { get; set; }
        public string CurrentReview { get; set; }
        public string LetterSubject { get; set; }
        public string LetterCrossReference { get; set; }
        public bool? WhFyi { get; set; }
        public bool IsLetterRequired { get; set; }
        public string NotRequiredReason { get; set; }
        public string ExternalAgencies { get; set; }
        public string LeadOfficeUsersIds { get; set; }
        public string LeadOfficeUsersDisplayNames { get; set; }
        public string CopiedOfficeName { get; set; }
        public string[] CopiedOffices { get; set; }
        public string CopiedUsersIds { get; set; }
        public string CopiedUsersDisplayNames { get; set; }
        public DateTime? LetterDate { get; set; }
        public DateTime? LetterReceiptDate { get; set; }
        public DateTime? DueforSignatureByDate { get; set; }
        public DateTime? PADDueDate { get; set; }
        public bool? Rejected { get; set; }
        public string RejectionReason { get; set; }
        public string RejectedLeadOffices { get; set; }
        public string FiscalYear { get; set; }
        public bool? IsPendingLeadOffice { get; set; }
        public bool? IsUnderReview { get; set; }
        public bool? IsSaved { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsFinalDocument { get; set; }
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
        public int? FolderId { get; set; }
        //public Folder Folder { get; set; }
        public int? AdministrationId { get; set; }
        public Administration Administration { get; set; }
        public string Icon { get; set; }
        public string ReferenceDocuments { get; set; }
        public string ReviewDocuments { get; set; }
        public string FinalDocuments { get; set; }
        public string DocumentsCount { get; set; }
        public int? FinalDocumentsCount { get; set; }
        public int? ReferenceDocumentsCount { get; set; }
        public int? ReviewDocumentsCount { get; set; }
        public string Originators { get; set; }
        public int OriginatorsCount { get; set; }
        public string Reviewers { get; set; }
        public int ReviewersCount { get; set; }
        public string FYIUsers { get; set; }
        public int FYIUsersCount { get; set; }
        public bool? IsCurrentUserFYI { get; set; }
        public string CompletedUsers { get; set; }
        public string CompletedRounds { get; set; }
        public int CompletedCount { get; set; }
        public string DraftUsers { get; set; }
        public int DraftCount { get; set; }
        public string CurrentUserUPN { get; set; }
        public string CurrentUserFullName { get; set; }
        public string CurrentUserEmail { get; set; }
        public new string CreatedBy { get; set; }
        public new string ModifiedBy { get; set; }
        public new DateTime ModifiedTime { get; set; }
        public new DateTime CreatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsEmailElligible { get; set; }

        public int CatsNotificationId { get; set; } = 0;
        public ICollection<CorrespondenceCopiedArchivedDto> CorrespondenceCopiedArchiveds { get; set; }

        public ICollection<CorrespondenceCopiedOfficeDto> CorrespondenceCopiedOffices { get; set; }
        public CollaborationDto Collaboration { get; set; }
        public string officeRole { get; internal set; }

        //public bool ShouldSerializeCollaboration()
        //{
        //    // don't serialize the Manager property if an employee is their own manager
        //    if (this.GetType().Name == "CorrespondenceDto")
        //        return false;
        //    else
        //        return true;
        //}
    }
}
