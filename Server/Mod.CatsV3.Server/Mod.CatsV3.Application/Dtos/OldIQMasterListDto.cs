using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mod.CatsV3.Application.Dtos
{
    public class OldIQMasterListDto : DtoBase
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
