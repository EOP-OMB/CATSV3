using Microsoft.Extensions.Logging;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using System;
using System.Collections.Generic;
using OfficeDevPnP.Core;
using System.IO;
using System.Linq;
using Mod.Framework.Configuration;
using WebProxy = System.Net.WebProxy;
using AuthenticationManager = OfficeDevPnP.Core.AuthenticationManager;
using System.Security;
using System.Net;
using Microsoft.SharePoint.Client;
using System.Runtime.CompilerServices;

namespace Mod.CatsV3.Application.Services
{
    public class SharepointService
    {
        SPConfiguration _SPConfiguration = new SPConfiguration();
        public static ClientContext clientContext;
        public static Web _srcWeb;
        const string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6InhIbG45ZmdFSHVwQjNzR0Rad3FfWE1ZZkZOMCIsImtpZCI6InhIbG45ZmdFSHVwQjNzR0Rad3FfWE1ZZkZOMCJ9.eyJhdWQiOiJkMjZiMGViNi02YzBkLTQyOTItODQxOS1mM2VhODZhNGYzMzgiLCJpc3MiOiJodHRwczovL2FkZnMub21iLmdvdi9hZGZzIiwiaWF0IjoxNTk0MDU5MzYxLCJuYmYiOjE1OTQwNTkzNjEsImV4cCI6MTU5NDA2Mjk2MSwiYXV0aF90aW1lIjoxNTk0MDQxNzA1LCJub25jZSI6IlNtOU9SalJ1YURCNU5VbE5TMUJPZFZGd2RVWmZlREpTTm01U1ZXbzNMa2hvTVdWdVNIZHZVRXMxZEZKViIsInN1YiI6InowVzlkUTZlVG50cXZkeXd5ZXFjNUxwL3IzKzNvMnVPV3h6cGZ1Ym11NTg9IiwidXBuIjoiMDAwNTc0ODZAdWlkLnBpdGMuZ292IiwidW5pcXVlX25hbWUiOiJMT0dJTlxcSkRLYXphZGkiLCJzaWQiOiJTLTEtNS0yMS0yNzE2Njg4NzAxLTIxMDQ5Mjk2NTEtMjg2NTQ4MTExOS0yMDk4NiIsImF0X2hhc2giOiJPekllVW13aHVxYmluT0JZaEZlR2hRIn0.b6KFPS8gPhsYoPi-zGrN2eS6kQPDxCf9sX4QrE1bcO76odnDJ1RYEpQUL9U2ZhtJ0EX9Q1or2tijsnFjMTylBRSLTYZx2-zMjkMHqMbtn5O7z1bVVDUNhUfvnxZs3onRrY5ptBKYcqrtL5UASY-aVpip6LvNXExt0y2OXrCWhUWcw3oHzpP3vv5Bbten3ULQryarkplTYKM9hPJ5On1NQTtZnmZ1Rh0gAoHKIe6PsXp3Dk_Mr-Jkgf0vbdQ-35mg4gjBWMQwPgbGzh1sFBbpkDDtDwlYyVNaoIXHL9YehJwLAigQsny4Q_yRR_tcyy1a5-staIAGqKecFd_pDrEGDQ";

        public SharepointService(IObjectMapper objectMapper, ILogger<IAppService> logger)
        {
            _SPConfiguration.SiteUrl = ConfigurationManager.Secrets["MOD.CatsV3.SPSiteUrl"];
            _SPConfiguration.SiteLibrary = ConfigurationManager.Secrets["MOD.CatsV3.DocumentLibraryDEV"];

            //var authManager = new AuthenticationManager();
            //clientContext = authManager.GetAzureADAccessTokenAuthenticatedContext("https://devportal.omb.gov/sites/CATS", token);

            clientContext = new ClientContext(_SPConfiguration.SiteUrl);
            //clientContext.ExecutingWebRequest += clientContext_ExecutingWebRequest;

            clientContext.ExecutingWebRequest += clientContext_ExecutingWebRequest;
            clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
            clientContext.Credentials = CredentialCache.DefaultCredentials;

            _srcWeb = clientContext.Web;
        }

        public SPConfiguration getSPConfigurations()
        {
            return _SPConfiguration;
        }

        public GroupCollection getSPGroupCollection()
        {
            //using (var clientContext = new ClientContext(_SPConfiguration.SiteUrl))
            //{

            try
            {
                Web srcWeb = clientContext.Web;

                clientContext.Load(srcWeb);
                clientContext.ExecuteQuery();
                GroupCollection groups = srcWeb.SiteGroups;
                //srcWeb.get
                clientContext.Load(groups);
                clientContext.ExecuteQuery();

                HashSet<string> permissionLevels = new HashSet<string>();

                return groups;
            }
            catch(Exception ex)
            {
                return null;
            }
                
            //}
        }

        public Folder getFolderRoleAssignments(string folderName, GroupCollection groups)
        {

           // using (var clientContext = new ClientContext(_SPConfiguration.SiteUrl))
            //{

                Web srcWeb = clientContext.Web;

                //Get the folder where to apply permissions
                Folder applyFolder = srcWeb.GetFolderByServerRelativeUrl(Path.Combine(new string[] { _SPConfiguration.SiteLibrary, folderName }));

                clientContext.Load(srcWeb);
                clientContext.Load(applyFolder, f => f.ListItemAllFields.HasUniqueRoleAssignments);
                clientContext.ExecuteQuery();

                HashSet<string> permissionLevels = new HashSet<string>();

                //do no break inheritance if adding users only
                try
                {
                    if (!applyFolder.ListItemAllFields.HasUniqueRoleAssignments)
                    {
                        //Console.WriteLine("Breaking inheritance…");
                        applyFolder.ListItemAllFields.BreakRoleInheritance(false, false);

                        //remove all the directly assigned permissions
                        permissionLevels = GetPermissionLevelByGroup();
                        //permissionLevels.Select(x => x)


                        foreach (Group grp in groups)
                        {
                            if (permissionLevels.Contains(grp.Title))
                            {
                                try
                                {
                                    applyFolder.ListItemAllFields.RoleAssignments.Groups.Remove(grp);
                                    //await clientContext.ExecuteQueryAsync();
                                }
                                catch (Exception ex)
                                {
                                    string exception = ex.Message;
                                }
                            }
                        }

                        clientContext.ExecuteQuery();

                    }
                }
                catch (Exception ex)
                {
                    //finalPackagingPermission += postCATSEmailRequest.CurrentFolder.Replace(postCATSEmailRequest.CurrentSiteUrl + "/", "") + " Failed BreakRoleInheritance " + ex.Message;
                }

                return applyFolder;

            //}

        }

        public HashSet<string> GetPermissionLevelByGroup()
        {

            //using (var clientContext = new ClientContext("CurrentSiteUrl"))
            //{
                try
                {
                    clientContext.Credentials = CredentialCache.DefaultCredentials;

                    Web srcWeb = clientContext.Web;

                    HashSet<string> permissionLevels = new HashSet<string>();

                    //Parameters to receive response from the server    
                    //RoleAssignments property should be passed in Load method to get the collection of Groups assigned to the web    
                    clientContext.Load(srcWeb, w => w.Title);
                    RoleAssignmentCollection roleAssignments = srcWeb.RoleAssignments;
                    //RoleAssignment.Member property returns the group associated to the web  
                    //RoleAssignement.RoleDefinitionBindings property returns the permissions associated to the group for the web  
                    clientContext.Load(roleAssignments, roleAssignement => roleAssignement.Include(r => r.Member, r => r.RoleDefinitionBindings.Include(x => x.Name)));
                    clientContext.ExecuteQuery();

                    //Console.WriteLine("Groups has permission to the Web: " + srcWeb.Title);
                    //Console.WriteLine("Groups Count: " + roleAssignments.Count.ToString());
                    //Console.WriteLine("Group with Permissions as follows:");
                    //foreach (RoleAssignment grp in roleAssignments)
                    //{
                    //    string strGroup = "";
                    //    strGroup += grp.Member.Title + " : ";

                    //    foreach (RoleDefinition rd in grp.RoleDefinitionBindings)
                    //    {
                    //        strGroup += rd.Name + " ";
                    //        string[] roles = new string[] { "Contribute", "LimitedAccess", "Read", "Edit" };
                    //        if ((grp.Member.Title.Contains("CATSAPP-") || grp.Member.Title.Contains("ACCESS-") || grp.Member.Title.Contains("DL-"))
                    //            && Array.IndexOf(roles, rd.Name) != -1)
                    //        {
                    //            permissionLevels.Add(grp.Member.Title);
                    //            //permissionLevels.Add(group.LoginName + ": " + definition.Name);
                    //        }
                    //    }
                    //    //Console.WriteLine(strGroup);
                    //}

                    foreach (var ra in roleAssignments)
                    {
                        clientContext.Load(ra.Member);
                        clientContext.Load(ra.RoleDefinitionBindings);

                        foreach (var definition in ra.RoleDefinitionBindings)
                        {

                            string[] roles = new string[] { "Contribute", "LimitedAccess", "Read", "Edit" };
                            string[] groupsToExclude = new string[] { "CATS Admins", "CATS Owners", "CATS Developers", "CATS Reports Admins", "CATS System Operators", "s-sqlserver" };

                            //permissionLevels.Add(ra.Member.LoginName);

                            if (roles.ToList().Any(x => x.Contains(definition.Name) == true) && groupsToExclude.ToList().Any(x => x.Contains(ra.Member.Title) == false))
                            {
                                permissionLevels.Add(ra.Member.LoginName);
                                //permissionLevels.Add(group.LoginName + ": " + definition.Name);
                            }

                            //if (roles.ToList().Any(x => x.Contains(definition.Name) == true) && groupsToExclude.ToList().Any(x => x.Contains(ra.Member.Title) == false))
                            //{
                            //    permissionLevels.Add(ra.Member.LoginName);
                            //    //permissionLevels.Add(group.LoginName + ": " + definition.Name);
                            //}

                        }

                    }

                    return permissionLevels;
                }
                catch (Exception ex)
                {
                    return null;
                }

            //}

        }

        public static RoleDefinitionBindingCollection getRoleDefinitions(string permissionLevel = "")
        {
            //Contributor permission
            RoleDefinitionBindingCollection contributorRdb = new RoleDefinitionBindingCollection(clientContext);
            contributorRdb.Add(_srcWeb.RoleDefinitions.GetByType(RoleType.Contributor));

            //CATS Edit permission
            RoleDefinitionBindingCollection catsEditRdb = new RoleDefinitionBindingCollection(clientContext);
            catsEditRdb.Add(_srcWeb.RoleDefinitions.GetByName("CATS Edit"));

            //CATS Surrogate permission
            RoleDefinitionBindingCollection surrogateEditRdb = new RoleDefinitionBindingCollection(clientContext);
            surrogateEditRdb.Add(_srcWeb.RoleDefinitions.GetByName("CATS Surrogates"));

            //Read only permission
            RoleDefinitionBindingCollection readOnlyRdb = new RoleDefinitionBindingCollection(clientContext);
            //readOnlyRdb.Add(srcWeb.RoleDefinitions.GetByType(RoleType.Reader));
            readOnlyRdb.Add(_srcWeb.RoleDefinitions.GetByName("CATS Read"));

            if (permissionLevel == "full")
                return contributorRdb;
            else if (permissionLevel == "Edit")
                return catsEditRdb;
            else if (permissionLevel == "surrogate")
                return catsEditRdb;
            else
                return readOnlyRdb;
        }

        public static Folder CheckIfFolderExistOrCreateOne(Web web, string serverRelativeUrl)
        {
            try
            {
                using (var clientContext = new ClientContext(ConfigurationManager.Secrets["MOD.CatsV3.SPSiteUrl"]))
                {
                    clientContext.ExecutingWebRequest += clientContext_ExecutingWebRequest;

                    clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
                    clientContext.Credentials = CredentialCache.DefaultCredentials;

                    Web web1 = clientContext.Web;

                    var folder = web1.GetFolderByServerRelativeUrl(serverRelativeUrl);
                    clientContext.Load(folder);
                    clientContext.ExecuteQuery();
                    return folder;
                }
            }

            catch(Microsoft.SharePoint.Client.ClientRequestException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }


            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        Console.WriteLine(sr.ReadToEnd());
                        string error = sr.ReadToEnd();
                    }
                }

                return null;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    return CreateFolder(serverRelativeUrl);
                }
                else return null;

            }
            catch (Exception ex)
            {
                return CreateFolder(serverRelativeUrl);

            }
        }

        public static Folder CreateFolder(string serverRelativeUrl)
        {
            try
            {
                string folderName = serverRelativeUrl.Split(@"/")[1]; 
                string library = serverRelativeUrl.Split(@"/")[0]; 
                var lst = _srcWeb.Lists.GetByTitle(library);
                var folder = lst.RootFolder.Folders.Add(folderName);
                folder.Update();
                clientContext.ExecuteQuery();

                return folder;
            }
            catch (ServerException ex)
            {
                return null;
            }
            
        }

        static void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
        {
            //try
            //{
            //    e.WebRequestExecutor.RequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");

            //    //add user agent
            //    e.WebRequestExecutor.WebRequest.UserAgent = "NONISV|OMB|CATSV3/1.0";

            //    bool f = e.WebRequestExecutor.WebRequest.Proxy.IsBypassed(new Uri("https://portal.omb.gov/sites/CATSV2"));

            //    //// the global proxy to an empty proxy.
            //    //IWebProxy myProxy = GlobalProxySelection.GetEmptyWebProxy();
            //    //GlobalProxySelection.Select = myProxy;

            //    ////add proxy
            //    WebProxy webProxy = new WebProxy();
            //    //webProxy.Address = new Uri("https://proxy.omb.gov:8080");
            //    webProxy.BypassProxyOnLocal = true;
            //    e.WebRequestExecutor.WebRequest.Proxy = GlobalProxySelection.GetEmptyWebProxy();//WebRequest.DefaultWebProxy;

            //    //e.WebRequestExecutor.WebRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            //}
            //catch
            //{ throw; }

            try
            {
                e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                e.WebRequestExecutor.WebRequest.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            }
            catch
            { throw; }

        }

        static void setProxy()
        {

        }
    }

    public class SPConfiguration
    {
        public string SiteUrl { get; set; }
        public string SiteLibrary { get; set; }
    }
}
