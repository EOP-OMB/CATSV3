
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Email.Models
{
    public class CATSSurrogate
    {
        public FieldUserValue CATSUser { get; set; }
        public string CATSUserSPUserID { get; set; }
        public string CATSUserUPN { get; set; }
        public FieldUserValue Surrogate { get; set; }
        public string Source { get; set; }
        public string SurrogateSPUserID { get; set; }
        public string SurrogateUPN { get; set; }

    }

    public class CATSNotificationType
    {
        public string ItemCATSID { get; set; }
        public string ItemCorrespondentName { get; set; }
        public string ItemSubject { get; set; }

    }
}
