using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Domain.Repositories;
using Mod.CatsV3.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Mod.Framework.User.Entities;
using Mod.Framework.User.Interfaces;
using Mod.Framework.Runtime.Session;

namespace Mod.CatsV3.Application.Services
{
    public class EmployeeAppService : CrudAppService<EmployeeDto, Employee>, IEmployeeAppService
    {

        public EmployeeAppService(IEmployeeRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }

        public override IEnumerable<EmployeeDto> GetAll()
        {
            var entities = Repository.GetAll(x => (x.Dept == "OMB" || x.Company == "OMB") && x.Inactive == false);

            
            return entities.Select(MapToDto).ToList();
        }

        public override EmployeeDto Get(int id)
        {
            var employee = base.Get(id);

            return employee;
        }

        public List<EmployeeDto> GetByDepartment(int departmentId)
        {
            var list = base.GetBy(x => x.DepartmentId == departmentId).OrderBy(x => x.DisplayName).ToList();

            return list;
        }
    }
}
