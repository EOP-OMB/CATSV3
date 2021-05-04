using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Mod.Framework.Dependency;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.EfCore.Repositories;
using Mod.CatsV3.EfCore;
using Mod.Framework.Configuration;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Application.Dtos;
using IObjectMapper = Mod.Framework.Application.ObjectMapping.IObjectMapper;
using Mod.Framework.Serilog;
using Mod.Framework.WebApi.Extensions;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Email.Models;
using Microsoft.AspNetCore.Http.Features;
using Mod.Framework.User.Entities;
using Mod.Framework.User.Repositories;
using Mod.Framework.User.Interfaces;
using Mod.Framework.User;
using Mod.Framework.User.Dependency;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Application;
using Mod.CatsV3.Email.Interface;
using Mod.CatsV3.Sharepoint.Services;

namespace Mod.CatsV3.WebApi
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCATV3Manager(this IServiceCollection services)
        {

            services.AddDbContext<CatsV3AppContext>(options =>
            {
                options
                      .UseSqlServer(ConfigurationManager.Secrets["MOD.CatsV3.ConnectionString"])
                      .EnableDetailedErrors()
                      .UseLazyLoadingProxies();
            });

            ModUserExtensions.AddModUsers(services);

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAllHeaders",
            //          builder =>
            //          {
            //              builder.AllowAnyOrigin()
            //                     .AllowAnyHeader()
            //                     .AllowAnyMethod();
            //          });
            //});

            //this to allow the upload of large files
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
                o.MultipartBoundaryLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.BufferBodyLengthLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddSerilogServices();

            services.AddModFramework();
            services.AddAutoMapper(mc =>
            {
                mc.CreateMap<CorrespondenceDto, Correspondence>().ReverseMap();
                mc.CreateMap<CollaborationDto, Collaboration>().ReverseMap();
                mc.CreateMap<LetterTypeDto, LetterType>().ReverseMap();
                mc.CreateMap<LeadOfficeDto, LeadOffice>().ReverseMap();
                mc.CreateMap<LeadOfficeMemberDto, LeadOfficeMember>().ReverseMap();
                mc.CreateMap<LeadOfficeOfficeManagerDto, LeadOfficeOfficeManager>().ReverseMap();
                mc.CreateMap<ExternalUserDto, ExternalUser>().ReverseMap();
                mc.CreateMap<OriginatorDto, Originator>().ReverseMap();
                mc.CreateMap<ReviewerDto, Reviewer>().ReverseMap();
                mc.CreateMap<FYIUserDto, FYIUser>().ReverseMap();
                mc.CreateMap<ReviewRoundDto, ReviewRound>().ReverseMap();
                mc.CreateMap<FolderDto, Folder>().ReverseMap();
                mc.CreateMap<DocumentDto, Document>().ReverseMap();
                mc.CreateMap<SurrogateOriginatorDto, SurrogateOriginator>().ReverseMap();
                mc.CreateMap<SurrogateReviewerDto, SurrogateReviewer>().ReverseMap();
                mc.CreateMap<DLGroupMembersDto, DLGroupMembers>().ReverseMap();
                mc.CreateMap<DLGroupDto, DLGroup>().ReverseMap();
                mc.CreateMap<EmailNotificationLogDto, EmailNotificationLog>().ReverseMap();
                mc.CreateMap<EmployeeDto, Employee>().ReverseMap();
                mc.CreateMap<DepartmentDto, Department>().ReverseMap();
                mc.CreateMap<OldIQMasterListDto, OldIQMasterList>().ReverseMap();
                mc.CreateMap<HelpDocumentDto, HelpDocument>().ReverseMap();
                mc.CreateMap<CorrespondenceCopiedArchivedDto, CorrespondenceCopiedArchived>().ReverseMap();
                mc.CreateMap<CorrespondenceCopiedOfficeDto, CorrespondenceCopiedOffice>().ReverseMap();
                mc.CreateMap<AdministrationDto, Administration>().ReverseMap();
                mc.CreateMap<RoleDto, Role>().ReverseMap();
            }, typeof(Startup).Assembly);

            services.AddScoped<IObjectMapper, AutoMapperObjectMapper>();


            //services.AddTransient<ICorrespondenceAppService, CorrespondenceAppService>();
            //services.AddTransient<ICorrespondenceRepository, CorrespondenceRepository>();

            services.AddScoped<ICorrespondenceAppService, CorrespondenceAppService>();
            services.AddScoped<ICorrespondenceRepository, CorrespondenceRepository>();


            //services.AddTransient<ICollaborationAppService, CollaborationAppService>();
            //services.AddTransient<ICollaborationRepository, CollaborationRepository>();

            services.AddScoped<ICollaborationAppService, CollaborationAppService>();
            services.AddScoped<ICollaborationRepository, CollaborationRepository>();


            services.AddTransient<ILetterTypeAppService, LetterTypeAppService>();
            services.AddTransient<ILetterTypeRepository, LetterTypeRepository>();


            services.AddTransient<ILeadOfficeAppService, LeadOfficeAppService>();
            services.AddTransient<ILeadOfficeRepository, LeadOfficeRepository>();

            services.AddTransient<ILeadOfficeMemberAppService, LeadOfficeMemberAppService>();
            services.AddTransient<ILeadOfficeMemberRepository, LeadOfficeMemberRepository>();

            services.AddTransient<ILeadOfficeOfficeManagerAppService, LeadOfficeOfficeManagerAppService>();
            services.AddTransient<ILeadOfficeOfficeManagerRepository, LeadOfficeOfficeManagerRepository>();


            services.AddTransient<IExternalUserAppService, ExternalUserAppService>();
            services.AddTransient<IExternalUserRepository, ExternalUserRepository>();


            //services.AddTransient<IReviewerAppService, ReviewerAppService>();
            //services.AddTransient<IReviewerRepository, ReviewerRepository>();


            services.AddScoped<IReviewerAppService, ReviewerAppService>();
            services.AddScoped<IReviewerRepository, ReviewerRepository>();

            //services.AddScoped<IOriginatorAppService, OriginatorAppService>();
            //services.AddScoped<IOriginatorRepository, OriginatorRepository>();

            services.AddScoped<IOriginatorAppService, OriginatorAppService>();
            services.AddScoped<IOriginatorRepository, OriginatorRepository>();


            //services.AddTransient<IFYIUserAppService, FYIUserAppService>();
            //services.AddTransient<IFYIUserRepository, FYIUserRepository>();


            services.AddScoped<IFYIUserAppService, FYIUserAppService>();
            services.AddScoped<IFYIUserRepository, FYIUserRepository>();


            services.AddTransient<IDocumentAppService, DocumentAppService>();
            services.AddTransient<IDocumentRepository, DocumentRepository>();

            services.AddTransient<IFolderAppService, FolderAppService>();
            services.AddTransient<IFolderRepository, FolderRepository>();

            services.AddTransient<IReviewRoundAppService, ReviewRoundAppService>();
            services.AddTransient<IReviewRoundRepository, ReviewRoundRepository>();

            services.AddTransient<ISurrogateOriginatorAppService, SurrogateOriginatorAppService>();
            services.AddTransient<ISurrogateOriginatorRepository, SurrogateOriginatorRepository>();

            services.AddTransient<ISurrogateReviewerAppService, SurrogateReviewerAppService>();
            services.AddTransient<ISurrogateReviewerRepository, SurrogateReviewerRepository>();

            services.AddTransient<IDLGroupAppService, DLGroupAppService>();
            services.AddTransient<IDLGroupRepository, DLGroupRepository>();

            services.AddTransient<IDLGroupMembersAppService, DLGroupMembersAppService>();
            services.AddTransient<IDLGroupMembersRepository, DLGroupMembersRepository>();


            services.AddTransient<IEmailNotificationLogAppService, EmailNotificationLogAppService>();
            services.AddTransient<IEmailNotificationLogRepository, EmailNotificationLogRepository>();

            services.AddScoped<ICatsAppService, CatsAppService>();

            services.AddScoped<ICatsEmailAppService, CatsEmailAppService>();

            services.AddScoped<ICatsDocumentAppService, CatsDocumentAppService>();

            // TODO:  Automatically register transient dependencies
            services.AddTransient<IDepartmentAppService, DepartmentAppService>();
            services.AddTransient<IDepartmentRepository, DepartmentRepository>();

            services.AddTransient<IEmployeeAppService, EmployeeAppService>();
            //services.AddTransient<IEmployeeExtendedRepository, EmployeeExtendedRepository>();
            services.AddTransient<IEmployeeRepository, EmployeeRepository>();

            services.AddTransient<IOldIQMasterListAppService, OldIQMasterListAppService>();
            services.AddTransient<IOldIQMasterListRepository, OldIQMasterListRepository>();

            services.AddTransient<IHelpDocumentAppService, HelpDocumentAppService>();
            services.AddTransient<IHelpDocumentRepository, HelpDocumentRepository>();

            services.AddTransient<ICorrespondenceCopiedArchivedAppService, CorrespondenceCopiedArchivedAppService>();
            services.AddTransient<ICorrespondenceCopiedArchivedRepository, CorrespondenceCopiedArchivedRepository>();

            services.AddTransient<IAdministrationAppService, AdministrationAppService>();
            services.AddTransient<IAdministrationRepository, AdministrationRepository>();

            services.AddTransient<IRoleAppService, RoleAppService>();
            services.AddTransient<IRoleRepository, RoleRepository>();

            return services;
        }
    }
}
