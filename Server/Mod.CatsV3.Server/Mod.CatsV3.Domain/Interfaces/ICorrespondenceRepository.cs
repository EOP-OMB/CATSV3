using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mod.CatsV3.Domain.Interfaces
{
    public interface ICorrespondenceRepository : IRepository<Correspondence>
    {
        IQueryable<Correspondence> QueryIncludingFilter(params Expression<Func<Correspondence, bool>>[] propertySelectors);

        IQueryable<Correspondence> QueryExcludeFilter(int offset, int limit, string sortOrder, string sortProperty, params Expression<Func<Correspondence, bool>>[] propertySelectors);
        Correspondence GetById(int id);
        //Correspondence Update(int id, CorrespondenceDto);
    }
}
