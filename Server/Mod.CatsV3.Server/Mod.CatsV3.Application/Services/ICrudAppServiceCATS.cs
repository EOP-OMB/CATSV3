using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mod.CatsV3.Application.Services
{
    public interface ICrudAppServiceCATS<TDto, TEntity> : ICrudAppService<TDto, TEntity>
    {
    }

    public interface ICrudAppServiceCATS<TDto, TEntity, TPrimaryKey> : ICrudAppService<TDto, TEntity, TPrimaryKey>
    {
    }
}
