using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class Document : FullAuditedEntity
    {
        public int? FolderId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public string FullPath
        {
            get
            {
                string SiteSPUrl = Environment.GetEnvironmentVariable("MOD.CatsV3.SPSiteUrl");
                string CATSLibrary = Environment.GetEnvironmentVariable("MOD.CatsV3.DocumentLibrary");
                return Utilities.getDocumentOpenScheme(null, null, this.Name) + (System.IO.Path.Combine(SiteSPUrl, CATSLibrary).Replace("\\", "/"));
            }
        }
    }
}
