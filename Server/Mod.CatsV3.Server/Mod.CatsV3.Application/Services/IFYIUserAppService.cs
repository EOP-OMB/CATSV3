using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;
namespace Mod.CatsV3.Application.Services
{
    public interface IFYIUserAppService : ICrudAppService<FYIUserDto, FYIUser>
    {
        IEnumerable<FYIUserDto> GetAll(string filter, string roundName);
        IEnumerable<FYIUserDto> GetAll(string filter);

        IEnumerable<FYIUserDto> GetAll(dynamic item);
    }
}
