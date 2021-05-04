using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class EmailNotificationLogDto : DtoBase
    {
        public string CATSID { get; set; }
        public string CurrentRound { get; set; }
        public string Category { get; set; }
        public bool? IsError { get; set; }
        public bool? IsCurrentRound { get; set; }
        public string ErrorMessage { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public string EventType { get; set; }
        public string EmailTemplate { get; set; }
        public string EmailSubject { get; set; }
        public string EmailOrigin { get; set; }
        public string EmailRecipients { get; set; }
        public string EmailCCs { get; set; }
        public string EmailMessage { get; set; }
        public string EmailOriginNames { get; set; }
        public string EmailRecipientNames { get; set; }
        public string EmailCCsNames { get; set; }
        public string StackTrace { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }


    }
}
