using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class EmailNotificationLog : FullAuditedEntity
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
        public new string CreatedBy { get; set; }
        public new string ModifiedBy { get; set; }
        public new DateTime ModifiedTime { get; set; }
        public new DateTime CreatedTime { get; set; }
        public new string DeletedBy { get; set; }
        public new DateTime? DeletedTime { get; set; }
        public new bool IsDeleted { get; set; }
    }
}
