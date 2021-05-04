using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Email.Models
{
    public class CATSEmailAPIConfiguration
    {
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public string EmailServer { get; set; }
        public int EmailPort { get; set; }
        public string EmailNoReply { get; set; }
        public string EmailTo { get; set; }
        public string EmailFrom { get; set; }
        public string EmailToCATSSupport { get; set; }
        public string LdapDomain { get; set; }
        public string LdapPath { get; set; }
        public string AllowedCrossOrigin { get; set; }
        public bool UseDefaultSender { get; set; }
        public bool CATSEmailNotificationDisabled { get; set; }
        public bool FinalDocumentEmailToOMBMailboxDisabled { get; set; }
        public int BatchTimeInterval { get; set; }
        public int BatchLimit { get; set; }
        public int BulkIPackageLimit { get; set; }
        public string LogsLocation { get; set; }
        public string TempAttachmentsLocation { get; set; }
        public string TestFileAttachmentLocation { get; set; }
        public string SiteUrlDev { get; set; }
        public string SiteUrlProd { get; set; }
        public string DocumentLibrary { get; set; }
        public string MasterList { get; set; }
    }
}
