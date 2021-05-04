using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class DLGroupDto : DtoBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<DLGroupMembers> DLGroupMembers { get; set; }
    }
}
