using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;


namespace Mod.CatsV3.Application.Services
{
    public interface ISurrogateOriginatorAppService : ICrudAppService<SurrogateOriginatorDto, SurrogateOriginator>
    {
        IEnumerable<SurrogateOriginatorDto> GetAll(string search, bool isDeletedIncluded);
    }
}
