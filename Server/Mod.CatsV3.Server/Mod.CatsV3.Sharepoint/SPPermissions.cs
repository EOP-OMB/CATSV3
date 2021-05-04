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

namespace Mod.CatsV3.Sharepoint
{
    public class SPPermissions
    {
        string siteLibrary = ConfigurationManager.Settings["MOD.CatsV3.DocumentLibraryDEV"];//ConfigurationManager.Secrets["MOD.CatsV3.DocumentLibraryDEV"];
        ILogger _logger;

        List<IFormFile> _formFiles;
        string _webRootPath;

        public SPPermissions(string WebRootPath, ILogger logger, List<IFormFile> formFiles)
        {
            _logger = logger;
            _formFiles = formFiles;
            _webRootPath = WebRootPath;
        }

        public async Task<bool> ApplyPermissionAsync(List<string> editGroups, List<string> readOnlyGroup, List<string> editUsers, List<string> redOnlyUsers, string CATSID, string processType, CorrespondenceDto dto = null)
        {
            try
            {
                using (var unitOfWork = new ContentManagerUnitOfWork(_webRootPath, CATSID))
                {
                    //check if folder exists
                    var folder = await unitOfWork.IsFolderExists(Path.Combine(new string[] { siteLibrary, CATSID }));
                    if (folder != null)
                    {
                        var groups = await unitOfWork.GetSPGroupCollection();

                        //remove all the existing permission first
                        await unitOfWork.removePermission(folder);
                        //Apply Permissions
                        List<MyPermission> myPermissions = await GetPermissionsAsync(processType, groups, editGroups, readOnlyGroup, editUsers, redOnlyUsers, unitOfWork, dto);
                        await unitOfWork.SetPermission(folder, myPermissions);

                        return true;
                    }
                    else
                    {
                        return false;
                    }

                    

                }
            }
            catch(Exception ex)
            {
                throw;
            }

        }

        public async Task<List<MyPermission>> GetPermissionsAsync(string processType, GroupCollection groups, List<string> editGroups, List<string> readOnlyGroup, List<string> editUsers, List<string> redOnlyUsers, ContentManagerUnitOfWork unitOfWork, CorrespondenceDto dto = null)
        {
            List<MyPermission> myPermissions = new List<MyPermission>();
            readOnlyGroup = readOnlyGroup.Where(d => editGroups.Any(m => m?.ToLower().Trim() != d?.ToLower().Trim())).ToList();

            switch (processType)
            {
                case "New Letter":
                    myPermissions = SetLeadAndCopiedOfficePermission(groups, editGroups, readOnlyGroup, unitOfWork, myPermissions, dto);
                    Console.WriteLine("New Letter");
                    break;
                case "Collaboration":
                    myPermissions = SetLeadAndCopiedOfficePermission(groups, editGroups, readOnlyGroup, unitOfWork, myPermissions, dto);

                    var editDLGroups = new List<string>();
                    var readOnlyDLGroups = new List<string>();
                    var editlistUsers = new List<string>();
                    var readOnlylistUsers = new List<string>();
                    var editSurrogateUsers = new List<string>();
                    var readOnlySurrogateUsers = new List<string>();

                    //Originator get edit
                    dto.Collaboration.Originators.ToList().ForEach(o => {
                        if (dto.LetterStatus.ToLower().Trim() == "closed")
                        {
                            readOnlylistUsers.Add(o.OriginatorUpn);
                        }
                        else if (!editlistUsers.Contains(o.OriginatorUpn) && (bool)o.IsDeleted != true)
                            editlistUsers.Add(o.OriginatorUpn);
                        //adding surrogates
                        o.SurrogateOriginators.Select(s => s.SurrogateUPN).ToList().ForEach(s => {
                            if (dto.LetterStatus.ToLower().Trim() == "closed")
                            {
                                readOnlySurrogateUsers.Add(s);
                            }
                            else if (!editSurrogateUsers.Contains(s) && (bool)o.IsDeleted != true)
                                editSurrogateUsers.Add(s);
                        });
                    });
                    dto.Collaboration.Reviewers.ToList().ForEach(r => {
                        //current round reviewer get edit
                        if (r.ReviewerUPN.ToLower().Trim().Contains("dl-") && (dto.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim()))
                        {
                            if (dto.LetterStatus.ToLower().Trim() == "closed")
                            {
                                readOnlyDLGroups.Add(r.ReviewerUPN);
                            }
                            else if (!editDLGroups.Contains(r.ReviewerUPN) && r.IsDeleted != true)
                                editDLGroups.Add(r.ReviewerUPN);
                            //adding surrogates from DL members
                            r.surrogateReviewers.Select(s => s.SurrogateUPN).ToList().ForEach(s => {
                                if (dto.LetterStatus.ToLower().Trim() == "closed")
                                {
                                    readOnlySurrogateUsers.Add(s);
                                }
                                else if (!editSurrogateUsers.Contains(s) && r.IsDeleted != true)
                                    editSurrogateUsers.Add(s);
                            });
                        }
                        else if (r.ReviewerUPN.ToLower().Trim().Contains("dl-") && (dto.CurrentReview.ToLower().Trim() != r.RoundName.ToLower().Trim()))
                        {
                            if (!readOnlyDLGroups.Contains(r.ReviewerUPN) && r.IsDeleted != true)
                                readOnlyDLGroups.Add(r.ReviewerUPN);
                            //adding surrogates from DL members
                            r.surrogateReviewers.Select(s => s.SurrogateUPN).ToList().ForEach(s => {
                                if (!readOnlySurrogateUsers.Contains(s) && r.IsDeleted != true)
                                    readOnlySurrogateUsers.Add(s);
                            });
                        }
                        else if (!r.ReviewerUPN.ToLower().Trim().Contains("dl-") && (dto.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim()))
                        {
                            if (dto.LetterStatus.ToLower().Trim() == "closed")
                            {
                                readOnlylistUsers.Add(r.ReviewerUPN);
                            }
                            else if (!editlistUsers.Contains(r.ReviewerUPN) && r.IsDeleted != true)
                                editlistUsers.Add(r.ReviewerUPN);
                            //adding surrogates
                            r.surrogateReviewers.Select(s => s.SurrogateUPN).ToList().ForEach(s => {
                                if (dto.LetterStatus.ToLower().Trim() == "closed")
                                {
                                    readOnlySurrogateUsers.Add(s);
                                }
                                else if (!editSurrogateUsers.Contains(s) && r.IsDeleted != true)
                                    editSurrogateUsers.Add(s);
                            });
                        }
                        else if (!r.ReviewerUPN.ToLower().Trim().Contains("dl-") && (dto.CurrentReview.ToLower().Trim() != r.RoundName.ToLower().Trim()))
                        {
                            if (!readOnlylistUsers.Contains(r.ReviewerUPN) && r.IsDeleted != true)
                                readOnlylistUsers.Add(r.ReviewerUPN);
                            //adding surrogates
                            r.surrogateReviewers.Select(s => s.SurrogateUPN).ToList().ForEach(s => {
                                if (!readOnlySurrogateUsers.Contains(s) && r.IsDeleted != true)
                                    readOnlySurrogateUsers.Add(s);
                            });
                        }
                        //previous round reviewer get readonly
                        else
                        {
                        //    if (!readOnlylistUsers.Contains(r.ReviewerUPN))
                        //        readOnlylistUsers.Add(r.ReviewerUPN);
                        }
                    }); 

                    //FYI set readonly
                    dto.Collaboration.fYIUsers.ToList().ForEach(f => {
                        if ( f.IsDeleted != true && f.FYIUpn.ToLower().Trim().Contains("dl-") && !editDLGroups.Contains(f.FYIUpn.ToLower().Trim()) && !readOnlyDLGroups.Contains(f.FYIUpn.ToLower().Trim()))
                        {
                            readOnlyDLGroups.Add(f.FYIUpn);
                        }
                        else if (f.IsDeleted != true && !f.FYIUpn.ToLower().Trim().Contains("dl-") && !editlistUsers.Contains(f.FYIUpn.ToLower().Trim()) && !readOnlylistUsers.Contains(f.FYIUpn.ToLower().Trim()))
                        {
                            readOnlylistUsers.Add(f.FYIUpn);
                        }
                    });

                    editDLGroups = editDLGroups.Where(d => !editGroups.Any(g => g?.ToLower().Trim() == d?.ToLower().Trim())).ToList();
                    readOnlyDLGroups = readOnlyDLGroups.Where(d => !readOnlyGroup.Any(g => g?.ToLower().Trim() == d?.ToLower().Trim()) && !editDLGroups.Any(g => g?.ToLower().Trim() == d?.ToLower().Trim())).ToList();

                    //added DL group permissions
                    myPermissions = SetDLPermission(groups, editDLGroups, readOnlyDLGroups, unitOfWork, myPermissions);

                    //making sure the users are not already listed for edit permission
                    readOnlylistUsers = readOnlylistUsers.Where(m => !editlistUsers.Any(u => u?.ToLower().Trim() == m?.ToLower().Trim())).ToList();

                    //added users permissions
                    myPermissions = await SetUserBasedPermissionAsync(groups, editlistUsers, readOnlylistUsers, unitOfWork, myPermissions);
                    
                    //added surrogates users permissions
                    myPermissions = await SetUserBasedPermissionAsync(groups, editSurrogateUsers, readOnlySurrogateUsers, unitOfWork, myPermissions, true);

                    Console.WriteLine("New Collaboration");
                    break;
                default:
                    Console.WriteLine("Next Round");
                    break;
            }

            return myPermissions;
        }

        private static List<MyPermission> SetLeadAndCopiedOfficePermission(GroupCollection groups, List<string> editGroups, List<string> readOnlyGroup, ContentManagerUnitOfWork unitOfWork, List<MyPermission> myPermissions, CorrespondenceDto dto)
        {
            //Assign Edit permission
            var edit = (from a in new string[] {
                        "CATSAPP-GRP-" + editGroups.FirstOrDefault(),
                        "CATSAPP-GRP-" + editGroups.FirstOrDefault() + "-TOP",
                        "CATSAPP-CORRESPONDENCE-UNIT-TEAM" }
                        from w in groups
                        where a == (w.Title)
                        select w).ToArray();
            edit.ToList().ForEach(grp =>
            {
                if (myPermissions.Any(x => x.group.Title == grp.Title) == false)
                {
                    myPermissions.Add(new MyPermission()
                    {
                        group = grp,
                        roleDefinitions = unitOfWork.getRoleDefinitions( dto.LetterStatus.ToLower().Trim() == "closed" ? "Read" : "Edit"),
                        roleDefinition = unitOfWork.getMyRoleDefinitions(dto.LetterStatus.ToLower().Trim() == "closed" ? "Read" : "Edit")
                    });
                }
            });

            //Assign Read Only permission
            var readOnly = (from a in readOnlyGroup.ToList().Select(x => "CATSAPP-GRP-" + x)
                            from w in groups
                            where a == (w.Title)
                            select w).ToArray();
            readOnly.ToList().ForEach(grp =>
            {
                if (myPermissions.Any(x => x.group.Title == grp.Title) == false)
                {
                    myPermissions.Add(new MyPermission()
                    {
                        group = grp,
                        roleDefinitions = unitOfWork.getRoleDefinitions("Read"),
                        roleDefinition = unitOfWork.getMyRoleDefinitions("Read")
                    });
                }
            });

            return myPermissions;
        }

        private static List<MyPermission> SetDLPermission(GroupCollection groups, List<string> editGroups, List<string> readOnlyGroup, ContentManagerUnitOfWork unitOfWork, List<MyPermission> myPermissions)
        {
            //Assign Edit permission
            var edit = (from a in editGroups.ToList().Select(x => x)
                        from w in groups
                        where a == (w.Title)
                        select w).ToArray();
            edit.ToList().ForEach(grp =>
            {
                if (myPermissions.Any(x => x.group?.Title == grp.Title) == false)
                {
                    myPermissions.Add(new MyPermission()
                    {
                        group = grp,
                        roleDefinitions = unitOfWork.getRoleDefinitions("Edit"),
                        roleDefinition = unitOfWork.getMyRoleDefinitions("Edit")
                    });
                }
            });

            //Assign Read Only permission
            var readOnly = (from a in readOnlyGroup.ToList().Select(x => x)
                            from w in groups
                            where a == (w.Title)
                            select w).ToArray();
            readOnly.ToList().ForEach(grp =>
            {
                if (myPermissions.Any(x => x.group?.Title == grp.Title) == false)
                {
                    myPermissions.Add(new MyPermission()
                    {
                        group = grp,
                        roleDefinitions = unitOfWork.getRoleDefinitions("Read"),
                        roleDefinition = unitOfWork.getMyRoleDefinitions("Read")
                    });
                }
            });

            return myPermissions;
        }

        private static async Task<List<MyPermission>> SetUserBasedPermissionAsync(GroupCollection groups, List<string> editUsers, List<string> readOnlyUsers, ContentManagerUnitOfWork unitOfWork, List<MyPermission> myPermissions, bool isSurrogate = false)
        {
            //Assign Edit permission
            for (int i = 0; i < editUsers.Count; i++)
            {
                User user = await unitOfWork.ensureUserAsync(editUsers[i]); if (user != null)
                {
                    if (myPermissions.Any(x => x.user?.LoginName == user.LoginName) == false)
                    {
                        myPermissions.Add(new MyPermission()
                        {
                            user = user,
                            roleDefinitions = unitOfWork.getRoleDefinitions(isSurrogate == true ? "surrogate" : "Edit"),
                            roleDefinition = unitOfWork.getMyRoleDefinitions(isSurrogate == true ? "surrogate" : "Edit")
                        });
                    }
                }
            }

            //Assign Read Only permission
            for (int i = 0; i < readOnlyUsers.Count; i++)
            {
                User user = await unitOfWork.ensureUserAsync(readOnlyUsers[i]); if (user != null)
                {
                    if (myPermissions.Any(x => x.user?.LoginName == user.LoginName) == false)
                    {
                        myPermissions.Add(new MyPermission()
                        {
                            user = user,
                            roleDefinitions = unitOfWork.getRoleDefinitions(isSurrogate == true ? "surrogate read" : "Read"),
                            roleDefinition = unitOfWork.getMyRoleDefinitions(isSurrogate == true ? "surrogate read" : "Read")
                        });
                    }
                }
            }

            return myPermissions;
        }
    }


}
