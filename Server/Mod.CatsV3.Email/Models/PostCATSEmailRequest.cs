using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Email.Models
{
    public class PostCATSEmailRequest
    {
        public string ItemCATSID { get; set; } = "";
        public string ItemCorrespondentName { get; set; } = "";
        public string LetterType { get; set; } = "";
        public string ItemSubject { get; set; } = "";
        public string CurrentUserEmail { get; set; } = "";
        public string[] Reviewers { get; set; } = Array.Empty<string>();
        public string[] ReviewersIncludedDLsMembers { get; set; } = Array.Empty<string>();
        public string[] ReviewersDLs { get; set; } = Array.Empty<string>();
        public string[] ReviewersNames { get; set; } = Array.Empty<string>();
        public string[] Originators { get; set; } = Array.Empty<string>();
        public string[] OriginatorsNames { get; set; } = Array.Empty<string>();
        public string[] FYIUsers { get; set; } = Array.Empty<string>();
        public string[] FYIUsersIncludedDLsMembers { get; set; } = Array.Empty<string>();
        public string[] FYIUsersDLs { get; set; } = Array.Empty<string>();
        public string[] FYIUsersNames { get; set; } = Array.Empty<string>();
        public string[] SurrogatesUsers { get; set; } = Array.Empty<string>();
        public string[] SurrogatesUsersDLs { get; set; } = Array.Empty<string>();
        public string[] SurrogatesUsersNames { get; set; } = Array.Empty<string>();
        public string[] LeadOfficeUsers { get; set; } = Array.Empty<string>();
        public string LeadOffice { get; set; }
        public string[] LeadOfficeUsersNames { get; set; } = Array.Empty<string>();
        public string[] CopiedUsers { get; set; } = Array.Empty<string>();
        public string[] CopiedUsersNames { get; set; } = Array.Empty<string>();
        public string CopiedOffices { get; set; }
        public string[] CorrespondentUnitUsers { get; set; } = Array.Empty<string>();
        public string[] CorrespondentUnitNames { get; set; } = Array.Empty<string>();
        public string[] ItemSignedFinalPDFPath { get; set; } = Array.Empty<string>();
        public string[] ItemSignedFinalPDFFileName { get; set; } = Array.Empty<string>();
        public string[] ItemReferenceDocumentPath { get; set; } = Array.Empty<string>();
        public string[] ItemReferenceDocumentFileName { get; set; } = Array.Empty<string>();
        public string[] ItemReviewDocumentPath { get; set; } = Array.Empty<string>();
        public string[] ItemReviewDocumentFileName { get; set; } = Array.Empty<string>();
        public string CurrentReviewRoundDueDate { get; set; } = "";
        public string UserUpn { get; set; } = "";
        public string CurrentReviewRound { get; set; } = "";
        public string ModifiedBy { get; set; } = "";
        public string SURROGATE_ID { get; set; } = "";
        public string SURROGATE_OF { get; set; } = "";
        public string CATSNotificationType { get; set; } = "";
        public string SummaryBackground { get; set; } = "";
        public string ReviewInstructions { get; set; } = "";
        public string DraftReason { get; set; } = "";
        public string[] DraftReviews { get; set; } = Array.Empty<string>();
        public string[] DraftReviewsNames { get; set; } = Array.Empty<string>();
        public string[] CompletedReviews { get; set; } = Array.Empty<string>();
        public string[] CompletedReviewsNames { get; set; } = Array.Empty<string>();
        public string DateCompleted { get; set; } = "";
        public string EmailTemplate { get; set; } = "";
        public string EmailControllerID { get; set; } = "";
        public string EmailControllerID_FYI { get; set; } = "";
        public string EmailControllerID_Originator { get; set; } = "";
        public string Source { get; set; } = "";
        public string EventType { get; set; } = "";
        public string CATSSupportTeam { get; set; } = "";
        public string CATSSupportStatusMessage { get; set; } = "";
        public string CATSSupportIntendedRecipients { get; set; } = "";
        public string CurrentSiteUrl { get; set; } = "";
        public string CurrentFolder { get; set; } = "";
        public bool IsMultipleNofications { get; set; } = false;
        public string mode { get; set; } = "";
        public string PreviousStatusMessage { get; set; } = "";
        public string PreviousRound { get; set; } = "";
        public string PreviousFolderName { get; set; } = "";
        public string PreviousFolderUrl { get; set; } = "";
        public string SURROGATE_OF_EMAIL { get; set; } = "";
        public string Due_for_Signature_By { get; set; } = "";
        public string CurrenFullName { get; set; } = "";
        public string Modified { get; set; } = "";
        public string RejectLeadOffice { get; set; } = "";
        public string RejectReasons { get; set; } = "";
        public string[] LeadOfficeUsersFullNames { get; internal set; } = Array.Empty<string>();
        public string[] OriginatorsFullNames { get; internal set; } = Array.Empty<string>();
        public string[] CopiedUsersFullNames { get; internal set; } = Array.Empty<string>();
        public string[] CorrespondentUnitUsersFullNames { get; internal set; } = Array.Empty<string>();
    }
}
