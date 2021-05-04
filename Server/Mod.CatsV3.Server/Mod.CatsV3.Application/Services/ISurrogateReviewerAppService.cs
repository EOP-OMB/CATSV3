using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public interface ISurrogateReviewerAppService : ICrudAppService<SurrogateReviewerDto, SurrogateReviewer>
    {
        IEnumerable<SurrogateReviewerDto> GetAll(string search, bool isDeleteIncluded);
        IEnumerable<SurrogateReviewerDto> GetAll(string[] upns, bool isDeleteIncluded);
    }
}
