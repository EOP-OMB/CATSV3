using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Dtos
{
    public class DepartmentDto : DepartmentBaseDto
    {
        public ICollection<DepartmentDto> Children { get; set; }
        public ICollection<EmployeeDto> Employees { get; set; }
        public bool ShouldSerializeChildren()
        {

            if (this.GetType().Name == "DepartmentDto")
                return false;
            else
                return true;
        }

        public bool ShouldSerializeEmployees()
        {

            if (this.GetType().Name == "DepartmentDto")
                return false;
            else
                return true;
        }
    }

    public class DepartmentChildDto : DepartmentBaseDto
    {

    }

    public class DepartmentBaseDto : DtoBase
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string AdSecurityGroup { get; set; }
    }
}
