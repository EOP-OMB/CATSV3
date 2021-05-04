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
    public class OldIQMasterList : FullAuditedEntity
    {
        public string CATSID { get; set; }
        public string CurrentReview { get; set; }
        public string IQStatus { get; set; }
        public string FinalDocuments { get; set; }
        public string ReferenceDocuments { get; set; }
        public string ReviewDocuments { get; set; }
        public string FolderUrlPath { get; set; }
        public string FolderName { get; set; }
        public int? FolderID { get; set; }
        public string CreatedByUpn { get; set; }
        public string ModifiedByUpn { get; set; }
        public string AttachedDocumentsOriginalNames { get; set; }
        public string SignedFinalResponsePDFFileName { get; set; }
    }
}
