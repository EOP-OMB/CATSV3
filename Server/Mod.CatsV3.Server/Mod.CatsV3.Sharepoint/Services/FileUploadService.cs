using Microsoft.AspNetCore.Http;
using Microsoft.SharePoint.Client;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.CatsV3.Sharepoint.Services
{
    public class FileUploadService
    {
        string siteLibrary = ConfigurationManager.Secrets["MOD.CatsV3.DocumentLibraryDEV"];
        ClientContext clientContext;
        Web web;
        List<IFormFile> _files; // incoming files
        string _WebRoothPath; //temporary location to save incoming files stream
        string _subFolder; // subfolder in the Sharepoint libary where the files will be uploaded. It's the item CATSID   
        string _fileType; // can be Reference or Review or Final documents
        public FileUploadService(
            List<IFormFile> files, 
            string WebRoothPath, 
            string subFolder, 
            ClientContext ClientContext, 
            Web web, string fileType = "")
        {
            _files = files;
            _WebRoothPath = WebRoothPath + "/Temp";
            _subFolder = subFolder;
            _fileType = fileType;
            clientContext = ClientContext;
            this.web = web;

            //check if the _WebRoothPath exists, if not creates
            if (!Directory.Exists(_WebRoothPath)){
                Directory.CreateDirectory(_WebRoothPath);
            }
        }

        public async Task<List<Document>> SaveFileStreamToSharepoint()
        {
            List<Document> documents = new List<Document>();
            foreach (var file in _files)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        string folderRelativeUrl = Path.Combine(siteLibrary, _subFolder).Replace("\\", "/"); 
                        string relativeFileUrl = Path.Combine(siteLibrary, _subFolder, file.FileName).Replace("\\", "/");

                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                        await SaveFileToSPLibrary(folderRelativeUrl, relativeFileUrl, fileBytes);

                        documents.Add(new Document()
                        {
                            Name = file.FileName,
                            Path = file.FileName,
                            Type = _fileType
                        });
                    }
                }
            }

            return documents;

        }

        public async Task<List<Document>> SaveFileStream()
        {
            try
            {
                //temporary save the files in the app root
                long size = _files.Sum(f => f.Length);
                var filePaths = new List<string>();
                foreach (var formFile in _files)
                {
                    if (formFile.Length > 0)
                    {
                        // full path to file in temp location
                        var filePath = Path.Combine(_WebRoothPath, formFile.FileName); ;//Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                        filePaths.Add(filePath);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }

                //upload to Sharepoint
                List<Document> documents = new List<Document>();
                foreach (var filePath in filePaths)
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        FileCreationInformation flciNewFile = new FileCreationInformation();
                        string fileName = _subFolder + "-REF-" + Path.GetFileName(filePath);
                        string newFileUrl = Path.Combine(siteLibrary, _subFolder, fileName).Replace("\\", "/");

                        var fileCreationInfo = new FileCreationInformation
                        {
                            ContentStream = fs,
                            Overwrite = true,
                            Url = newFileUrl
                        };

                        List docs = web.Lists.GetByTitle(siteLibrary);
                        Microsoft.SharePoint.Client.File uploadFile = docs.RootFolder.Files.Add(fileCreationInfo);

                        clientContext.Load(uploadFile);
                        await clientContext.ExecuteQueryAsync();

                        documents.Add(new Document()
                        {
                            Name = fileName,
                            Path = fileName, 
                            Type= _fileType
                        }); 
                    }
                }

                //delete all the temp files
                foreach (var filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                return documents;

            }
            catch { throw; }
        }

        async private Task<string> SaveFileToSPLibrary(string folderRelativeUrl, string relativeItemUrl, byte[] fileData)
        {

            using (System.IO.Stream stream = new System.IO.MemoryStream(fileData))
            {
                try
                {
                    var fci = new Microsoft.SharePoint.Client.FileCreationInformation
                    {
                        Url = relativeItemUrl,
                        ContentStream = stream,
                        Overwrite = true
                    };

                    Microsoft.SharePoint.Client.Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(folderRelativeUrl);
                    Microsoft.SharePoint.Client.FileCollection files = folder.Files;
                    Microsoft.SharePoint.Client.File file = files.Add(fci);

                    clientContext.Load(files);
                    clientContext.Load(file);
                    await clientContext.ExecuteQueryAsync();
                }
                catch (Exception ex)
                {

                }

                return "";

            }
        }



    }
}
