using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class CommonFeatures : ICommonFeatures
    {
        bool IsDeleted;
        bool Rejected; 
        string LetterStatus; 
        string ReviewStatus; 
        Folder Folder; 
        string CATSID; 
        Collaboration Collaboration;
        public CommonFeatures(bool IsDeleted, bool Rejected, string LetterStatus, string ReviewStatus, Folder Folder, string CATSID, Collaboration Collaboration)
        {
            this.CATSID = CATSID;
            this.IsDeleted = IsDeleted;
            this.LetterStatus = LetterStatus;
            this.ReviewStatus = ReviewStatus;
            this.Folder = Folder;
            this.Collaboration = Collaboration;
        }

        public string Icon
        {
            get
            {
                return Utilities.getIcon(IsDeleted, Rejected, LetterStatus, ReviewStatus);
            }
        }
        public string DocumentsCount
        {
            get
            {
                return Utilities.getDocumentCount(Folder);
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
                return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.CurrentOriginators);
            }
        }
        public string Reviewers
        {
            get
            {
                return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.CurrentReviewers);
            }
        }
        public string FYIUsers
        {
            get
            {
                return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.CurrentFYIUsers);
            }
        }
        public string CompletedUsers
        {
            get
            {
                return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.CompletedReviewers);
            }
        }
        public string DraftUsers
        {
            get
            {
                return Utilities.getAssignedUsers(Collaboration == null ? "" : Collaboration.DraftReviewers);
            }
        }
        public string CompletedRounds
        {
            get
            {
                return Utilities.getCompletedRounds(Collaboration == null ? "" : Collaboration.CompletedRounds);
            }
        }
    }
}
