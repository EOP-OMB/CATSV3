using Microsoft.AspNetCore.Http;
using Microsoft.SharePoint.Client;
using Mod.CatsV3.Application.Dtos;
using Mod.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;

namespace Mod.CatsV3.Sharepoint
{
    public class SPMaintenance
    {
        string _siteDestinationUrl;
        private string _siteUrl;
        private string _library;
        ClientContext _clientContext;
        ClientContext _clientContextDestination;

        public SPMaintenance()
        {
            string siteLibrary = "IQ Intake Work Area Library";
            string siteDestinationUrl = ConfigurationManager.Secrets["MOD.CatsV3.SPSiteUrl"];
            string siteUrl = siteDestinationUrl;

            string siteDestination = siteDestinationUrl;

            _siteUrl = siteUrl;
            _library = siteLibrary;
            _siteDestinationUrl = siteDestination;


            //source
            var clientContext = new ClientContext(_siteUrl);
            clientContext.ExecutingWebRequest += clientContext_ExecutingWebRequest;

            clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
            clientContext.Credentials = CredentialCache.DefaultCredentials;
            _clientContext = clientContext;

            //destination
            var clientContextDestination = new ClientContext(_siteDestinationUrl);
            clientContextDestination.ExecutingWebRequest += clientContext_ExecutingWebRequest;

            clientContextDestination.AuthenticationMode = ClientAuthenticationMode.Default;
            clientContextDestination.Credentials = CredentialCache.DefaultCredentials;
            _clientContextDestination = clientContextDestination;

        }

        public async System.Threading.Tasks.Task<List<CATSItem>> getAllFilesAsync(string[] folderNames)
        {
            using (var ctx = new ClientContext(_siteUrl)) {

                ctx.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                ctx.AuthenticationMode = ClientAuthenticationMode.Default;
                ctx.Credentials = CredentialCache.DefaultCredentials;

                List list = ctx.Web.Lists.GetByTitle(_library);

                ctx.Load(list, l => l.Fields);
                //ctx.Load(list.RootFolder, folder =>
                //    folder.ServerRelativeUrl,
                //    folder => folder.Files.Include(f => f.Name, f => f.ServerRelativeUrl),
                //     folder => folder.Folders.Include(fo => fo.Name, fo => fo.ServerRelativeUrl, fo => fo.Files.Include(f => f.Name, f => f.ServerRelativeUrl)));
                ctx.Load(list.RootFolder, folder => folder.Folders.Include(
                    fo => fo.Name,
                    fo => fo.TimeCreated,
                    fo => fo.ServerRelativeUrl,
                    fo => fo.Files,
                    fo => fo.Folders.Include(sub => sub.Name, sub => sub.TimeCreated, sub => sub.ServerRelativeUrl, 
                        sub => sub.Files.Include(f => f.Name, f => f.TimeCreated, f => f.ServerRelativeUrl))
                    )
                );

                //ctx.Load(list.RootFolder.Folders, folders => folders.Include(f => f.Name, f => f.ServerRelativeUrl, f => f.Files, f => f.Folders));
                //ctx.Load(list.RootFolder.Files);
                await ctx.ExecuteQueryAsync();

                FolderCollection fcol = list.RootFolder.Folders;
                List<CATSItem> lstFiles = new List<CATSItem>();

                foreach (Folder f in fcol)
                {
                    if (folderNames.ToList().Contains(f.Name))
                    {
                        CATSItem item = new CATSItem();
                        item.Files = new List<string>();
                        item.CATSID = f.Name;
                        item.serverRelativeUrl = f.ServerRelativeUrl;

                        FileCollection files = f.Files;
                        foreach (SP.File file in files)
                        {
                            item.Files.Add(file.ServerRelativeUrl);
                        }

                        FolderCollection folders = f.Folders;
                        var lastfolder = folders.ToList().OrderByDescending(d => d.TimeCreated).FirstOrDefault();
                        if (lastfolder != null)
                        {
                            item.subServerRelativeUrl = lastfolder.ServerRelativeUrl;
                            lastfolder.Files.ToList().ForEach(f =>
                            {
                                item.Files.Add(f.ServerRelativeUrl);
                            });
                        }
                        //foreach (Folder folder in folders)
                        //{
                        //    folder.Files.ToList().ForEach(f =>
                        //    {
                        //        item.Files.Add(f.ServerRelativeUrl);
                        //    });

                        //}

                        lstFiles.Add(item);
                    }
                }

                return lstFiles;
            }
        }

        public async Task<List<IFormFile>> getIFormFilesAsync(CATSItem item, Application.Dtos.CorrespondenceDto dto, IEnumerable<Application.Dtos.OldIQMasterListDto> oldMasterdata)
        {

            List<IFormFile> iFormFiles = new List<IFormFile>();
            try
            {

                using (var context = new ClientContext(_siteUrl))
                {

                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                    context.AuthenticationMode = ClientAuthenticationMode.Default;
                    context.Credentials = CredentialCache.DefaultCredentials;

                    if(item.Files.Count > 0)
                    {
                        if (item.serverRelativeUrl != null)
                        {
                            string finalDocument = oldMasterdata.Where(x => x.CATSID == dto.CATSID).Select(x => x.FinalDocuments).FirstOrDefault();
                            string referenceDocument = oldMasterdata.Where(x => x.CATSID == dto.CATSID).Select(x => x.ReferenceDocuments).FirstOrDefault();
                            string reviewDocument = oldMasterdata.Where(x => x.CATSID == dto.CATSID).Select(x => x.ReviewDocuments).FirstOrDefault();

                            FileCollection files = context.Web.GetFolderByServerRelativeUrl(item.serverRelativeUrl).Files;
                            context.Load(files);

                            await context.ExecuteQueryAsync();
                            foreach (Microsoft.SharePoint.Client.File file in files)
                            {
                                ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                context.Load(file);
                                await context.ExecuteQueryAsync();

                                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                {
                                    if (data != null)
                                    {
                                        string filePrefix = "";
                                        string fileType = setDocumentType(file.Name, oldMasterdata, dto, out filePrefix);
                                        string filename = filePrefix + "_" + file.Name;

                                        data.Value.CopyTo(mStream);
                                        var msfile = new FormFile(mStream, 0, mStream.Length, "name", file.Name);
                                        iFormFiles.Add(msfile);


                                        //file.CopyTo(ms);
                                        var fileBytes = mStream.ToArray();
                                        string s = Convert.ToBase64String(fileBytes);

                                        using (System.IO.Stream stream = new System.IO.MemoryStream(fileBytes))
                                        {
                                            try
                                            {
                                                var fci = new Microsoft.SharePoint.Client.FileCreationInformation
                                                {
                                                    Url = "filerelativeItemUrl",
                                                    ContentStream = stream,
                                                    Overwrite = true
                                                };

                                                Microsoft.SharePoint.Client.Folder folder = context.Web.GetFolderByServerRelativeUrl("folderRelativeUrl");
                                                Microsoft.SharePoint.Client.FileCollection files1 = folder.Files;
                                                Microsoft.SharePoint.Client.File file1 = files1.Add(fci);

                                                context.Load(files1);
                                                context.Load(file1);
                                                await context.ExecuteQueryAsync();
                                            }
                                            catch (Exception ex)
                                            {
                                                throw;
                                            }

                                        }


                                    }
                                }
                            }
                        }

                        if (item.subServerRelativeUrl != null)
                        {
                            FileCollection subfiles = context.Web.GetFolderByServerRelativeUrl(item.subServerRelativeUrl).Files;
                            context.Load(subfiles);

                            await context.ExecuteQueryAsync();
                            foreach (Microsoft.SharePoint.Client.File file in subfiles)
                            {
                                ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                context.Load(file);
                                await context.ExecuteQueryAsync();

                                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                {
                                    if (data != null)
                                    {
                                        string filePrefix = "";
                                        string fileType = setDocumentType(file.Name, oldMasterdata, dto, out filePrefix);
                                        string filename = filePrefix + "_" + file.Name;

                                        data.Value.CopyTo(mStream);
                                        var msfile = new FormFile(mStream, 0, mStream.Length, "name", file.Name);
                                        iFormFiles.Add(msfile);

                                        var fileBytes = mStream.ToArray();
                                        string s = Convert.ToBase64String(fileBytes);

                                        using (System.IO.Stream stream = new System.IO.MemoryStream(fileBytes))
                                        {
                                            try
                                            {
                                                var fci = new Microsoft.SharePoint.Client.FileCreationInformation
                                                {
                                                    Url = "filerelativeItemUrl",
                                                    ContentStream = stream,
                                                    Overwrite = true
                                                };

                                                Microsoft.SharePoint.Client.Folder folder = context.Web.GetFolderByServerRelativeUrl("folderRelativeUrl");
                                                Microsoft.SharePoint.Client.FileCollection files1 = folder.Files;
                                                Microsoft.SharePoint.Client.File file1 = files1.Add(fci);

                                                context.Load(files1);
                                                context.Load(file1);
                                                await context.ExecuteQueryAsync();
                                            }
                                            catch (Exception ex)
                                            {
                                                throw;
                                            }

                                        }

                                    }
                                }
                            }
                        }

                    }

                }

            }
            catch (Exception exp)
            {

            }

            return iFormFiles;
        }

        public async Task<string> processDocumentsUpload(Application.Dtos.CorrespondenceDto dto,  IEnumerable<Application.Dtos.OldIQMasterListDto> oldMasterdata)
        {

            List<IFormFile> iFormFiles = new List<IFormFile>();
            string result = "";
            try
            {

                using (var context = new ClientContext(_siteUrl))
                {

                    context.ExecutingWebRequest += clientContext_ExecutingWebRequest;
                    context.AuthenticationMode = ClientAuthenticationMode.Default;
                    context.Credentials = CredentialCache.DefaultCredentials;

                    string currentFolder = oldMasterdata.Where(x => x.CATSID == dto.CATSID).Select(x => x.FolderUrlPath).FirstOrDefault();
                    currentFolder = currentFolder != null ? currentFolder.Replace("OldFolder", "NewFolder") : "";
                    string rootFolderPath = "/sites/root folder path/" + dto.CATSID;

                    string destinationFolder = "/sites/sitename/Documents Library/" + dto.CATSID;

                    if (!string.IsNullOrEmpty(currentFolder))
                    {
                        if (string.IsNullOrEmpty(rootFolderPath) == false)
                        {

                            FileCollection files = context.Web.GetFolderByServerRelativeUrl(rootFolderPath).Files;
                            context.Load(files);

                            await context.ExecuteQueryAsync();
                            foreach (Microsoft.SharePoint.Client.File file in files)
                            {
                                ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                context.Load(file);
                                await context.ExecuteQueryAsync();

                                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                {
                                    if (data != null)
                                    {

                                        string filePrefix = "";
                                        string fileType = setDocumentType(file.Name, oldMasterdata, dto, out filePrefix);
                                        string filename = filePrefix + "_" + file.Name;

                                        data.Value.CopyTo(mStream);
                                        var msfile = new FormFile(mStream, 0, mStream.Length, "name", file.Name);
                                        iFormFiles.Add(msfile);
                                        //file.CopyTo(ms);
                                        var fileBytes = mStream.ToArray();
                                        string s = Convert.ToBase64String(fileBytes);

                                        using (System.IO.Stream stream = new System.IO.MemoryStream(fileBytes))
                                        {
                                            try
                                            {
                                                var fci = new Microsoft.SharePoint.Client.FileCreationInformation
                                                {
                                                    Url = destinationFolder + "/" + filename,
                                                    ContentStream = stream,
                                                    Overwrite = true
                                                };

                                                Microsoft.SharePoint.Client.Folder folder = await IsFolderExists(destinationFolder, dto.CATSID);
                                                //Microsoft.SharePoint.Client.Folder folder = _clientContextDestination.Web.GetFolderByServerRelativeUrl(destinationFolder);
                                                Microsoft.SharePoint.Client.FileCollection files1 = folder.Files;
                                                Microsoft.SharePoint.Client.File file1 = files1.Add(fci);

                                                _clientContextDestination.Load(files1);
                                                _clientContextDestination.Load(file1);
                                                await _clientContextDestination.ExecuteQueryAsync(); 
                                                result += dto.CATSID + " : File -> " + file.Name + "| ";
                                            }
                                            catch (Exception ex)
                                            {
                                                throw;
                                            }

                                        }

                                        ////List<IFormFile> files = new List<IFormFile>();
                                        ////Apply permission and eventually upload documents
                                        //SPUploadDocuments sPUploadDocuments = new SPUploadDocuments(contentRootPath, _logger, iFormFiles, dto.CATSID, _catsAppService.getFolderAppService(), _catsAppService.getDocumentAppService(), fileType, "New Letter");
                                        //dto = await sPUploadDocuments.DocumentsProcessing(dto, false);

                                        ////update the record in SQL
                                        //dto = _catsAppService.getCorrespondenceAppService().Update(dto);

                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(currentFolder) == false)
                        {
                            FileCollection subfiles = context.Web.GetFolderByServerRelativeUrl(currentFolder).Files;
                            context.Load(subfiles);

                            await context.ExecuteQueryAsync();
                            foreach (Microsoft.SharePoint.Client.File file in subfiles)
                            {
                                ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                context.Load(file);
                                await context.ExecuteQueryAsync();

                                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                {
                                    if (data != null)
                                    {
                                        string filePrefix = "";
                                        string fileType = setDocumentType(file.Name, oldMasterdata, dto, out filePrefix);
                                        string filename = filePrefix + "_" + file.Name;

                                        data.Value.CopyTo(mStream);
                                        var msfile = new FormFile(mStream, 0, mStream.Length, "name", file.Name);
                                        iFormFiles.Add(msfile);
                                        //file.CopyTo(ms);
                                        var fileBytes = mStream.ToArray();
                                        string s = Convert.ToBase64String(fileBytes);

                                        using (System.IO.Stream stream = new System.IO.MemoryStream(fileBytes))
                                        {
                                            try
                                            {
                                                var fci = new Microsoft.SharePoint.Client.FileCreationInformation
                                                {
                                                    Url = destinationFolder + "/" + filename,
                                                    ContentStream = stream,
                                                    Overwrite = true
                                                };

                                                Microsoft.SharePoint.Client.Folder folder = await IsFolderExists(destinationFolder, dto.CATSID);
                                                //folder = _clientContextDestination.Web.GetFolderByServerRelativeUrl(destinationFolder);
                                                Microsoft.SharePoint.Client.FileCollection files1 = folder.Files;
                                                Microsoft.SharePoint.Client.File file1 = files1.Add(fci);

                                                _clientContextDestination.Load(files1);
                                                _clientContextDestination.Load(file1);
                                                await _clientContextDestination.ExecuteQueryAsync();
                                                result += dto.CATSID + " : File -> " + file.Name + "| ";
                                            }
                                            catch (Exception ex)
                                            {
                                                throw;
                                            }

                                        }

                                    }
                                }
                            }
                        }

                    }

                }

            }
            catch (Exception exp)
            {

            }
            return result;
           
        }

        public async Task<SP.Folder> IsFolderExists(string serverRelativeUrl, string cATSID)
        {
            try
            {
                serverRelativeUrl = serverRelativeUrl.Replace("\\", "/");
                var web = _clientContextDestination.Web;
                var folder = web.GetFolderByServerRelativeUrl(serverRelativeUrl);
                _clientContextDestination.Load(folder,
                    f => f.Name,
                    f => f.ServerRelativeUrl,
                    f => f.ListItemAllFields.HasUniqueRoleAssignments,
                    f => f.ListItemAllFields.RoleAssignments.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings.Include(m => m.Name)),
                    f => f.Files.Include(file => file.Name, file => file.ServerRelativeUrl));
                await _clientContextDestination.ExecuteQueryAsync();

                return folder;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    return await CreateFolder(serverRelativeUrl, cATSID);
                }
                else
                {
                    throw new System.ArgumentException(ex.Message, Environment.UserName + " MESSAGE: " + ex.InnerException);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<SP.Folder> CreateFolder(string serverRelativeUrl, string catsID)
        {
            try
            {
                string folderName = catsID;
                string library = "Documents Library";
                var web = _clientContextDestination.Web;
                var lst = web.Lists.GetByTitle(library);
                var folder = lst.RootFolder.Folders.Add(folderName);
                folder.Update();
                await _clientContextDestination.ExecuteQueryAsync();

                return await IsFolderExists(serverRelativeUrl, catsID);
            }
            catch (ServerException ex)
            {
                throw;
            }
        }

        private string setDocumentType(string fileName, IEnumerable<Application.Dtos.OldIQMasterListDto> oldMasterdata, Application.Dtos.CorrespondenceDto dto, out string filePrefix)
        {
            string PREFIX = "";
            string documentType = "";
            string finalDocument = oldMasterdata.Where(x => x.CATSID == dto.CATSID && x.FinalDocuments != null).Select(x => Uri.UnescapeDataString(x.FinalDocuments)).FirstOrDefault();
            finalDocument = string.IsNullOrEmpty(finalDocument) ? "" : finalDocument;

            string referenceDocument = oldMasterdata.Where(x => x.CATSID == dto.CATSID && x.ReferenceDocuments != null).Select(x => Uri.UnescapeDataString(x.ReferenceDocuments)).FirstOrDefault();
            referenceDocument = string.IsNullOrEmpty(referenceDocument) ? "" : referenceDocument;

            string reviewDocument = oldMasterdata.Where(x => x.CATSID == dto.CATSID && x.ReviewDocuments != null).Select(x => Uri.UnescapeDataString(x.ReviewDocuments)).FirstOrDefault();
            reviewDocument = string.IsNullOrEmpty(reviewDocument) ? "" : reviewDocument;

            if (finalDocument.Trim().Contains(fileName) && finalDocument.Trim() != "")
            {
                documentType = "Final Document";
                PREFIX = "FINAL";
            }
            else if (reviewDocument.Trim().Contains(fileName) && reviewDocument.Trim() != "")
            {
                documentType = "Review Document";
                PREFIX = "REV";
            }
            else if (referenceDocument.Trim().Contains(fileName) && referenceDocument.Trim() != "")
            {
                documentType = "Reference Document";
                PREFIX = "REF";
            }
            else
            {
                documentType = "Reference Document";
                PREFIX = "REF";
            }


            filePrefix = PREFIX;

            return documentType;
        }

        public Task<List<IFormFile>> getIFormFile(CATSItem item)
        {
            item.Files.ToList().ForEach(filePath => {
                //using (FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(context, filePath))
                //{
                //    //// Combine destination folder with filename -- don't concatenate
                //    //// it's just wrong!
                //    //var filePath = Path.Combine(destinationFolder, item.File.Name);

                //    //// Erase existing files, cause that's how I roll
                //    //if (System.IO.File.Exists(filePath))
                //    //{
                //    //    System.IO.File.Delete(filePath);
                //    //}

                //    //// Create the file
                //    //using (var fileStream = System.IO.File.Create(filePath))
                //    //{
                //    //    fileInfo.Stream.CopyTo(fileStream);
                //    //}
                //}

            });
            throw new NotImplementedException();
        }

        public void dooooo()
        {
            using (var context = new ClientContext(_siteUrl))
            {

                // Get a reference to the SharePoint site
                var web = context.Web;

                // Get a reference to the document library
                var list = context.Web.Lists.GetByTitle(_library);

                // Get the list of files you want to export. I'm using a query
                // to find all files where the "Status" column is marked as "Approved"
                var camlQuery = new CamlQuery
                {
                    ViewXml = @"<Query><Query>"
                };

                // Retrieve the items matching the query
                var items = list.GetItems(camlQuery);

                // Make sure to load the File in the context otherwise you won't go far
                context.Load(items, items2 => items2.IncludeWithDefaultProperties
                    (item => item.DisplayName, item => item.File));

                // Execute the query and actually populate the results
                context.ExecuteQueryAsync();

                // Iterate through every file returned and save them
                foreach (var item in items)
                {
                    //THIS IS THE LINE THAT CAUSES ISSUES!!!!!!!!
                    //using (FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(context, item.File.ServerRelativeUrl))
                    //{
                    //    //// Combine destination folder with filename -- don't concatenate
                    //    //// it's just wrong!
                    //    //var filePath = Path.Combine(destinationFolder, item.File.Name);

                    //    //// Erase existing files, cause that's how I roll
                    //    //if (System.IO.File.Exists(filePath))
                    //    //{
                    //    //    System.IO.File.Delete(filePath);
                    //    //}

                    //    //// Create the file
                    //    //using (var fileStream = System.IO.File.Create(filePath))
                    //    //{
                    //    //    fileInfo.Stream.CopyTo(fileStream);
                    //    //}
                    //}
                }
            }
        }
        public async System.Threading.Tasks.Task getCatsItemsAsync()
        {
            //get all the items from Master list
            using (var clientContext = _clientContext)
            {
                SP.List oList = clientContext.Web.Lists.GetByTitle("IQ MasterList");

                var camlQuery = new CamlQuery();
                camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                        <Query>
                            <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='true' /></OrderBy>
                        </Query>
                        <ViewFields>
                            <FieldRef Name='Title'/>
                            <FieldRef Name='Modified' />
                            <FieldRef Name='CATS_x0020_ID' />
                            <FieldRef Name='LeadOffices' />
                            <FieldRef Name='OfficeToSubmitCopy' />
                        </ViewFields>
                        <RowLimit Paged='TRUE'></RowLimit></View>";


                var collListItem = oList.GetItems(camlQuery);
                //clientContext.Load(oList); 
                clientContext.Load(collListItem); 

                await clientContext.ExecuteQueryAsync();

                foreach (ListItem oListItem in collListItem)
                {
                    string catsid = oListItem["CATS_x0020_ID"].ToString();
                }

            }
        }

        public async System.Threading.Tasks.Task CallReviewerForpathAsync(string CATSID)
        {
            //get all the items from Master list
            using (var clientContext = _clientContext)
            {

                clientContext.ExecutingWebRequest += clientContext_ExecutingWebRequest;

                clientContext.AuthenticationMode = ClientAuthenticationMode.Default;
                clientContext.Credentials = CredentialCache.DefaultCredentials;

                SP.List oList = clientContext.Web.Lists.GetByTitle("IQ MasterList");

                var camlQuery = new CamlQuery();
                camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                        <Query>
                            <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='true' /></OrderBy>
                        </Query>
                        <ViewFields>
                            <FieldRef Name='Title'/>
                            <FieldRef Name='Modified' />
                            <FieldRef Name='CATS_x0020_ID' />
                            <FieldRef Name='LeadOffices' />
                            <FieldRef Name='OfficeToSubmitCopy' />
                        </ViewFields>
                        <RowLimit Paged='TRUE'></RowLimit></View>";


                var collListItem = oList.GetItems(camlQuery);
                //clientContext.Load(oList); 
                clientContext.Load(collListItem);

                await clientContext.ExecuteQueryAsync();

                foreach (ListItem oListItem in collListItem)
                {
                    string catsid = oListItem["CATS_x0020_ID"].ToString();
                }

            }
        }

       public void MoveFiles() {
            string dstUrl = "http://migrationsite/sites/sitename";
            string sourceFolder = "http://migrationsite/sites/sitename/libraryname/folder1";
            string destFolder = dstUrl + "/libraryname/folder1";
            using (ClientContext srcContext = _clientContext)
            {
                MoveCopyOptions option = new MoveCopyOptions();
                option.KeepBoth = true;
                //MoveCopyUtil.CopyFolder(srcContext, sourceFolder, destFolder, option);
                MoveCopyUtil.CopyFile(srcContext, sourceFolder, destFolder, true, option); ;
                srcContext.ExecuteQueryAsync();
            }
        }

        public void CreateFolder1(string folderName)
        {
            using (ClientContext clientContext = _clientContext)
            {
                // clientcontext.Web.Lists.GetById - This option also can be used to get the list using List GUID
                // This value is NOT List internal name
                List byTitle = clientContext.Web.Lists.GetByTitle("New list");

                // New object of "ListItemCreationInformation" class
                ListItemCreationInformation listItemCreationInformation = new ListItemCreationInformation();

                // Below are options.
                // (1) File - This will create a file in the list or document library
                // (2) Folder - This will create a foder in list(if folder creation is enabled) or documnt library
                listItemCreationInformation.UnderlyingObjectType = FileSystemObjectType.Folder;

                // This will et the internal name/path of the file/folder
                listItemCreationInformation.LeafName = folderName;

                ListItem listItem = byTitle.AddItem(listItemCreationInformation);

                // Set folder Name
                listItem["Title"] = folderName;

                //clientContext.Load(listItem => listItem.);
                listItem.Update();
                clientContext.ExecuteQueryAsync();
            }
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
    }

    public class CATSItem
    {
        public string CATSID { get; set; }
        public string serverRelativeUrl { get; set; }
        public string subServerRelativeUrl { get; set; }
        public Round  Round { get; set; }
        public List<string> Files { get; set; }

    }

    public class Round
    {
        public string RoundName { get; set; }
        public string RoundNameAcronym { get; set; }
        public string FilePath { get; set; }

    }
}
