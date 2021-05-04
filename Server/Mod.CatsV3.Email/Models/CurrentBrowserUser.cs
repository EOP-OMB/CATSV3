using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Email.Models
{
    public class CurrentBrowserUser
    {
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string Office { get; set; }
        public string PreferredName { get; set; }
        public string SharePointUserID { get; set; }
        public string[] MemberGroupsCollection { get; set; }
        public string ItemSubject { get; set; }
        public string UserUpn { get; set; }
        public string UserFullName { get; set; }
        public string CATSID { get; set; }
        public string CurrentSiteUrl { get; set; }
        public string SURROGATE_ID { get; set; }
        public string SURROGATE_OF { get; internal set; }
        public string SURROGATE_OF_EMAIL { get; internal set; }
        public string SURROGATE_LOGINNAME { get; internal set; }
    }
}
