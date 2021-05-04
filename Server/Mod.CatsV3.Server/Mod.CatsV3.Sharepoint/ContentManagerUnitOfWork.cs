using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using Mod.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Mod.CatsV3.Sharepoint.Services;
using Mod.CatsV3.Domain.Entities;
using SP = Microsoft.SharePoint.Client;

namespace Mod.CatsV3.Sharepoint
{
    public class ContentManagerUnitOfWork : IContentManagerUnitOfWork, IDisposable
    {
        private ClientContext clientContext;
        private Web web;
        private string webRootPath;
        private string subFolder;
        string siteLibrary = ConfigurationManager.Secrets["MOD.CatsV3.DocumentLibrary"];
        UserCollection collUser;

        public ContentManagerUnitOfWork(string WebRootPath, string SubFolder) 
        {
            string siteUrl = ConfigurationManager.Secrets["MOD.CatsV3.SPSiteUrl"];
            webRootPath = WebRootPath;
            subFolder = SubFolder;

            //var authManager = new OfficeDevPnP.Core.AuthenticationManager();
            //clientContext = authManager.GetWebLoginClientContext(siteUrl);
            clientContext = new ClientContext(siteUrl);
            clientContext.ExecutingWebRequest += clientContext_ExecutingWebRequest;
            clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
            clientContext.Credentials = CredentialCache.DefaultCredentials;

            web = clientContext.Web;
        }
        public void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
        {
            try
            {
                e.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                e.WebRequest.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            }
            catch
            { throw; }
        }
        public async Task<GroupCollection> GetSPGroupCollection()
        {
            try
            {
                clientContext.Load(web);
                await clientContext.ExecuteQueryAsync();
                GroupCollection sitegroups = web.SiteGroups;
                clientContext.Load(sitegroups, group => group.Include(
                            r => r.Id,
                            r => r.LoginName,
                            r => r.Title,
                            r =>
                            r.Users.Include(u => u.LoginName, u => u.Id, u => u.Title)));
                await clientContext.ExecuteQueryAsync();

                HashSet<string> permissionLevels = new HashSet<string>();

                return sitegroups;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Group> GetGroupMembers(string groupName)
        {
            try
            {
                Group group = web.SiteGroups.GetByName(groupName);
                clientContext.Load(web, w => w.Title);
                clientContext.Load(group, grp => grp.Title, grp => grp.Users);

                await clientContext.ExecuteQueryAsync();

                return group;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }


        }
        public async Task<SP.Folder> IsFolderExists(string serverRelativeUrl)
        {
            try
            {
                serverRelativeUrl = serverRelativeUrl.Replace("\\", "/");
                var folder = web.GetFolderByServerRelativeUrl(serverRelativeUrl);
                clientContext.Load(folder,
                    f => f.Name, 
                    f => f.ServerRelativeUrl,
                    f => f.ListItemAllFields.HasUniqueRoleAssignments,
                    f => f.ListItemAllFields.RoleAssignments.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings.Include(m => m.Name)),
                    f => f.Files.Include(file => file.Name, file => file.ServerRelativeUrl));
                await clientContext.ExecuteQueryAsync();

                //check if the folder has unique role assignment. if not then break Inheritance
                await BreakInheritance(folder);

                return folder;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    return await CreateFolder(serverRelativeUrl);
                }
                else
                {
                    throw new System.ArgumentException(ex.Message, Environment.UserName + " MESSAGE: " + ex.InnerException); 
                }
            }
            catch (Exception ex)
            {
                return await CreateFolder(serverRelativeUrl); 
            }
        }
        public async Task<SP.Folder> CreateFolder(string serverRelativeUrl)
        {
            try
            {
                string folderName = serverRelativeUrl.Split(@"/")[1];
                string library = serverRelativeUrl.Split(@"/")[0];
                var lst = web.Lists.GetByTitle(library);
                var folder = lst.RootFolder.Folders.Add(folderName);
                folder.Update();
                await clientContext.ExecuteQueryAsync();

                return await IsFolderExists(serverRelativeUrl);
            }
            catch (ServerException ex)
            {
                throw; 
            }
        }
        public async Task<SP.Folder> BreakInheritance(SP.Folder folder)
        {
            if (!folder.ListItemAllFields.HasUniqueRoleAssignments)
            {
                 folder.ListItemAllFields.BreakRoleInheritance(false, false);
                 await clientContext.ExecuteQueryAsync();
            }

            return folder;
        }
        public async Task<SP.Folder> RestoreInRoleInheritance(SP.Folder folder)
        {
            if (folder.ListItemAllFields.HasUniqueRoleAssignments)
            {
                folder.ListItemAllFields.ResetRoleInheritance();
                await clientContext.ExecuteQueryAsync();
            }

            return folder;
        }
        public async Task<bool> SetPermission(SP.Folder folder, List<MyPermission> myPermissions)
        {
            try
            {
                await removePermission(folder);
                foreach (var permission in myPermissions)
                {
                    var folderRoles = folder.ListItemAllFields.RoleAssignments;
                    if (permission.user != null)
                    {
                        //User user = await ensureUserAsync(permission.user.LoginName);
                        //folderRoles.Add(user, permission.roleDefinitions);
                        folderRoles.Add(permission.user, new RoleDefinitionBindingCollection(clientContext) { permission.roleDefinition });
                    }
                    else if (permission.group != null)
                    {
                        //folderRoles.Add(permission.group, permission.roleDefinitions);
                        folderRoles.Add(permission.group, new RoleDefinitionBindingCollection(clientContext) { permission.roleDefinition });
                    }
                    else
                    {
                        return false;
                    }
                }
                
                folder.Update();

                await clientContext.ExecuteQueryAsync();

            }
            catch (Exception ex) { throw; }


            return true;

            throw new NotImplementedException();
        }
        public async Task<bool> SetPermission(SP.Folder folder, RoleDefinitionBindingCollection role, Object o)
        {
            try
            {
                var folderRoles = folder.ListItemAllFields.RoleAssignments;
                if (o is User u)
                {
                    User user = await ensureUserAsync(u.LoginName);
                    folderRoles.Add(user, role);
                }
                else if (o is Group g)
                {
                    folderRoles.Add(g, role);
                }
                else
                {
                    return false;
                }
                folder.Update();

                await clientContext.ExecuteQueryAsync();

            }
            catch(Exception ex) { throw; }
            

            return true;

        }

        public async Task<bool> removePermission(SP.Folder folder)
        {
            //The below function Breaks the role assignment inheritance for the list and gives the current list its own copy of the role assignments
            if (folder.ListItemAllFields.HasUniqueRoleAssignments == true)
            {
                folder.ListItemAllFields.ResetRoleInheritance();
                folder.Update();
                await clientContext.ExecuteQueryAsync();
                folder.ListItemAllFields.BreakRoleInheritance(false, false);
                folder.Update();
                await clientContext.ExecuteQueryAsync();
            }
            else
            {
                folder.ListItemAllFields.BreakRoleInheritance(false, false);
                folder.Update();
                await clientContext.ExecuteQueryAsync();
            }

            return true;

        }
        public async Task<bool> removePermission(SP.Folder folder, string[] groupnames, string[] usernames)
        {

            var groups = GetSPGroupCollection();

            var myGroups = (from a in groupnames
                            from w in groups.Result
                            where a == (w.Title)
                            select w).ToArray();

            foreach (Group eachGroup in myGroups)
            {
                folder.ListItemAllFields.RoleAssignments.GetByPrincipal(eachGroup).DeleteObject();
                try { folder.Update(); await clientContext.ExecuteQueryAsync(); }
                catch (Exception ex) { throw; }
            }

            foreach (string userLogin in usernames)
            {
                User user = await ensureUserAsync(userLogin);
                folder.ListItemAllFields.RoleAssignments.GetByPrincipal(user).DeleteObject();
                try { folder.Update(); await clientContext.ExecuteQueryAsync(); }
                catch (Exception ex) { throw; }
            }

            return true;

        }
        public RoleDefinitionBindingCollection getRoleDefinitions(string permissionLevel = "")
        {
            //Contributor permission
            RoleDefinitionBindingCollection contributorRdb = new RoleDefinitionBindingCollection(clientContext);
            RoleDefinition contributorRoleDefinition = web.RoleDefinitions.GetByType(RoleType.Contributor);
            clientContext.Load(contributorRoleDefinition);
            contributorRdb.Add(contributorRoleDefinition);

            //CATS Edit permission
            RoleDefinitionBindingCollection catsEditRdb = new RoleDefinitionBindingCollection(clientContext);
            RoleDefinition catseditRoleDefinition = web.RoleDefinitions.GetByName("CATS Edit");
            clientContext.Load(catseditRoleDefinition);
            catsEditRdb.Add(catseditRoleDefinition);

            //CATS Surrogate permission
            RoleDefinitionBindingCollection surrogateEditRdb = new RoleDefinitionBindingCollection(clientContext);
            RoleDefinition surrogateRoleDefinition = web.RoleDefinitions.GetByName("CATS Surrogates");
            clientContext.Load(surrogateRoleDefinition);
            surrogateEditRdb.Add(surrogateRoleDefinition);

            //CATS Read Surrogate permission
            RoleDefinitionBindingCollection surrogateReadRdb = new RoleDefinitionBindingCollection(clientContext);
            RoleDefinition surrogateReadRoleDefinition = web.RoleDefinitions.GetByName("CATS Read Surrogate");
            clientContext.Load(surrogateReadRoleDefinition);
            surrogateReadRdb.Add(surrogateReadRoleDefinition);

            //Read only permission
            RoleDefinitionBindingCollection readOnlyRdb = new RoleDefinitionBindingCollection(clientContext);
            RoleDefinition readOnlRoleDefinition = web.RoleDefinitions.GetByName("CATS Read");
            clientContext.Load(readOnlRoleDefinition);
            readOnlyRdb.Add(readOnlRoleDefinition);

            if (permissionLevel == "full")
                return contributorRdb;
            else if (permissionLevel == "Edit")
                return catsEditRdb;
            else if (permissionLevel == "surrogate")
                return surrogateEditRdb;
            else if (permissionLevel == "surrogate read")
                return surrogateReadRdb;
            else
                return readOnlyRdb;
        }


        public RoleDefinition getMyRoleDefinitions(string permissionLevel = "")
        {
            //Contributor permission
            RoleDefinition contributorRoleDefinition = web.RoleDefinitions.GetByType(RoleType.Contributor);
            clientContext.Load(contributorRoleDefinition);

            //CATS Edit permission
            RoleDefinition catseditRoleDefinition = web.RoleDefinitions.GetByName("CATS Edit");
            clientContext.Load(catseditRoleDefinition);

            //CATS Surrogate permission
            RoleDefinition surrogateRoleDefinition = web.RoleDefinitions.GetByName("CATS Surrogates");
            clientContext.Load(surrogateRoleDefinition);

            //CATS Surrogate read permission
            RoleDefinition surrogateReadRoleDefinition = web.RoleDefinitions.GetByName("CATS Read Surrogate");
            clientContext.Load(surrogateReadRoleDefinition);

            //Read only permission
            RoleDefinition readOnlRoleDefinition = web.RoleDefinitions.GetByName("CATS Read");
            clientContext.Load(readOnlRoleDefinition);

            if (permissionLevel == "full")
                return contributorRoleDefinition;
            else if (permissionLevel == "Edit")
                return catseditRoleDefinition;
            else if (permissionLevel == "surrogate")
                return surrogateRoleDefinition;
            else if (permissionLevel == "surrogate read")
                return surrogateReadRoleDefinition;
            else
                return readOnlRoleDefinition;
        }
        public async Task<List<Document>> UploadFilesToSharePoint(List<IFormFile> files, string fileType = "", SP.Folder Clientfolder = null)
        {
            try
            {
                //upload files
                FileUploadService fileUploadService = new FileUploadService(files, webRootPath, subFolder, clientContext, web, fileType);
                //return await fileUploadService.SaveFileStream();
                return await fileUploadService.SaveFileStreamToSharepoint();

            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public async Task<User> ensureUserAsync(string userLogin)
        {
            userLogin = userLogin.Contains("i:0e.t|adfs|") ? userLogin : "i:0e.t|adfs|" + userLogin;
            try {
                User user = web.EnsureUser(userLogin);
                clientContext.Load(user);
                await clientContext.ExecuteQueryAsync();
                return user;
            }
            catch(Exception ex)
            {
                return null;
            }
            
        }

        public List<string> getAllLibraryFiles(string folderName)
        {
            List list = clientContext.Web.Lists.GetByTitle("Documents");

            clientContext.Load(list);
            clientContext.Load(list.RootFolder, folder =>
                folder.ServerRelativeUrl,
                folder => folder.Files.Include(f => f.Name, f => f.ServerRelativeUrl),
                 folder => folder.Folders.Include(fo => fo.Name, fo => fo.ServerRelativeUrl, fo => fo.Files.Include(f => f.Name, f => f.ServerRelativeUrl)));
            //cxt.Load(list.RootFolder.Folders);
            // cxt.Load(list.RootFolder.Files);
            //cxt.ExecuteQueryAsync();

            FolderCollection fcol = list.RootFolder.Folders;
            List<string> lstFile = new List<string>();
            foreach (SP.Folder f in fcol)
            {
                if (f.Name == folderName)
                {

                    FileCollection files = f.Files;
                    foreach (SP.File file in files)
                    {
                        lstFile.Add(file.ServerRelativeUrl);
                    }

                    FolderCollection folders = f.Folders;
                    foreach (SP.Folder folder in folders)
                    {
                        folder.Files.ToList().ForEach(f => {
                            lstFile.Add(f.ServerRelativeUrl);
                        });

                    }

                    //cxt.Load(f.Files);
                    //cxt.ExecuteQueryAsync();
                    //FileCollection fileCol = f.Files;
                    //foreach (File file in fileCol)
                    //{
                    //    lstFile.Add(file.ServerRelativeUrl);
                    //}
                }
            }

            return lstFile;
        }


        public void Dispose()
        {
            clientContext.Dispose();
        }
    }

    public static class FormFileExtensions
    {
        public static async Task<byte[]> GetBytes(this IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
    public class MyPermission
    {
        public Group group { get; set; }
        public User user { get; set; }
        public RoleDefinitionBindingCollection roleDefinitions { get; set; }
        public RoleDefinition roleDefinition { get; set; }
    }
}
