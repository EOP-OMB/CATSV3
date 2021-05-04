using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public interface ICollaborationAppService : ICrudAppService<CollaborationDto, Collaboration>
    {
        IEnumerable<CollaborationDto> GetCorrespondence(int correspondenceId);
        IEnumerable<CollaborationDto> GetAll(string filter, string[] offices);
        IEnumerable<CollaborationDto> GetAll(string filter, string search, string[] offices);

        IEnumerable<CollaborationDto> GetAll(string filter, string search, string[] offices, Boolean isPendingCopied);
    }
}
