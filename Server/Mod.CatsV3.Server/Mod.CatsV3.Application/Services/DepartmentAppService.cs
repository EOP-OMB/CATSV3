using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using Mod.Framework.User.Entities;
using Mod.Framework.User.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Mod.CatsV3.Application.Services
{
    public class DepartmentAppService : CrudAppService<DepartmentDto, Department>, IDepartmentAppService
    {
        public DepartmentAppService(IDepartmentRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }

        public override IEnumerable<DepartmentDto> GetAll()
        {
            var departments = base.GetAll();

            var dept = departments.FirstOrDefault();

            //SortChildren(dept);

            return departments;
        }
    }
}
