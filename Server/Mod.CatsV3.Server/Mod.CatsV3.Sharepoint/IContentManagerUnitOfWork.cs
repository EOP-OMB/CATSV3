using Microsoft.SharePoint.Client;
using System;
using System.Threading.Tasks;

namespace Mod.CatsV3.Sharepoint
{
    public interface IContentManagerUnitOfWork
    {
        public Task<GroupCollection> GetSPGroupCollection();
        public Task<Folder> IsFolderExists(string serverRelativeUrl);
        public Task<Folder> CreateFolder(string serverRelativeUrl);
        public Task<Group> GetGroupMembers(string groupName);
        public Task<Folder> BreakInheritance(Folder folder);
        public Task<Folder> RestoreInRoleInheritance(Folder folder);
        public Task<bool> SetPermission(Folder folder, RoleDefinitionBindingCollection role, Object o);
        public Task<bool> removePermission(Folder folder, string[] groupnames, string[] usernames);
        public RoleDefinitionBindingCollection getRoleDefinitions(string permissionLevel = "");
        public void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e);
    }
}