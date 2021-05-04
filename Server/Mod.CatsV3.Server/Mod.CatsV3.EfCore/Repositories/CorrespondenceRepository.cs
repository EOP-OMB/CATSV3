using Microsoft.EntityFrameworkCore;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.EfCore.Repositories;
using Mod.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mod.CatsV3.EfCore.Repositories
{
    public class CorrespondenceRepository : EfRepositoryBase<CatsV3AppContext, Correspondence>, ICorrespondenceRepository
    {
        public CorrespondenceRepository(CatsV3AppContext context) : base(context)
        {

        }

        public IQueryable<Correspondence> QueryIncludingFilter(params Expression<Func<Correspondence, bool>>[] propertySelectors)
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



        public IQueryable<Correspondence> QueryExcludeFilter(int offset, int limit, string sortOrder, string sortProperty, params Expression<Func<Correspondence, bool>>[] propertySelectors)
        {
            var query = Table.AsQueryable();
            var propertyInfo = typeof(Correspondence).GetProperty(sortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (!propertySelectors.IsNullOrEmpty())
            {
                foreach (var propertySelector in propertySelectors)
                {
                    if (offset == 0 && limit == 0)
                    {
                        if (sortOrder == "asc")
                        {
                            query = query.IgnoreQueryFilters().Where(propertySelector).OrderBy(x => x.CATSID);
                        }
                        else
                        {
                            query = query.IgnoreQueryFilters().Where(propertySelector).OrderByDescending(x => x.CATSID);
                        }
                        
                    }
                    else
                    {
                        if (sortOrder == "asc")
                        {
                            query = query.IgnoreQueryFilters().Where(propertySelector).OrderBy(x => x.CATSID).Skip(offset * limit).Take(limit);
                        }
                        else
                        {
                            query = query.IgnoreQueryFilters().Where(propertySelector).OrderByDescending(x => x.CATSID).Skip(offset * limit).Take(limit);
                        }
                    }
                    
                }
            }

            return query;
        }

        public Correspondence GetById(int id)
        {
            var query = Table.AsQueryable();
            return query.IgnoreQueryFilters().Where(x => x.Id == id).FirstOrDefault();
        }

        public void AddItemToOrder()
        {

        }
    }
}
