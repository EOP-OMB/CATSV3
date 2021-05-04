using Mod.Framework.Domain.Entities.Auditing;
using Mod.Framework.User.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class EmployeeExtended : Employee
    {
        public bool ShouldSerializeDirectReports()
        {

            if (this.GetType().Name == "EmployeeProxy")
                return false;
            else
                return true;
        }
    }
}
