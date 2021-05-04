using Mod.Framework.Application;
using System;
using System.Collections.Generic;
using System.Text;


namespace Mod.CatsV3.Application.Services
{
    public interface ICatsAppService : IAppService
    {
        ICorrespondenceAppService getCorrespondenceAppService();
        ICollaborationAppService getCollaborationAppService();
        IOriginatorAppService getOriginatorAppService();
        IReviewerAppService getReviewerAppService();
        IFYIUserAppService getFYIUserAppService();
        IFolderAppService getFolderAppService();
        IDocumentAppService getDocumentAppService();
        IEmployeeAppService getEmployeeAppService();
        IEmailNotificationLogAppService getEmailNotificationLogAppService();
        ILeadOfficeAppService getLeadOfficeAppService();
        ILeadOfficeMemberAppService getLeadOfficeMemberAppService();
        ILeadOfficeOfficeManagerAppService getLeadOfficeOfficeManagerAppService();
        IDLGroupAppService getDLGroupAppService();
        IDLGroupMembersAppService getDLGroupMembersAppServicee();
        ISurrogateOriginatorAppService geturrogateOriginatorAppService();
        ISurrogateReviewerAppService getSurrogateReviewerAppService();
        ICorrespondenceCopiedArchivedAppService getCorrespondenceCopiedArchivedAppService();
    }
}
