using Microsoft.EntityFrameworkCore;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.Framework.Domain.Repositories;
using Mod.Framework.EfCore.Repositories;
using Mod.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mod.CatsV3.EfCore.Repositories
{
    public class CorrespondenceCopiedArchivedRepository : EfRepositoryBase<CatsV3AppContext, CorrespondenceCopiedArchived>, ICorrespondenceCopiedArchivedRepository
    {
        public CorrespondenceCopiedArchivedRepository(CatsV3AppContext context) : base(context)
        {
        }

    }
}