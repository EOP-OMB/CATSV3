using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class Folder : FullAuditedEntity
    {        

        public int CorrespondenceId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FullPath
        {
            get
            {
                string SiteSPUrl = Environment.GetEnvironmentVariable("MOD.CatsV3.SPSiteUrl");
                return System.IO.Path.Combine(SiteSPUrl, this.Name).Replace("\\", "/");
            }
        }
        public virtual ICollection<Document> Documents { get; set; }
    }
}
