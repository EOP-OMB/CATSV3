using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public interface IOriginatorAppService : ICrudAppService<OriginatorDto, Originator>
    {
        IEnumerable<OriginatorDto> GetAll(string filter, string search);
        IEnumerable<OriginatorDto> GetAll(string filter);
        IEnumerable<OriginatorDto> GetAll(dynamic item);
    }
}
