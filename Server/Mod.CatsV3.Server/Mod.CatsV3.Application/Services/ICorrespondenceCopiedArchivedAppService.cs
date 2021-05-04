using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using Mod.Framework.User.Entities;
using System.Collections.Generic;

namespace Mod.CatsV3.Application.Services
{
    public interface ICorrespondenceCopiedArchivedAppService : ICrudAppService<CorrespondenceCopiedArchivedDto, CorrespondenceCopiedArchived>
    {
    }
}
