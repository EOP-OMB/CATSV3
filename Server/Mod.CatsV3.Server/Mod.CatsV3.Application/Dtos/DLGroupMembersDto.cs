using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class DLGroupMembersDto : DtoBase
    {
        public int DLGroupId { get; set; }
        public virtual DLGroup DLGroup { get; set; }
        public string UserUPN { get; set; }
        public string UserFullName { get; set; }
        public int? RoleId { get; set; }
    }
}
