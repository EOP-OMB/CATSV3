using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Application;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mod.CatsV3.Application.Services
{
    public interface ICorrespondenceAppService : ICrudAppService<CorrespondenceDto, Correspondence>
    {
        CorrespondenceDto GetById(int id);
        CorrespondenceDto RestoreDeleted(CorrespondenceDto dto);
        IEnumerable<CorrespondenceDto> GetAllForMaintenance();
        IEnumerable<CorrespondenceDto> GetAllForSurrogates(string CatsUser, string Surrogate, bool isOriginator = false);
        IEnumerable<CorrespondenceDto> GetAll(string filter, string search);
        IEnumerable<CorrespondenceDto> GetAll(string filter, string[] search);
        IEnumerable<CorrespondenceDto> GetAll(string filter, string search, string[] offices);
        CorrespondenceDto UpdateWithCollaboration(CorrespondenceDto dto);
        IEnumerable<CorrespondenceDto> GetAll(string filter, string search, int offset, int limit);
        PaginationType<CorrespondenceDto> GetAllWithPagination(string filter, string search, int offset, int limit);
        PaginationType<CorrespondenceDto> GetAllWithPagination(string filter, string search, int offset, int limit, string sortBy, string sortDirection);
        public PaginationType<CorrespondenceDto> GetAllWithPagination(CollaborationParam collaborationParam);
        PaginationType<CorrespondenceDto> GetAllDeletedWithPagination(string filter, string search, int offset, int limit, string sortProperty, string sortOrder);
    }
}
