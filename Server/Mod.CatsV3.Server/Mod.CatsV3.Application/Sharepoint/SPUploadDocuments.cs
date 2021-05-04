using Microsoft.SharePoint.Client;
using Mod.CatsV3.Application.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using Mod.CatsV3.Domain;
using Mod.Framework.Configuration;
using Mod.CatsV3.Sharepoint;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Application.Dtos;

namespace Mod.CatsV3.Application.Sharepoint
{
    public class SPUploadDocuments
    {

        IFolderAppService _serviceFolder;
        IDocumentAppService _serviceDocument;
        List<IFormFile> _formFiles;
        ILogger _logger;
        string _webRootPath;
        string _subFolder;
        string _fileType;
        string _processType;
        public SPUploadDocuments(string WebRootPath, 
            ILogger logger,
            List<IFormFile> formFiles, 
            string SubFolder, IFolderAppService serviceFolder, 
            IDocumentAppService serviceDocument,
            string fileType,
            string processType) {

            _logger = logger;
            _formFiles = formFiles;
            _webRootPath = WebRootPath;
            _fileType = fileType;
            _subFolder = SubFolder;
            _serviceFolder = serviceFolder;
            _serviceDocument = serviceDocument;
            _processType = processType;
        }

        public async Task<CorrespondenceDto> DocumentsProcessing(CorrespondenceDto dto, bool IsNewLetter)
        {
            //set Sharepoint library folder Permissions

            //Set first the permission
            SPPermissions permissionAndUploadFiles = new SPPermissions(_webRootPath, _logger, _formFiles);
            List<string> editGroups = new List<string>();
            editGroups.Add(dto.LeadOfficeName);
            List<string> readOnlyGroups = new List<string>();
            readOnlyGroups = dto.CopiedOfficeName == null ? new List<string>() : dto.CopiedOfficeName.Split(";").ToList();
            readOnlyGroups.Add("LA");// LA group should have at least reao permission
            readOnlyGroups.Add("LA-TOP");// LA group should have at least reao permission
            readOnlyGroups = readOnlyGroups.Where(x => editGroups.Contains(x) == false).ToList(); // make sure the group is not already assigned edit permission

            //apply permission
            if (await permissionAndUploadFiles.ApplyPermissionAsync(editGroups, readOnlyGroups, null, null, dto.CATSID, _processType, dto))
            {
                //Create folder record first if new letter
                if (IsNewLetter || dto.FolderId == 0 || dto.FolderId == null)
                {
                    FolderDto folder = _serviceFolder.Create(new FolderDto()
                    {
                        CorrespondenceId = dto.Id,
                        Name = dto.CATSID,
                        Path = dto.CATSID
                    });
                    dto.FolderId = folder.Id;
                }

                //Upload files to the Sharepoint library
                if (_formFiles.Count > 0)
                {
                    List<Document> uploadedDocuments = await UploadToSharePoint();

                    // and Insert documents to tables
                    if (uploadedDocuments.Count > 0)
                    {
                        foreach (var doc in uploadedDocuments)
                        {
                            _serviceDocument.Create(new DocumentDto()
                            {
                                FolderId = (int)dto.FolderId,
                                Name = doc.Name,
                                Path = doc.Path,
                                Type = doc.Type
                            });
                        }
                    }
                }

            }

            return dto;
        }

        public async Task<List<Document>> UploadToSharePoint()
        {
            try
            {
                using (var unitOfWork = new ContentManagerUnitOfWork(_webRootPath, _subFolder))
                {
                    List<Document> documents = new List<Document>();
                    documents = await unitOfWork.UploadFilesToSharePoint(_formFiles.ToList(), _fileType, null);

                    return documents;

                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
