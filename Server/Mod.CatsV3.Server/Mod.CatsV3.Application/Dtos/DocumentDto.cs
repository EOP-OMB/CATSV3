using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class DocumentDto : DtoBase
    {
        public int? FolderId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string DeletedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsModified { get; set; }
    }
}
