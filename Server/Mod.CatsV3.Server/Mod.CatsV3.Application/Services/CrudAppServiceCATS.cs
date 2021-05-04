using Microsoft.Extensions.Logging;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Domain.Entities;
using Mod.Framework.Domain.Repositories;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;

namespace Mod.CatsV3.Application.Services
{
    public abstract class CrudAppServiceCATS<TDto, TEntity> : CrudAppServiceCATS<TDto, TEntity, int>
        where TDto : IDto<int>
        where TEntity : class,  IEntity<int>
    {
        public CrudAppServiceCATS(IRepository<TEntity, int> repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
            if(session.Principal == null) {
                throw new UnauthorizedAccessException("Session Expired. Please Refresh your browser");
            }            
        }

        public override TDto Create(TDto dto)
        {
            if (!Permissions.CanCreate)
                throw new SecurityException("Access denied.  Cannot create object of type " + typeof(IEntityCATS).Name);

            var entity = MapToEntity(dto);
            entity.GetType().GetProperty("CreatedTime").SetValue(entity, DateTime.Now, null);
            entity.GetType().GetProperty("CreatedBy").SetValue(entity, Session.Principal.Upn, null);
            entity.GetType().GetProperty("ModifiedBy").SetValue(entity, Session.Principal.Upn, null);
            entity.GetType().GetProperty("ModifiedTime").SetValue(entity, DateTime.Now, null);
            entity.GetType().GetProperty("DeletedBy").SetValue(entity, null, null);
            entity.GetType().GetProperty("DeletedTime").SetValue(entity, null, null);
            entity.GetType().GetProperty("IsDeleted").SetValue(entity, false, null);

            Repository.Insert(entity);

            return MapToDto(entity);
        }

        public override TDto Update(TDto dto)
        {
            if (!Permissions.CanUpdate)
                throw new SecurityException("Access denied.  Cannot update object of type " + typeof(IEntityCATS).Name);

            PropertyInfo createdTimeInfo = dto.GetType().GetProperty("CreatedTime");
            var createdTime = createdTimeInfo.GetValue(dto, null);
            createdTime = createdTime == null || createdTime.ToString() == DateTime.MinValue.ToString() ? DateTime.Now : createdTime;

            PropertyInfo createdByInfo = dto.GetType().GetProperty("CreatedBy");
            var createdBy = createdByInfo.GetValue(dto, null) == null ? Session.Principal.Upn : createdByInfo.GetValue(dto, null);

            PropertyInfo isDeletedInfo = dto.GetType().GetProperty("IsDeleted");
            var isDeleted = isDeletedInfo.GetValue(dto, null);

            var entity = Repository.Get(dto.Id);
            dto.GetType().GetProperty("CreatedTime").SetValue(dto, createdTime, null);
            dto.GetType().GetProperty("CreatedBy").SetValue(dto, createdBy, null);
            dto.GetType().GetProperty("ModifiedBy").SetValue(dto, Session.Principal.Upn, null);
            dto.GetType().GetProperty("ModifiedTime").SetValue(dto, DateTime.Now, null);
            dto.GetType().GetProperty("DeletedBy").SetValue(dto, (bool)isDeleted == true ? Session.Principal.Upn : null, null);
            dto.GetType().GetProperty("DeletedTime").SetValue(dto, (bool)isDeleted == true ? (DateTime?)DateTime.Now : null, null);
            //entity.GetType().GetProperty("IsDeleted").SetValue(entity, false, null);

            MapToEntity(dto, entity);
            Repository.Update(entity);

            return MapToDto(entity);
        }

    }

    public abstract class CrudAppServiceCATS<TDto, TEntity, TPrimaryKey> : CrudAppService<TDto, TEntity, TPrimaryKey>
       where TDto : IDto<TPrimaryKey>
       where TEntity : class, IEntity<TPrimaryKey>
    {
        public CrudAppServiceCATS(IRepository<TEntity, TPrimaryKey> repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
            if (session.Principal == null)
            {
                throw new UnauthorizedAccessException("Session Expired. Please Refresh your browser");
            }
        }

    }
}
