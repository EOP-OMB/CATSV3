using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class ExternalUserDto : DtoBase
    {
        public string Title { get; set; }
        public string Name { get; set; }
    }
}
