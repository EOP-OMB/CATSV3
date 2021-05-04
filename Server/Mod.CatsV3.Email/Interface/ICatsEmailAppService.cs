using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Email.Models;
using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mod.CatsV3.Email.Interface
{
    public interface ICatsEmailAppService : IAppService
    {
        void sendEmailnotificationAsync(CorrespondenceDto dto, string CurrentUserUPN, CurrentBrowserUser currentBrowserUser, List<IFormFile> formFiles = null);
        void sendEmailnotificationAsync(CorrespondenceDto dto, CollaborationDto collaboration, OriginatorDto[] originators, ReviewerDto[] reviewers, FYIUserDto[] fyiusers, CurrentBrowserUser currentBrowserUser, List<IFormFile> formFiles = null);
        void sendEmailnotificationAsync(CorrespondenceDto dto, CurrentBrowserUser currentBrowserUser, List<IFormFile> formFiles = null);
        Task SenArchivedEmailToMailbox(HttpContext HttpContext);
    }
}
