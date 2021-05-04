using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class HelpDocumentDto : DtoBase
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool? IsExternal { get; set; }
        public bool? IsDevider { get; set; }
}
}
