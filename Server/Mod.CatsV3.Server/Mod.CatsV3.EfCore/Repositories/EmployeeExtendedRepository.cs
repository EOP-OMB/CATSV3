using Microsoft.EntityFrameworkCore;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.EfCore.Repositories;
using Mod.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mod.CatsV3.EfCore.Repositories
{
    public class EmployeeExtendedRepository : EfRepositoryBase<CatsV3AppContext, EmployeeExtended>, IEmployeeExtendedRepository
    {
        public EmployeeExtendedRepository(CatsV3AppContext context) : base(context)
        {
        }

        public IQueryable<EmployeeExtended> QueryIncludingFilter(Expression<Func<EmployeeExtended, bool>>[] propertySelectors)
        {
            var query = Table.AsQueryable();

            if (!propertySelectors.IsNullOrEmpty())
            {
                foreach (var propertySelector in propertySelectors)
                {
                    //query = query.Include(propertySelector);
                    query = query.Where(propertySelector);
                }
            }

            return query;
        }

    }
}