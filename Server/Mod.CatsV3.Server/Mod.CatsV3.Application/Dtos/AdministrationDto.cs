using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class AdministrationDto : DtoBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsCurrent { get; set; }
        public DateTime? InaugurationDate { get; set; }
    }
}
