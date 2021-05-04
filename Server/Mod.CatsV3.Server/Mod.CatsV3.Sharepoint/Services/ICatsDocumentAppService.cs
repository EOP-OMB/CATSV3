using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mod.CatsV3.Sharepoint.Services
{
    public interface ICatsDocumentAppService : IAppService
    {
        Task<CorrespondenceDto> uploadDocumentsAsync(List<IFormFile> formFiles, string fileType, string processType, CorrespondenceDto dto);
    }
}
