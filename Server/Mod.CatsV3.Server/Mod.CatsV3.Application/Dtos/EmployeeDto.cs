using Mod.Framework.Application;
using Mod.Framework.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mod.CatsV3.Application.Dtos
{
    public class EmployeeDto : DtoBase
    {

        public string Ein { get; set; }
        public string Upn { get; set; }
        public string SamAccountName { get; set; }
        public string Type { get; set; }

        public string Company { get; set; }
        public string Dept { get; set; }  // Department
        public string Division { get; set; }
        public string Office { get; set; }

        public string GivenName { get; set; }
        public string MiddleName { get; set; }  // Initials
        public string Surname { get; set; }
        public string PreferredName { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string EmailAddress { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string OfficePhone { get; set; }
        public string MobilePhone { get; set; }
        public string MailNickName { get; set; }

        public bool Inactive { get; set; }
        public DateTime? InactiveDate { get; set; }

        public string ManagerEin { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }

        public int? ReportsToEmployeeId { get; set; }
        public virtual Employee ReportsTo { get; set; }

        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }
        public bool ShouldSerializeReportsTo()
        {

            if (this.GetType().Name == "EmployeeDto")
                return false;
            else
                return true;
        }
        public bool ShouldSerializeDepartment()
        {

            if (this.GetType().Name == "EmployeeDto")
                return false;
            else
                return true;
        }
    }
}


