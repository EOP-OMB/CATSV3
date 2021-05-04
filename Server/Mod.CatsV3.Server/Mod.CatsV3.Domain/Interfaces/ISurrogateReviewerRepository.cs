using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mod.CatsV3.Domain.Interfaces
{
    public interface ISurrogateReviewerRepository : IRepository<SurrogateReviewer>
    {
        IQueryable<SurrogateReviewer> QueryIncludingFilter(params Expression<Func<SurrogateReviewer, bool>>[] propertySelectors);
    }
}
