using Microsoft.Extensions.Logging;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class CatsAppService : AppService, ICatsAppService
    {
        ICollaborationAppService _collaborationAppService;
         IOriginatorAppService _originatorService;
         IReviewerAppService _reviewerService;
         IFYIUserAppService _fYIUserService;
         ICorrespondenceAppService _correspondenceService;
         IFolderAppService _serviceFolder;
         IDocumentAppService _serviceDocument;
         IEmployeeAppService _servicemployee;
         IEmailNotificationLogAppService _serviceEmailLog;
         IDLGroupAppService _dLGroupService;
         IDLGroupMembersAppService _dLGroupMembersApp;
         ILeadOfficeAppService _serviceLeadOffice;
         ILeadOfficeMemberAppService _leadOfficeMemberAppService;
         ILeadOfficeOfficeManagerAppService _leadOfficeOfficeManagerAppService;
         ISurrogateOriginatorAppService _surrogateOriginatorAppService;
         ISurrogateReviewerAppService _surrogateReviewerAppService;
         IEmailNotificationLogAppService _emailNotificationLogAppService;
         ICorrespondenceCopiedArchivedAppService _correspondenceCopiedArchivedAppService;

        public CatsAppService(IObjectMapper objectMapper, ILogger<ICatsAppService> logger, IModSession session,
            ICollaborationAppService collaborationAppService,
            IOriginatorAppService originatorService,
            IReviewerAppService reviewerService,
            IFYIUserAppService fYIUserService,
            ICorrespondenceAppService correspondenceService,
            IFolderAppService serviceFolder,
            IDocumentAppService serviceDocument,
            IEmployeeAppService servicemployee,
            IEmailNotificationLogAppService serviceEmailLog,
            ILeadOfficeAppService serviceLeadOffice,
            IDLGroupAppService dLGroupService,
            IDLGroupMembersAppService dLGroupMembersApp,
            ILeadOfficeMemberAppService leadOfficeMemberAppService,
            ILeadOfficeOfficeManagerAppService leadOfficeOfficeManagerAppService,
            ISurrogateOriginatorAppService surrogateOriginatorAppService,
            ISurrogateReviewerAppService surrogateReviewerAppService,
            ICorrespondenceCopiedArchivedAppService correspondenceCopiedArchivedAppService
            ) : base(objectMapper, null, session)
        {
            _collaborationAppService = collaborationAppService;
            _originatorService = originatorService;
            _reviewerService = reviewerService;
            _fYIUserService = fYIUserService;
            _correspondenceService = correspondenceService;
            _serviceFolder = serviceFolder;
            _serviceDocument = serviceDocument;
            _servicemployee = servicemployee;
            _serviceEmailLog = serviceEmailLog;
            _serviceLeadOffice = serviceLeadOffice;
            _dLGroupService = dLGroupService;
            _dLGroupMembersApp = dLGroupMembersApp;
            _leadOfficeMemberAppService = leadOfficeMemberAppService;
            _leadOfficeOfficeManagerAppService = leadOfficeOfficeManagerAppService;
            _surrogateOriginatorAppService = surrogateOriginatorAppService;
            _surrogateReviewerAppService = surrogateReviewerAppService;
            _correspondenceCopiedArchivedAppService = correspondenceCopiedArchivedAppService;
        }
        public ICorrespondenceAppService getCorrespondenceAppService()
        {
            return _correspondenceService;
        }
        public ICollaborationAppService getCollaborationAppService()
        {
            return _collaborationAppService;
        }
        public IOriginatorAppService getOriginatorAppService()
        {
            return _originatorService;
        }
        public IReviewerAppService getReviewerAppService()
        {
            return _reviewerService;
        }
        public IFYIUserAppService getFYIUserAppService()
        {
            return _fYIUserService;
        }
        public IFolderAppService getFolderAppService()
        {
            return _serviceFolder;
        }
        public IDocumentAppService getDocumentAppService()
        {
            return _serviceDocument;
        }
        public IEmployeeAppService getEmployeeAppService()
        {
            return _servicemployee;
        }
        public IEmailNotificationLogAppService getEmailNotificationLogAppService()
        {
            return _serviceEmailLog;
        }
        public ILeadOfficeAppService getLeadOfficeAppService()
        {
            return _serviceLeadOffice;
        }
        public IDLGroupAppService getDLGroupAppService()
        {
            return _dLGroupService;
        }
        public IDLGroupMembersAppService getDLGroupMembersAppServicee()
        {
            return _dLGroupMembersApp;
        }
        public ILeadOfficeMemberAppService getLeadOfficeMemberAppService()
        {
            return _leadOfficeMemberAppService;
        }
        public ILeadOfficeOfficeManagerAppService getLeadOfficeOfficeManagerAppService()
        {
            return _leadOfficeOfficeManagerAppService;
        }
        public ISurrogateOriginatorAppService geturrogateOriginatorAppService()
        {
            return _surrogateOriginatorAppService;
        }
        public ISurrogateReviewerAppService getSurrogateReviewerAppService()
        {
            return _surrogateReviewerAppService;
        }
        public ICorrespondenceCopiedArchivedAppService getCorrespondenceCopiedArchivedAppService()
        {
            return _correspondenceCopiedArchivedAppService;
        }
    }
}
