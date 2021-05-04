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
    public class SurrogateReviewerRepository : EfRepositoryBase<CatsV3AppContext, SurrogateReviewer>, ISurrogateReviewerRepository
    {
        public SurrogateReviewerRepository(CatsV3AppContext context) : base(context)
        {
        }
        public IQueryable<SurrogateReviewer> QueryIncludingFilter(params Expression<Func<SurrogateReviewer, bool>>[] propertySelectors)
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
