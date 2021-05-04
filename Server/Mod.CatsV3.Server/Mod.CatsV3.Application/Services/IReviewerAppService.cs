using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public interface IReviewerAppService : ICrudAppService<ReviewerDto, Reviewer>
    {
        IEnumerable<ReviewerDto> GetAll(string upn, List<string> userDlGroup, string option, string round);
        IEnumerable<ReviewerDto> GetAll(string upn, string option, string round);
        IEnumerable<ReviewerDto> GetAll(string upn, string roundName);
        IEnumerable<ReviewerDto> GetAll(dynamic item);
    }
}
