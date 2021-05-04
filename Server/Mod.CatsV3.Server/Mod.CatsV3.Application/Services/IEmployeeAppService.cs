using Mod.CatsV3.Application.Dtos;
using Mod.Framework.Application;
using Mod.Framework.User.Entities;
using System.Collections.Generic;

namespace Mod.CatsV3.Application.Services
{
    public interface IEmployeeAppService : ICrudAppService<EmployeeDto, Employee>
    {
        List<EmployeeDto> GetByDepartment(int id);
    }
}
