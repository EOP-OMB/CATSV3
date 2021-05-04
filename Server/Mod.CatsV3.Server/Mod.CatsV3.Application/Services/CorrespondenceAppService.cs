using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;

namespace Mod.CatsV3.Application.Services
{
    public class CorrespondenceAppService : CrudAppServiceCATS<CorrespondenceDto, Correspondence>, ICorrespondenceAppService
    {
        ICorrespondenceRepository _repository;
        IOriginatorAppService _originatorAppService;
        IReviewerAppService _reviewerAppService;
        IFYIUserAppService _fYIUserAppService;
        ICollaborationRepository _collaborationRepository;
        ICorrespondenceCopiedArchivedRepository _correspondenceCopiedArchivedRepository;
        ISurrogateReviewerRepository _surrogateReviewerRepository;
        ISurrogateOriginatorRepository _surrogateOriginatorRepository;
        IModSession _session;
        List<SurrogateOriginator> surrogateOriginators;
        List<SurrogateReviewer> surrogateReviewers;
        List<DLGroupMembers> _dLGroupMembers;

        public CorrespondenceAppService(
            ICorrespondenceRepository repository, 
            IObjectMapper objectMapper, 
            ILogger<IAppService> logger, 
            IModSession session,
            ICollaborationRepository collaborationRepository ,
            IServiceScopeFactory ServiceProvider,
            ICorrespondenceCopiedArchivedRepository correspondenceCopiedArchivedRepository,
            ISurrogateReviewerRepository surrogateReviewerRepository,
            ISurrogateOriginatorRepository surrogateOriginatorRepository,
            IOriginatorAppService originatorAppService,
            IReviewerAppService reviewerAppService,
            IFYIUserAppService fYIUserAppService,
            //ICatsAppService catsAppService,
            IDLGroupMembersRepository dLGroupMembersRepository) : base(repository, objectMapper, logger, session)
        {
            _repository = repository;
            _originatorAppService = originatorAppService;
            _reviewerAppService = reviewerAppService;
            _fYIUserAppService = fYIUserAppService;
            _collaborationRepository = collaborationRepository;
            _correspondenceCopiedArchivedRepository = correspondenceCopiedArchivedRepository;
            _surrogateReviewerRepository = surrogateReviewerRepository;
            _surrogateOriginatorRepository = surrogateOriginatorRepository;
            surrogateReviewers = surrogateReviewerRepository.GetAll();
            surrogateOriginators = surrogateOriginatorRepository.GetAll();
            _dLGroupMembers = dLGroupMembersRepository.GetAll();
            _session = session;
        }

        public CorrespondenceDto GetById(int id)
        {
            var Dto = MapToDto(_repository.GetById(id));
            return Dto;
        }

        CorrespondenceDto ICorrespondenceAppService.RestoreDeleted(CorrespondenceDto dto)
        {
            var entity = _repository.GetById(dto.Id);
            entity.AdminClosureDate = dto.AdminClosureDate;
            entity.AdminClosureReason = dto.AdminClosureReason;
            entity.AdminReOpenDate = dto.AdminReOpenDate;
            entity.AdminReOpenReason = dto.AdminReOpenReason;
            entity.DeletedBy = dto.DeletedBy;
            entity.IsDeleted = dto.IsDeleted;
            entity.DeletedTime = dto.DeletedTime;
            entity = Repository.Update(entity);
            var newDto = MapToDto(entity);
            return newDto;
        }

        public IEnumerable<CorrespondenceDto> GetAllForMaintenance()
        {
            var entities = Repository.GetAll();
            entities = AddSurrogates(entities.ToList()).ToList();
            return entities.Select(MapToDto).ToList();
        }

        public IEnumerable<CorrespondenceDto> GetAllForSurrogates(string CatsUser, string Surrogate, bool isOriginator = false)
        {

            List<Correspondence> entities = new List<Correspondence>();
            try
            {
                var RevsurrogateFor = surrogateReviewers.Where(s => (s.SurrogateUPN.ToLower().Trim() == Surrogate.ToLower().Trim())).Select(s => s.CATSUserUPN).ToList();
                var OrisurrogateFor = surrogateOriginators.Where(s => s.SurrogateUPN.ToLower().Trim() == Surrogate.ToLower().Trim()).Select(s => s.CATSUserUPN).ToList();
                if (isOriginator == false)
                {
                    RevsurrogateFor = RevsurrogateFor.Where(s => s.ToLower().Trim() != CatsUser.ToLower().Trim()).ToList();
                    entities = Repository.GetAll(x => x.Collaboration.Reviewers.Any(r =>
                        r.ReviewerUPN.ToLower().Trim().Contains(CatsUser.ToLower().Trim()) &&
                        r.ReviewerUPN.ToLower().Trim().Contains(Surrogate.ToLower().Trim()) == false //&&
                        ) && //where the intended surrogate is not already a surrogate of another user assigned to this item

                        x.Collaboration.Originators.Any(r =>
                        r.OriginatorUpn.ToLower().Trim().Contains(Surrogate.ToLower().Trim()) == false)  // where surrogate is not already an originator

                    );

                    entities = entities.Where(x => x.Collaboration.Reviewers.Any(r => RevsurrogateFor.Any(s => s.ToLower().Trim().Contains(r.ReviewerUPN.ToLower().Trim())) == false && OrisurrogateFor.Any(s => s.ToLower().Trim().Contains(r.ReviewerUPN.ToLower().Trim())) == false)).ToList(); 
                }
                else
                {
                    OrisurrogateFor = OrisurrogateFor.Where(s => s.ToLower().Trim() != CatsUser.ToLower().Trim()).ToList();
                    entities = Repository.GetAll(x => x.Collaboration.Originators.Any(r =>
                        r.OriginatorUpn.ToLower().Trim().Contains(CatsUser.ToLower().Trim()) &&
                        r.OriginatorUpn.ToLower().Trim().Contains(Surrogate.ToLower().Trim()) == false
                        ) && // where the intended surrogate is not already a surrogate of another user assigned to this item

                        x.Collaboration.Reviewers.Any(r =>
                        r.ReviewerUPN.ToLower().Trim().Contains(Surrogate.ToLower().Trim()) == false)  // where surrogate is not already an originator

                    );

                    entities = entities.Where(x => x.Collaboration.Originators.Any(r => RevsurrogateFor.Any(s => s.ToLower().Trim().Contains(r.OriginatorUpn.ToLower().Trim())) == false && OrisurrogateFor.Any(s => s.ToLower().Trim().Contains(r.OriginatorUpn.ToLower().Trim())) == false)).ToList();
                }
            }
            catch(Exception ex)
            {

            }


            entities = AddSurrogates(entities.ToList()).ToList();
            return entities.Select(MapToDto).ToList();
        }

        IEnumerable<CorrespondenceDto> ICorrespondenceAppService.GetAll(string filter, string search)
        {

            if (!string.IsNullOrEmpty(search))
            {
                Boolean hasDate = false;
                DateTime dateTime = new DateTime();
                CheckIfDate(search, ref hasDate, ref dateTime);

                if (filter == "Open & Closed")
                {
                    if (hasDate)
                    {
                        var entities = Repository.GetAll(x => x.IsDeleted != true &&
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date
                            )
                        );
                        entities = AddSurrogates(entities.ToList()).ToList();
                        return entities.Select(MapToDto).ToList();
                    }
                    else
                    {

                        var entities = Repository.GetAll(x => x.IsDeleted != true &&
                           (
                               EF.Functions.Like(x.CATSID, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + search.Trim() + "%")
                           )
                       );
                        entities = AddSurrogates(entities.ToList()).ToList();
                        return entities.Select(MapToDto).ToList();
                    }
                }
                else
                {
                    if (hasDate)
                    {
                        var entities = Repository.GetAll(x => x.IsDeleted != true && x.LetterStatus == filter.Trim() &&
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date
                            )
                        );
                        entities = AddSurrogates(entities.ToList()).ToList();
                        return entities.Select(MapToDto).ToList();
                    }
                    else
                    {
                        var entities = Repository.GetAll(x => x.IsDeleted != true && x.LetterStatus == filter.Trim() &&
                           (
                               EF.Functions.Like(x.CATSID, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + search.Trim() + "%")
                            )
                        );
                        entities = AddSurrogates(entities.ToList()).ToList();
                        return entities.Select(MapToDto).ToList();

                    }
                }
            }
            else
            {

                var entities = Repository.GetAll(x => x.IsDeleted == false && x.LetterStatus == filter.Trim());
                entities = AddSurrogates(entities.ToList()).ToList();
                return entities.Select(MapToDto).ToList();
            }

        }

        IEnumerable<CorrespondenceDto> ICorrespondenceAppService.GetAll(string filter, string[] offices)
        {

            if (filter == "Open & Closed")
            {
                var entities = _repository.GetAll(x => x.IsDeleted == false);
                entities = AddSurrogates(entities.ToList()).ToList();

                return entities.Select(MapToDto).ToList();
            }
            else
            {
                var entities = _repository.GetAll(x => x.IsDeleted == false && x.LetterStatus == filter && offices.Contains(x.LeadOfficeName)); 
                entities = AddSurrogates(entities.ToList()).ToList();

                return entities.Select(MapToDto).ToList();
            }

        }

        public IEnumerable<CorrespondenceDto> GetAll(string filter, string search, string[] offices)
        {
            search = string.IsNullOrEmpty(search) ? "" : search.Trim();
            Boolean hasDate = false;
            DateTime dateTime = new DateTime();
            CheckIfDate(search, ref hasDate, ref dateTime);

            if (filter == "Open & Closed")
            {
                if (hasDate)
                {

                    var entities = Repository.GetAll(getSearchDatePredicateForFields(dateTime));
                    entities = entities.Where(x => offices.Contains(x.LeadOfficeName) || offices.Any(o =>
                    {
                        if (x.CopiedOfficeName == null) return false;
                        else return x.CopiedOfficeName.Contains(o) == true;
                    })).ToList();
                    entities = AddSurrogates(entities.ToList()).ToList();
                    return entities.Select(MapToDto).ToList();
                }
                else
                {
                    var entities = Repository.GetAll(getSearchPredicateForFields(search));
                    entities = entities.Where(x => offices.Contains(x.LeadOfficeName) || offices.Any(o =>
                    {
                        if (x.CopiedOfficeName == null) return false;
                        else return x.CopiedOfficeName.Contains(o) == true;
                    })).ToList();
                    entities = AddSurrogates(entities.ToList()).ToList();
                    return entities.Select(MapToDto).ToList();
                }
            }
            else
            {
                if (hasDate)
                {
                    var entities = Repository.GetAll(getSearchDatePredicateForFields(dateTime));
                    entities = entities.Where(x => offices.Contains(x.LeadOfficeName) || offices.Any(o =>
                    {
                        if (x.CopiedOfficeName == null) return false;
                        else return x.CopiedOfficeName.Contains(o) == true;
                    })).ToList();
                    entities = AddSurrogates(entities.ToList()).ToList();
                    return entities.Select(MapToDto).ToList();
                }
                else
                {
                    var entities = Repository.GetAll(getSearchPredicateForFields(search));
                    entities = entities.Where(x => offices.Contains(x.LeadOfficeName) || offices.Any(o =>
                    {
                        if (x.CopiedOfficeName == null) return false;
                        else return x.CopiedOfficeName.Contains(o) == true;
                    })).ToList();
                    entities = AddSurrogates(entities.ToList()).ToList();
                    return entities.Select(MapToDto).ToList();
                }
            }
        }

        public   CorrespondenceDto UpdateWithCorrespondenceCopiedArchiveds(CorrespondenceDto dto)
        {
            var entity = Repository.Get(dto.Id);

            if (dto.CorrespondenceCopiedArchiveds != null)
            {
                var archiveds =  _correspondenceCopiedArchivedRepository.GetAll(x => x.CorrespondenceId == dto.Id).OrderByDescending(x => x.Id);
                
                var deleted = dto.CorrespondenceCopiedArchiveds.Where(x => (bool)x.IsDeleted).FirstOrDefault();
                var created = dto.CorrespondenceCopiedArchiveds.Where(x => x.Id == 0).FirstOrDefault();

                if (deleted != null)
                {
                    if (archiveds != null)
                    {
                        archiveds.ToList().ForEach(x =>
                        {
                            if (x.Id == deleted.Id)
                            {
                                x.IsDeleted = true;
                                x.DeletedTime = deleted.DeletedTime;
                                x.ModifiedBy = deleted.ModifiedBy;
                                x.CreatedBy = deleted.CreatedBy;
                                x.DeletedBy = deleted.DeletedBy;
                            }
                        });

                        //entity.CorrespondenceCopiedArchiveds = archiveds.ToList();
                    }
                }

               
                if (created != null)
                {
                    CorrespondenceCopiedArchived d = new CorrespondenceCopiedArchived();
                    //archiveds.Add(ObjectMapper.Map<CorrespondenceCopiedArchivedDto, CorrespondenceCopiedArchived>(created, d));
                    //entity.CorrespondenceCopiedArchiveds = archiveds.ToList();
                }

            };
            //ObjectMapper.Map<CorrespondenceDto, Correspondence>(dto, entity)
           
            PropertyInfo[] properties = entity.GetType().GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                var propName = pi.Name;
                var entityValue = pi.GetValue(entity, null);
                if (dto.GetType().GetProperty(propName) != null)
                {
                    var dtoValue = dto.GetType().GetProperty(propName).GetValue(dto, null);
                    if (dtoValue != null)
                    {
                        if (entityValue.Equals(dtoValue) == false)
                        {
                            entityValue = dtoValue;
                            entity.GetType().GetProperty(propName).SetValue(entityValue, null);
                        }
                    }
                }
            }

            entity = Repository.Update(entity);

            var newDto = MapToDto(entity);

            return newDto;

            //return AttachAssignments(newDto.Id, newDto);
        }
        Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> ICorrespondenceAppService.GetAllDeletedWithPagination(string filter, string search, int offset, int limit, string sortProperty, string sortOrder)
        {
            var propertyInfo = typeof(Correspondence).GetProperty(sortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (!string.IsNullOrEmpty(search))
            {
                Boolean hasDate = false;
                DateTime dateTime = new DateTime();
                CheckIfDate(search, ref hasDate, ref dateTime);

                if (hasDate)
                {
                    //DATA
                    var entities = _repository.QueryExcludeFilter(offset, limit, sortOrder, sortProperty, getSearchDatePredicateForFields(dateTime, true));
                    entities = (IQueryable<Correspondence>)AddSurrogates(entities.ToList());

                    //COUNT
                    var count = _repository.QueryExcludeFilter(offset, limit, sortOrder, sortProperty, getSearchDatePredicateForFields(dateTime, true)).Count();

                    var dto = entities.Select(MapToDto).ToList();
                    Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                    return paginationType;
                }
                else
                {
                    //DATA
                    var entities = _repository.QueryExcludeFilter(offset, limit, sortOrder, sortProperty, getSearchPredicateForFields(search, true, filter));
                    entities = (IQueryable<Correspondence>)AddSurrogates(entities.ToList()).ToList();

                    //COUNT
                    var count = _repository.QueryExcludeFilter(offset, limit, sortOrder, sortProperty, getSearchPredicateForFields(search, true, filter)).Count();

                    var dto = entities.Select(MapToDto).ToList();
                    Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                    return paginationType;

                }
            }
            else
            {
                var count = _repository.QueryExcludeFilter(0, 0, sortOrder, sortProperty, x => x.IsDeleted == true).Count();//Repository.GetAll(x => x.IsDeleted == true).Count();
                IEnumerable<Correspondence> entities;
                if (sortOrder.ToLower() == "asc")
                {
                    entities = _repository.QueryExcludeFilter(offset, limit, sortOrder, sortProperty, x => x.IsDeleted == true);//.OrderBy(x => propertyInfo.GetValue(x, null)).Skip(offset * limit).Take(limit);
                    entities = AddSurrogates(entities.ToList()).ToList();
                }
                else
                {
                    entities = _repository.QueryExcludeFilter(offset, limit, sortOrder, sortProperty, x => x.IsDeleted == true);//.OrderByDescending(x => propertyInfo.GetValue(x, null)).Skip(offset * limit).Take(limit);
                    entities = AddSurrogates(entities.ToList()).ToList();
                }

                var dto = entities.Select(MapToDto).ToList();
                Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                return paginationType;
            }

        }

        Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> ICorrespondenceAppService.GetAllWithPagination(string filter, string search, int offset, int limit, string sortProperty, string sortOrder)
        {

            var propertyInfo = typeof(Correspondence).GetProperty(sortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (!string.IsNullOrEmpty(search))
            {
                Boolean hasDate = false;
                DateTime dateTime = new DateTime();
                CheckIfDate(search, ref hasDate, ref dateTime);


                if (hasDate)
                {
                    //DATA
                    var entities = Repository.GetAll(getSearchDatePredicateForFields(dateTime)).OrderByDescending(x => x.CATSID).Skip(offset * limit).Take(limit);
                    entities = AddSurrogates(entities.ToList());

                    //COUNT
                    var count = Repository.GetAll(getSearchDatePredicateForFields(dateTime)).Count();

                    var dto = entities.Select(MapToDto).ToList();
                    Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                    return paginationType;
                }
                else
                {
                    //DATA
                    var entities = Repository.GetAll(getSearchPredicateForFields(search, false, filter)).OrderByDescending(x => x.CATSID).Skip(offset * limit).Take(limit);
                    entities = AddSurrogates(entities.ToList());

                    //COUNT
                    var count = Repository.GetAll(getSearchPredicateForFields(search, false, filter)).Count();

                    var dto = entities.Select(MapToDto).ToList();
                    Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                    return paginationType;

                }
            }
            else
            {
                var count = Repository.GetAll(x => x.IsDeleted == false && x.LetterStatus == filter.Trim()).Count();
                IEnumerable<Correspondence> entities;
                if (sortOrder.ToLower() == "asc")
                {
                    entities = Repository.GetAll(x => x.IsDeleted == false && x.LetterStatus == filter.Trim()).OrderBy(x => propertyInfo.GetValue(x, null)).Skip(offset * limit).Take(limit);
                }
                else
                {
                    entities = Repository.GetAll(x => x.IsDeleted == false && x.LetterStatus == filter.Trim()).OrderByDescending(x => propertyInfo.GetValue(x, null)).Skip(offset * limit).Take(limit);
                }
                entities = AddSurrogates(entities.ToList()).ToList();
                var dto = entities.Select(MapToDto).ToList();
                Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                return paginationType;
            }

        }
        //For Collaboration and Office Data options
        public PaginationType<CorrespondenceDto> GetAllWithPagination(CollaborationParam collaborationParam)
        {

            var propertyInfo = typeof(Correspondence).GetProperty(collaborationParam.sortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            bool isNestedProInfo = false;
            if (propertyInfo == null)
            {
                PropertyInfo property = typeof(Correspondence).GetProperty("collaboration", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo nestedProperty = property.PropertyType.GetProperty(collaborationParam.sortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                propertyInfo = nestedProperty;
                isNestedProInfo = true;
            }

            if (!string.IsNullOrEmpty(collaborationParam.search))
            {
                Boolean hasDate = false;
                DateTime dateTime = new DateTime();
                CheckIfDate(collaborationParam.search, ref hasDate, ref dateTime);

                if (hasDate)
                {
                    //DATA
                    var entities = Repository.GetAll(CheckForCriteriaSearchdate(collaborationParam, dateTime)).OrderByDescending(x => x.CATSID).Skip(collaborationParam.pageNumber * collaborationParam.pageSize).Take(collaborationParam.pageSize);
                    entities = AddSurrogates(entities.ToList()).ToList();

                    //COUNT
                    var count = Repository.GetAll(CheckForCriteriaSearchdate(collaborationParam, dateTime)).Count();

                    var dto = entities.Select(MapToDto).ToList();
                    dto = setExtraSettings(dto, collaborationParam);
                    Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                    return paginationType;
                }
                else
                {
                    //DATA
                    var entities = Repository.GetAll(CheckForCriteriaSearch(collaborationParam)).OrderByDescending(x => x.CATSID).Skip(collaborationParam.pageNumber * collaborationParam.pageSize).Take(collaborationParam.pageSize);
                    entities = AddSurrogates(entities.ToList());

                    //COUNT
                    var count = Repository.GetAll(CheckForCriteriaSearch(collaborationParam)).Count();

                    var dto = entities.Select(MapToDto).ToList();
                    dto = setExtraSettings(dto, collaborationParam);
                    Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                    return paginationType;

                }
            }
            else
            {
                var count = Repository.GetAll(CheckForCriteriaNoSearch(collaborationParam)).Count();
                IEnumerable<Correspondence> entities;
                if (collaborationParam.sortOrder.ToLower() == "asc")
                {
                    if (isNestedProInfo)
                    {
                        entities = Repository.GetAll(CheckForCriteriaNoSearch(collaborationParam)).OrderBy(x => propertyInfo.GetValue(x.Collaboration, null)).Skip(collaborationParam.pageNumber * collaborationParam.pageSize).Take(collaborationParam.pageSize);
                        entities = AddSurrogates(entities.ToList()).ToList();
                    }
                    else
                    {
                        entities = Repository.GetAll(CheckForCriteriaNoSearch(collaborationParam)).OrderBy(x => propertyInfo.GetValue(x, null)).Skip(collaborationParam.pageNumber * collaborationParam.pageSize).Take(collaborationParam.pageSize);
                        entities = AddSurrogates(entities.ToList()).ToList();
                    }                    
                }
                else
                {
                    if (isNestedProInfo)
                    {
                        entities = Repository.GetAll(CheckForCriteriaNoSearch(collaborationParam)).OrderByDescending(x => propertyInfo.GetValue(x.Collaboration, null)).Skip(collaborationParam.pageNumber * collaborationParam.pageSize).Take(collaborationParam.pageSize);
                    }
                    else
                    {
                        entities = Repository.GetAll(CheckForCriteriaNoSearch(collaborationParam)).OrderByDescending(x => propertyInfo.GetValue(x, null)).Skip(collaborationParam.pageNumber * collaborationParam.pageSize).Take(collaborationParam.pageSize);
                    }
                }

                entities = AddSurrogates(entities.ToList()).ToList();
                var dto = entities.Select(MapToDto).ToList();
                dto = setExtraSettings(dto, collaborationParam);
                Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto> paginationType = new Mod.CatsV3.Domain.Services.PaginationType<CorrespondenceDto>(dto, count);
                return paginationType;
            }

        }

        public CorrespondenceDto UpdateWithCollaboration(CorrespondenceDto dto)
        {
            var entity = Repository.Get(dto.Id);

            if ((entity.Collaboration != null || entity.Collaboration == null) && dto.Collaboration == null)
            {
                PropertyInfo[] properties = entity.GetType().GetProperties();
                //PropertyInfo[] properties = entity.GetType()
                //                        .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                //                                       BindingFlags.Public | BindingFlags.NonPublic)
                //                        .Where(p => p.CanWrite )
                //                        .ToArray();



                foreach (PropertyInfo pi in properties)
                {
                    var propName = pi.Name;
                    var entityValue = pi.GetValue(entity, null);
                    PropertyInfo myPropInfo = dto.GetType().GetProperty(propName);
                    if (propName != "CorrespondenceCopiedOffice")
                    {

                        if (pi != null && myPropInfo != null && (myPropInfo.PropertyType == typeof(string) ||
                        myPropInfo.PropertyType == typeof(string) ||
                        myPropInfo.PropertyType == typeof(DateTime) ||
                        myPropInfo.PropertyType == typeof(DateTime?) ||
                        myPropInfo.PropertyType == typeof(bool) ||
                        myPropInfo.PropertyType == typeof(bool?) ||
                        myPropInfo.PropertyType == typeof(int)))
                        {
                            var dtoValue = myPropInfo.GetValue(dto, null);
                            if (dtoValue != null)
                            {
                                if (dtoValue != entityValue && dtoValue != null && pi.CanWrite == true)
                                {
                                    //pi.SetValue(entity, dtoValue, null);
                                    entity.GetType().GetProperty(propName).SetValue(entity, dtoValue, null);
                                }
                            }
                        }
                    }
                }

                entity = Repository.Update(entity);
                entity = Repository.Get(entity.Id);
                entity = AddSurrogates(entity);

                var newDto = MapToDto(entity);

                return newDto;

            }
            else if (entity.Collaboration != null && dto.Collaboration != null)
            {
                //Add / Update Originators/Reviewers/FYIUsers
                setCollaborationMembers(dto, dto.Collaboration, dto.Collaboration.Originators.ToArray(), dto.Collaboration.Reviewers.ToArray(), dto.Collaboration.fYIUsers.ToArray());
                //refresh the entity
                entity = Repository.Get(dto.Id);

                // update correspondence
                PropertyInfo[] properties = entity.GetType().GetProperties();

                foreach (PropertyInfo pi in properties)
                {
                    var propName = pi.Name;
                    if (propName != "CorrespondenceCopiedOffice")
                    {
                        var entityValue = pi.GetValue(entity, null);
                        PropertyInfo myPropInfo = dto.GetType().GetProperty(propName);

                        if (pi != null && myPropInfo != null && (myPropInfo.PropertyType == typeof(string) ||
                            myPropInfo.PropertyType == typeof(string) ||
                            myPropInfo.PropertyType == typeof(DateTime) ||
                            myPropInfo.PropertyType == typeof(DateTime?) ||
                            myPropInfo.PropertyType == typeof(bool) ||
                            myPropInfo.PropertyType == typeof(bool?) ||
                            myPropInfo.PropertyType == typeof(int)))
                        {
                            var dtoValue = myPropInfo.GetValue(dto, null);
                            if (dtoValue != null)
                            {
                                if (dtoValue != entityValue && dtoValue != null && pi.CanWrite == true)
                                {
                                    //pi.SetValue(entity, dtoValue, null);
                                    entity.GetType().GetProperty(propName).SetValue(entity, dtoValue, null);
                                }
                            }
                        }
                    }
                }

                //update collaboration
                PropertyInfo[] propertiesCollaboration = entity.Collaboration.GetType().GetProperties(); 

                foreach (PropertyInfo pi in propertiesCollaboration)
                {
                    var propName = pi.Name;
                    if (propName.ToUpper() != "CORRESPONDENCE")
                    {
                        var entityValue = pi.GetValue(entity.Collaboration, null);
                        PropertyInfo myPropInfo = dto.Collaboration.GetType().GetProperty(propName);

                        if (pi != null && myPropInfo != null && (myPropInfo.PropertyType == typeof(string) ||
                            myPropInfo.PropertyType == typeof(string) ||
                            myPropInfo.PropertyType == typeof(DateTime) ||
                            myPropInfo.PropertyType == typeof(DateTime?) ||
                            myPropInfo.PropertyType == typeof(bool) ||
                            myPropInfo.PropertyType == typeof(bool?) ||
                            myPropInfo.PropertyType == typeof(int)))
                        {
                            var dtoValue = myPropInfo.GetValue(dto.Collaboration, null);
                            if (dtoValue != null)
                            {
                                if (dtoValue != entityValue && dtoValue != null && pi.CanWrite == true)
                                {
                                    entity.Collaboration.GetType().GetProperty(propName).SetValue(entity.Collaboration, dtoValue, null);
                                }
                            }
                        }
                    }
                    
                };

                //update collaboration entity
                entity.Collaboration.CreatedBy = entity.Collaboration.CreatedBy;
                entity.Collaboration.CreatedTime = entity.Collaboration.CreatedTime;
                entity.Collaboration.ModifiedBy = Session.Principal.DisplayName;
                entity.Collaboration.ModifiedTime = DateTime.Now;
                entity.Collaboration.DeletedBy = dto.IsDeleted == true ? Session.Principal.DisplayName : null;
                entity.Collaboration.DeletedTime = dto.IsDeleted == true ? (DateTime?)dto.DeletedTime : null;
                entity.Collaboration.IsDeleted = dto.IsDeleted;
                entity.Collaboration.CurrentOriginatorsIds = string.Join(";", entity.Collaboration.Originators.ToList().Where(u => u.RoundName == dto.CurrentReview && u.IsDeleted == false).OrderBy(u => u.OriginatorName).Select(u => u.OriginatorUpn).ToArray());
                entity.Collaboration.CurrentOriginators = string.Join(";", entity.Collaboration.Originators.ToList().Where(u => u.RoundName == dto.CurrentReview && u.IsDeleted == false).OrderBy(u => u.OriginatorName).Select(u => u.OriginatorName).ToArray());
                entity.Collaboration.CurrentReviewersIds = string.Join(";", entity.Collaboration.Reviewers.ToList().Where(u => u.RoundName == dto.CurrentReview && u.IsDeleted == false).OrderBy(u => u.ReviewerName).Select(u => u.ReviewerUPN).ToArray());
                entity.Collaboration.CurrentReviewers = string.Join(";", entity.Collaboration.Reviewers.ToList().Where(u => u.RoundName == dto.CurrentReview && u.IsDeleted == false).OrderBy(u => u.ReviewerName).Select(u => u.ReviewerName).ToArray());
                entity.Collaboration.CurrentFYIUsersIds = string.Join(";", entity.Collaboration.FYIUsers.ToList().Where(u => u.RoundName == dto.CurrentReview && u.IsDeleted == false).OrderBy(u => u.FYIUserName).Select(u => u.FYIUpn).ToArray());
                entity.Collaboration.CurrentFYIUsers = string.Join(";", entity.Collaboration.FYIUsers.ToList().Where(u => u.RoundName == dto.CurrentReview && u.IsDeleted == false).OrderBy(u => u.FYIUserName).Select(u => u.FYIUserName).ToArray()); ;

                //update correspondence entity
                entity.ModifiedBy = Session.Principal.DisplayName;
                entity.ModifiedTime = DateTime.Now;
                entity.DeletedBy = dto.IsDeleted == true ? Session.Principal.DisplayName : null;
                entity.DeletedTime = dto.IsDeleted == true ? (DateTime?)dto.DeletedTime : null;
                entity.IsDeleted = dto.IsDeleted;
                entity = Repository.Update(entity);
                entity = Repository.Get(entity.Id);
                entity = AddSurrogates(entity);

                var newDto = MapToDto(entity);

                return newDto;
            }
            else
            {
                return dto;
            }
        }

        private List<CorrespondenceDto> setExtraSettings(List<CorrespondenceDto> dtos, CollaborationParam collaborationParam)
        {
            dtos.ForEach(dto =>
            {
                if (collaborationParam.roles.Contains("admin"))
                {
                    dto.officeRole = "Admin";
                }
                else if (!string.IsNullOrEmpty(dto.LeadOfficeName))
                {
                    if (collaborationParam.offices.ToLower().Trim().Contains(dto.LeadOfficeName.ToLower().Trim()) )
                    {
                        dto.officeRole = "Lead";
                    }
                    else
                    {
                        dto.officeRole = "Copied";
                    }
                }
                else if (!string.IsNullOrEmpty(dto.CopiedOfficeName))
                {
                    var d =  dto.CopiedOfficeName.ToLower().Trim().Split(";").Any(x => collaborationParam.offices.ToLower().Trim().Contains(x.ToLower().Trim()));
                    if (d == true)
                    {
                        dto.officeRole = "Copied";
                    }
                }

                //set FYI flag
                if (dto.Collaboration != null && collaborationParam.filter.ToLower() == "fyi")
                {
                    if (dto.Collaboration.fYIUsers.Any(x => x.FYIUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) || collaborationParam.dlgroups.ToLower().Trim().Contains(x.FYIUpn.ToLower().Trim())))
                    {
                        dto.IsCurrentUserFYI = true;
                        dto.Icon = "info";
                    }
                    else
                    {
                        dto.IsCurrentUserFYI = false;
                    }
                }
                else
                {
                    dto.IsCurrentUserFYI = false;
                }

            });

            if(collaborationParam.dataOption == "pending")
            {
                //dtos = dtos.Where(x => x.officeRole == "Lead").ToList();
            }
            else if (collaborationParam.dataOption == "copied")
            {
                dtos = dtos.Where(x => x.officeRole == "Copied").ToList();
            }

            return dtos;
        }
        public static Expression<Func<Correspondence, bool>> getSearchDatePredicateForFields(DateTime dateTime, bool isDeleted = false)
        {
            return (x => x.IsDeleted == isDeleted &&
                       (
                           ((DateTime)x.LetterDate).Date == dateTime.Date ||
                           ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                           ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                           x.CreatedTime.Date == dateTime.Date ||
                           x.ModifiedTime.Date == dateTime.Date
                        )
                    );
        }

        public static Expression<Func<Correspondence, bool>> getSearchPredicateForFields(string search, bool isDeleted = false, string status = "Open")
        {
            var filterByFieldExpression = checkIfByFieldSearch(search, isDeleted, status);
            if (filterByFieldExpression != null)
            {
                return filterByFieldExpression;

            }
            else
            {
                return setSearchPredicate(search, isDeleted, status);
            }
        }

        private static Expression<Func<Correspondence, bool>> checkIfByFieldSearch(string search, bool isDeleted, string status = "Open")
        {

            if (search.Contains("filterByColumn|"))
            {
                Type elementType = typeof(Correspondence);
                var parameterExpression = Expression.Parameter(elementType);

                var fieldName = search.Split("|")[1];
                var fieldValue = search.Split("|")[2];

                var propertyInfo = typeof(Correspondence).GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    return null;
                }
                else if (propertyInfo.CanWrite == false)
                {
                    return null;
                }
                else
                {
                    var lambda = GetExpression<Correspondence>(fieldName, fieldValue, isDeleted, status);
                    var compiledLambda = lambda.Compile();
                    return lambda;
                }


            }
            else
            {
                return null;

            }
        }

        private static Expression<Func<Correspondence, bool>> setSearchPredicate(string search, bool isDeleted, string status = "Open")
        {
            return (x => x.IsDeleted == isDeleted && x.LetterStatus == status &&
                                       (
                                           EF.Functions.Like(x.CATSID, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.LeadOfficeName, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.CorrespondentName, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.LetterTypeName, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.OtherSigners, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.ReviewStatus, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.CurrentReview, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.LetterSubject, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.FiscalYear, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.LetterCrossReference, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.CreatedBy, "%" + search.Trim() + "%") ||
                                           EF.Functions.Like(x.ModifiedBy, "%" + search.Trim() + "%") ||
                                           x.Collaboration.Originators.Any( d => d.OriginatorName.ToLower().Trim().Contains(search.ToLower().Trim())) ||
                                           x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(search.ToLower().Trim())) ||
                                           x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(search.ToLower().Trim()))
                                        )
                                    );
        }

        public static Expression<Func<T, bool>> GetExpression<T>(string propertyName, string propertyValue, bool isDeleted = false, string status = "Open")
        {
            Boolean hasDate = false;
            DateTime dateTime = new DateTime();
            CheckIfDate(propertyValue, ref hasDate, ref dateTime);

            var parameterExp = Expression.Parameter(typeof(T), "x");

            //IsDeleted property
            var propertyExpDeleted = Expression.Property(parameterExp, "IsDeleted");
            MethodInfo methodDelete = typeof(bool).GetMethod("Equals", new[] { typeof(bool) });
            var someValueDeleted = Expression.Constant(isDeleted, typeof(bool));
            //var expressionValue = Expression.Equal(propertyExpDeleted, someValueDeleted);
            var containsMethodExpDeleted = Expression.Call(propertyExpDeleted, methodDelete, someValueDeleted);

            //Status property
            var propertyExpStatus = Expression.Property(parameterExp, "LetterStatus");
            MethodInfo methodStatus = typeof(string).GetMethod("Equals", new[] { typeof(string) });
            var someValueStatus = Expression.Constant(status, typeof(string));
            var containsMethodExpStatus = Expression.Call(propertyExpStatus, methodStatus, someValueStatus);

            ////dynamic property
            var propertyExp = Expression.Property(parameterExp, propertyName);
            if (propertyExp.Type == typeof(string))
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var someValue = Expression.Constant(propertyValue, typeof(string));
                var containsMethodExp = Expression.Call(propertyExp, method, someValue);

                var body = Expression.AndAlso(containsMethodExpDeleted, containsMethodExpStatus);
                body = Expression.AndAlso(body, containsMethodExp);

                return Expression.Lambda<Func<T, bool>>(body, parameterExp);
                //return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
            }
            if (propertyExp.Type == typeof(Administration))
            {
                // Get property of property of complex type (Administration)
                MemberExpression memberField = Expression.PropertyOrField(propertyExp, "Name");

                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var someValue = Expression.Constant(propertyValue, typeof(string));
                var containsMethodExp = Expression.Call(memberField, method, someValue);

                var body = Expression.AndAlso(containsMethodExpDeleted, containsMethodExpStatus);
                body = Expression.AndAlso(body, containsMethodExp);

                return Expression.Lambda<Func<T, bool>>(body, parameterExp);
                //return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
            }
            else if (hasDate)
            {
                var converter = TypeDescriptor.GetConverter(propertyExp.Type);
                var result = converter.ConvertFrom(propertyValue);

                var currentDate = dateTime;
                var nextDate = currentDate.AddDays(1);
                var result2 = converter.ConvertFrom(nextDate.ToShortDateString());
                var ex1 = Expression.GreaterThanOrEqual(propertyExp, Expression.Constant(result, propertyExp.Type.Name == "DateTime" ? typeof(DateTime) : typeof(DateTime?)));
                var ex2 = Expression.LessThan(propertyExp, Expression.Constant(result2, propertyExp.Type.Name == "DateTime" ? typeof(DateTime) : typeof(DateTime?)));
                var bodyex12 = Expression.AndAlso(ex1, ex2);
                var body = Expression.AndAlso(containsMethodExpDeleted, containsMethodExpStatus);
                body = Expression.AndAlso(body, bodyex12);

                return Expression.Lambda<Func<T, bool>>(body, parameterExp);
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(propertyExp.Type);
                var result = converter.ConvertFrom(propertyValue);
                var someValue = Expression.Constant(result, hasDate ? typeof(DateTime?) : typeof(int?));
                var containsMethodExp = Expression.Equal(propertyExp, someValue);

                var body = Expression.AndAlso(containsMethodExpDeleted, containsMethodExpStatus);
                body = Expression.AndAlso(body, containsMethodExp);

                return Expression.Lambda<Func<T, bool>>(body, parameterExp);
            }
        }

        public static Expression<Func<T, bool>> GetExpressionForCollaboration<T>(string propertyName, string propertyValue, bool isDeleted = false, string status = "Open")
        {
            var parameterExp = Expression.Parameter(typeof(T), "type");

            //IsDeleted property
            var propertyExpDeleted = Expression.Property(parameterExp, "IsDeleted");
            MethodInfo methodDelete = typeof(bool).GetMethod("Equals", new[] { typeof(bool) });
            var someValueDeleted = Expression.Constant(isDeleted, typeof(bool));
            //var expressionValue = Expression.Equal(propertyExpDeleted, someValueDeleted);
            var containsMethodExpDeleted = Expression.Call(propertyExpDeleted, methodDelete, someValueDeleted);

            //Status property
            var propertyExpStatus = Expression.Property(parameterExp, "LetterStatus");
            MethodInfo methodStatus = typeof(string).GetMethod("Equals", new[] { typeof(string) });
            var someValueStatus = Expression.Constant(status, typeof(string));
            var containsMethodExpStatus = Expression.Call(propertyExpStatus, methodStatus, someValueStatus);

            //dynamic property
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(propertyValue, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            var body = Expression.AndAlso(containsMethodExpDeleted, containsMethodExpStatus);
            body = Expression.AndAlso(body, containsMethodExp);

            return Expression.Lambda<Func<T, bool>>(body, parameterExp);
        }
        public static Expression<Func<Correspondence, bool>> CheckForCriteriaNoSearch(CollaborationParam collaborationParam)
        {
            //copied && archived at the same time
            if (collaborationParam.dataOption.ToLower() == "copied" && collaborationParam.filter.ToLower() == "archived")
            {
                return (x => x.IsDeleted == false &&
                             x.CorrespondenceCopiedOffices.Any(C => collaborationParam.offices.ToLower().Trim().Contains(C.OfficeName.ToLower().Trim()) && C.CorrespondenceId == x.Id) &&
                             x.CorrespondenceCopiedArchiveds.Any(c => c.ArchivedUserUpn.Trim() == collaborationParam.userlogin.Trim())
                              );
            }
            //copied only
            else if (collaborationParam.dataOption.ToLower() == "copied")
            {
                return (x => x.IsDeleted == false &&
                             x.CorrespondenceCopiedOffices.Any(C => collaborationParam.offices.ToLower().Trim().Contains(C.OfficeName.ToLower().Trim()) && C.CorrespondenceId == x.Id) &&
                             !x.CorrespondenceCopiedArchiveds.Any(c => c.ArchivedUserUpn.Trim() == collaborationParam.userlogin.Trim())
                             );
            }
            //pending
            else if (collaborationParam.dataOption.ToLower() == "pending")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsUnderReview == false && x.IsPendingLeadOffice == true && collaborationParam.offices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()));
            }
            //collaboration
            else if (collaborationParam.dataOption.ToLower() == "collaboration")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsUnderReview == true &&
                     (collaborationParam.officemMnagerOffices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()) || 
                      x.Collaboration.Originators.Any(X => X.OriginatorUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim())))
                );
            }
            //office data
            else if (collaborationParam.dataOption == "office data")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() &&
                     (collaborationParam.offices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()))
                );
            }
            //review completed
            else if (collaborationParam.filter.ToLower() == "completed")
            {
               Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.Collaboration != null && //any , but not deleted
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim())) && // assigned to the current users
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.ToLower().Trim() == collaborationParam.userlogin.ToLower().Trim()).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().Trim() == collaborationParam.filter.ToLower().Trim()// current users maximum record status is  "Completed"
               );
                return predicate;            }
            //draft
            else if (collaborationParam.filter.ToLower() == "draft")
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && x.LetterStatus.ToLower().Trim() == "Open" && //not deleted, open, in clearance
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) && X.RoundName.ToLower().Trim() == x.CurrentReview.ToLower().Trim()) && // assigned to the current user and round name equals current item status
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.Trim() == collaborationParam.userlogin.Trim()).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().Trim() == collaborationParam.filter.ToLower().Trim()// current users maximum record status is  "draft"
                 );
                return predicate;
            }
            //Review FYI
            else if (collaborationParam.filter.ToLower() == "fyi")
            {
                Expression<Func<Correspondence, bool>> predicate = 
                (x => 
                 x.IsDeleted == false && x.IsUnderReview == true && 
                 //x.Collaboration.Reviewers.Any(r => r.ReviewerUPN.ToLower().Trim() != collaborationParam.userlogin.ToLower().Trim() && x.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim()) &&
                 //x.Collaboration.Reviewers.Any(r => collaborationParam.dlgroups.ToLower().Trim().Contains(r.ReviewerUPN.ToLower().Trim()) == false && x.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim())
                 // &&
                 (
                    x.Collaboration.FYIUsers.Any(f => f.FYIUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) == true && x.CurrentReview.ToLower().Trim() == f.RoundName.ToLower().Trim()) ||
                    x.Collaboration.FYIUsers.Any(f => collaborationParam.dlgroups.ToLower().Trim().Contains(f.FYIUpn.ToLower().Trim()) == true && x.CurrentReview.ToLower().Trim() == f.RoundName.ToLower().Trim())
                 )
                );
                return predicate;
            }
            //review not completed
            else
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && x.LetterStatus.ToLower().Trim() == "Open" && //not deleted, open, in clearance
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && (X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) || collaborationParam.dlgroups.ToLower().Trim().Contains(X.ReviewerUPN.ToLower().Trim())) && X.RoundName.ToLower().Trim() == x.CurrentReview.ToLower().Trim()) && // (assigned to the current user or the current uers's lead offices) and round name equals current item status
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN == collaborationParam.userlogin)
                        .Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().ToLower().Trim() != "draft" &&// current users maximum record status is  "Not completed"
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.ToLower().Trim() == collaborationParam.userlogin.ToLower().Trim() || collaborationParam.dlgroups.ToLower().Trim().Contains(r.ReviewerUPN.ToLower().Trim()) == true)
                        .Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().ToLower().Trim() != "completed"
               );
                return predicate;
            }
            

        }

        public static Expression<Func<Correspondence, bool>> CheckForCriteriaSearchdate(CollaborationParam collaborationParam, DateTime dateTime)
        {

            //copied && archived at the same time
            if (collaborationParam.dataOption.ToLower() == "copied" && collaborationParam.filter.ToLower() == "archived")
            {
                return (x => x.IsDeleted == false &&
                             x.CorrespondenceCopiedOffices.Any(C => collaborationParam.offices.ToLower().Trim().Contains(C.OfficeName.ToLower().Trim()) && C.CorrespondenceId == x.Id) &&
                             x.CorrespondenceCopiedArchiveds.Any(c => c.ArchivedUserUpn.Trim() == collaborationParam.userlogin.Trim()) &&
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                                ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            ));
            }
            //copied only
            else if (collaborationParam.dataOption.ToLower() == "copied")
            {
                return (x => x.IsDeleted == false &&
                             x.CorrespondenceCopiedOffices.Any(C => collaborationParam.offices.ToLower().Trim().Contains(C.OfficeName.ToLower().Trim()) && C.CorrespondenceId == x.Id) &&
                             !x.CorrespondenceCopiedArchiveds.Any(c => c.ArchivedUserUpn.Trim() == collaborationParam.userlogin.Trim()) &&
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            ));
            }
            //pending
            else if (collaborationParam.dataOption.ToLower() == "pending")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsUnderReview == false && x.IsPendingLeadOffice == true && collaborationParam.offices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()) &&
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            ));
            }
            //collaboration and office data
            else if (collaborationParam.dataOption.ToLower() == "collaboration")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsUnderReview == true &&
                     (collaborationParam.officemMnagerOffices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()) || x.Collaboration.Originators.Any(X => X.OriginatorUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()))) &&
                            (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                                ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date 
                            )
                );
            }
            //collaboration and office data
            else if (collaborationParam.dataOption == "office data")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() &&
                     (collaborationParam.offices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()))  &&
                            (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            )
                );
            }
            //review completed
            else if (collaborationParam.filter.ToLower() == "completed")
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && //any , but not deleted
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim())) && // assigned to the current users
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.Trim() == collaborationParam.userlogin.Trim()).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().Trim() == collaborationParam.filter.ToLower().Trim() &&// current users maximum record status is  "Completed"                      
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            )
                );
                return predicate;
            }
            //draft
            else if (collaborationParam.filter.ToLower() == "draft")
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && x.LetterStatus.ToLower().Trim() == "Open" && //not deleted, open, in clearance
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) && X.RoundName.ToLower().Trim() == x.CurrentReview.ToLower().Trim()) && // assigned to the current user and round name equals current item status
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.Trim() == collaborationParam.userlogin.Trim()).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().Trim() == collaborationParam.filter.ToLower().Trim() &&// current users maximum record status is  "draft"                     
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            )
                 );
                return predicate;
            }
            //Review FYI
            else if (collaborationParam.filter.ToLower() == "fyi")
            {
                Expression<Func<Correspondence, bool>> predicate =
                (x =>
                     x.IsDeleted == false && x.IsUnderReview == true &&
                     //x.Collaboration.Reviewers.Any(r => r.ReviewerUPN.ToLower().Trim() != collaborationParam.userlogin.ToLower().Trim() && x.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim()) &&
                     //x.Collaboration.Reviewers.Any(r => collaborationParam.dlgroups.ToLower().Trim().Contains(r.ReviewerUPN.ToLower().Trim()) == false && x.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim())
                     // &&
                     (
                        x.Collaboration.FYIUsers.Any(f => f.FYIUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) == true && x.CurrentReview.ToLower().Trim() == f.RoundName.ToLower().Trim()) ||
                        x.Collaboration.FYIUsers.Any(f => collaborationParam.dlgroups.ToLower().Trim().Contains(f.FYIUpn.ToLower().Trim()) == true && x.CurrentReview.ToLower().Trim() == f.RoundName.ToLower().Trim())
                     ) &&                    
                     (
                        ((DateTime)x.LetterDate).Date == dateTime.Date ||
                        ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                        ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                        x.CreatedTime.Date == dateTime.Date ||
                        x.ModifiedTime.Date == dateTime.Date ||
                        ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                        ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                     )
                 );
                return predicate;
            }
            //review not completed
            else
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && x.LetterStatus.ToLower().Trim() == "Open" && //not deleted, open, in clearance
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && (X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) || collaborationParam.dlgroups.ToLower().Trim().Contains(X.ReviewerUPN.ToLower().Trim())) && X.RoundName.ToLower().Trim() == x.CurrentReview.ToLower().Trim()) && // (assigned to the current user or the current uers's lead offices) and round name equals current item status
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN == collaborationParam.userlogin).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().ToLower().Trim() != "draft" &&// current users maximum record status is  "Not completed"
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN == collaborationParam.userlogin).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().ToLower().Trim() != "completed" &&
                           (
                               ((DateTime)x.LetterDate).Date == dateTime.Date ||
                               ((DateTime)x.LetterReceiptDate).Date == dateTime.Date ||
                               ((DateTime)x.DueforSignatureByDate).Date == dateTime.Date ||
                               x.CreatedTime.Date == dateTime.Date ||
                               x.ModifiedTime.Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundStartDate).Date == dateTime.Date ||
                               ((DateTime)x.Collaboration.CurrentRoundEndDate).Date == dateTime.Date
                            )
               );
                return predicate;
            }

        }

        public static Expression<Func<Correspondence, bool>> CheckForCriteriaSearch(CollaborationParam collaborationParam)
        {
            //copied && archived at the same time
            if (collaborationParam.dataOption.ToLower() == "copied" && collaborationParam.filter.ToLower() == "archived")
            {
                return (x => x.IsDeleted == false &&
                             x.CorrespondenceCopiedOffices.Any(C => collaborationParam.offices.ToLower().Trim().Contains(C.OfficeName.ToLower().Trim()) && C.CorrespondenceId == x.Id) &&
                             x.CorrespondenceCopiedArchiveds.Any(c => c.ArchivedUserUpn.Trim() == collaborationParam.userlogin.Trim()) &&
                           (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            ));
            }
            //copied only
            else if (collaborationParam.dataOption.ToLower() == "copied")
            {
                return (x => x.IsDeleted == false &&
                             x.CorrespondenceCopiedOffices.Any(C => collaborationParam.offices.ToLower().Trim().Contains(C.OfficeName.ToLower().Trim()) && C.CorrespondenceId == x.Id) &&
                             !x.CorrespondenceCopiedArchiveds.Any(c => c.ArchivedUserUpn.Trim() == collaborationParam.userlogin.Trim()) &&
                           (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            ));
            }
            //pending
            else if (collaborationParam.dataOption.ToLower() == "pending")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsUnderReview == false && x.IsPendingLeadOffice == true && collaborationParam.offices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()) &&
                           (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            ));
            }
            //collaboration
            else if (collaborationParam.dataOption.ToLower() == "collaboration")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsUnderReview == true &&
                     (collaborationParam.officemMnagerOffices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim()) || x.Collaboration.Originators.Any(X => X.OriginatorUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()))) &&
                            (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            )
                );
            }
            //office data
            else if (collaborationParam.dataOption == "office data")
            {
                return (x => x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() && x.IsDeleted == false && x.LetterStatus == collaborationParam.filter.Trim() &&
                     (collaborationParam.offices.ToLower().Trim().Contains(x.LeadOfficeName.ToLower().Trim())) &&
                            (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            )
                );
            }
            //review completed
            else if (collaborationParam.filter.ToLower() == "completed")
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && //any , but not deleted
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim())) && // assigned to the current users
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.Trim() == collaborationParam.userlogin.Trim()).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().Trim() == collaborationParam.filter.ToLower().Trim() &&// current users maximum record status is  "Completed"                      
                           (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            )
                );
                return predicate;
            }
            //draft
            else if (collaborationParam.filter.ToLower() == "draft")
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && x.LetterStatus.ToLower().Trim() == "Open" && //not deleted, open, in clearance
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) && X.RoundName.ToLower().Trim() == x.CurrentReview.ToLower().Trim()) && // assigned to the current user and round name equals current item status
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN.Trim() == collaborationParam.userlogin.Trim()).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().Trim() == collaborationParam.filter.ToLower().Trim() &&// current users maximum record status is  "draft"
                    (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            )
                 );
                return predicate;
            }
            //Review FYI
            else if (collaborationParam.filter.ToLower() == "fyi")
            {
                Expression<Func<Correspondence, bool>> predicate =
                (x => 
                     x.IsDeleted == false && x.IsUnderReview == true &&
                     //x.Collaboration.Reviewers.Any(r => r.ReviewerUPN.ToLower().Trim() != collaborationParam.userlogin.ToLower().Trim() && x.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim()) &&
                     //x.Collaboration.Reviewers.Any(r => collaborationParam.dlgroups.ToLower().Trim().Contains(r.ReviewerUPN.ToLower().Trim()) == false && x.CurrentReview.ToLower().Trim() == r.RoundName.ToLower().Trim())
                     // &&
                     (
                        x.Collaboration.FYIUsers.Any(f => f.FYIUpn.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) == true && x.CurrentReview.ToLower().Trim() == f.RoundName.ToLower().Trim()) ||
                        x.Collaboration.FYIUsers.Any(f => collaborationParam.dlgroups.ToLower().Trim().Contains(f.FYIUpn.ToLower().Trim()) == true && x.CurrentReview.ToLower().Trim() == f.RoundName.ToLower().Trim())
                     ) &&
                     (
                        EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                        EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                        x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                        x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                        x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                    )
                 );
                return predicate;
            }
            //review not completed
            else
            {
                Expression<Func<Correspondence, bool>> predicate = (x => x.IsDeleted == false && x.IsUnderReview == true && x.LetterStatus.ToLower().Trim() == "Open" && //not deleted, open, in clearance
                    x.Collaboration.Reviewers.Any(X => X.IsDeleted == false && (X.ReviewerUPN.ToLower().Trim().Contains(collaborationParam.userlogin.ToLower().Trim()) || collaborationParam.dlgroups.ToLower().Trim().Contains(X.ReviewerUPN.ToLower().Trim())) && X.RoundName.ToLower().Trim() == x.CurrentReview.ToLower().Trim()) && // (assigned to the current user or the current uers's lead offices) and round name equals current item status
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN == collaborationParam.userlogin).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().ToLower().Trim() != "draft" &&// current users maximum record status is  "Not completed"
                    x.Collaboration.Reviewers.Where(r => r.Id == x.Collaboration.Reviewers.Where(r => r.ReviewerUPN == collaborationParam.userlogin).Max(r => r.Id)).FirstOrDefault().RoundActivity.ToLower().ToLower().Trim() != "completed" &&
                    (
                               EF.Functions.Like(x.CATSID, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LeadOfficeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CorrespondentName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterTypeName, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.OtherSigners, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ReviewStatus, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CurrentReview, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterSubject, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.FiscalYear, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.LetterCrossReference, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.CreatedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               EF.Functions.Like(x.ModifiedBy, "%" + collaborationParam.search.Trim() + "%") ||
                               x.Collaboration.Originators.Any(d => d.OriginatorName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.Reviewers.Any(d => d.ReviewerName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim())) ||
                               x.Collaboration.FYIUsers.Any(d => d.FYIUserName.ToLower().Trim().Contains(collaborationParam.search.ToLower().Trim()))
                            )
               );
                return predicate;
            }

        }
        private static void CheckIfDate(string search, ref bool hasDate, ref DateTime dateTime)
        {
            try
            {
                dateTime = DateTime.Parse(search);
                hasDate = true;
            }
            catch (Exception ex) { }
        }

        public object GetAllWithPagination(string filter, string search, int offset, int limit)
        {
            throw new NotImplementedException();
        }

        PaginationType<CorrespondenceDto> ICorrespondenceAppService.GetAllWithPagination(string filter, string search, int offset, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CorrespondenceDto> GetAll(string filter, string search, int offset, int limit)
        {
            throw new NotImplementedException();
        }
        public Correspondence AddSurrogates(Correspondence entity)
        {
            if (entity.Collaboration != null)
            {
                entity.Collaboration.Originators.ToList().ForEach(rev =>
                {
                    rev.SurrogateOriginators = surrogateOriginators.FindAll(s => s.CATSUserUPN?.ToLower().Trim() == rev.OriginatorUpn?.ToLower().Trim());
                });
                entity.Collaboration.Reviewers.ToList().ForEach(rev =>
                {
                    if (rev.ReviewerUPN.ToLower().Trim().Contains("dl-"))
                    {
                        var members = _dLGroupMembers.Where(d => d.DLGroup.Name.ToLower().Trim() == rev.ReviewerUPN.ToLower().Trim()).Select(d => d.UserUPN);
                        rev.SurrogateReviewers = surrogateReviewers.FindAll(s => members.Any(m => m.ToLower().Trim() == s.CATSUserUPN?.ToLower().Trim()));
                    }
                    else
                        rev.SurrogateReviewers = surrogateReviewers.FindAll(s => s.CATSUserUPN?.ToLower().Trim() == rev.ReviewerUPN?.ToLower().Trim());
                });
                entity = Repository.Save(entity);
            }

            return entity;
        }

        public IEnumerable<Correspondence> AddSurrogates(List<Correspondence> entities)
        {
            entities.OrderByDescending(x => x.Id).ToList().ForEach(entity => {
                if (entity.Collaboration != null)
                {
                    entity.Collaboration.Originators.ToList().ForEach(rev =>
                    {
                        rev.SurrogateOriginators = surrogateOriginators.FindAll(s => s.CATSUserUPN?.ToLower().Trim() == rev.OriginatorUpn?.ToLower().Trim());
                    });
                    entity.Collaboration.Reviewers.ToList().ForEach(rev =>
                    {
                        if (rev.ReviewerUPN.ToLower().Trim().Contains("dl-"))
                        {
                            var members = _dLGroupMembers.Where(d => d.DLGroup.Name.ToLower().Trim() == rev.ReviewerUPN.ToLower().Trim()).Select(d => d.UserUPN);
                            rev.SurrogateReviewers = surrogateReviewers.FindAll(s => members.Any(m => m.ToLower().Trim().Contains(s.CATSUserUPN?.ToLower().Trim())));
                        }
                        else
                            rev.SurrogateReviewers = surrogateReviewers.FindAll(s => s.CATSUserUPN?.ToLower().Trim() == rev.ReviewerUPN?.ToLower().Trim());
                    });
                    entity = Repository.Save(entity);
                }
            });

            return entities;
        }

        private void setCollaborationMembers(CorrespondenceDto correspondenceDto, CollaborationDto collaborationDto, OriginatorDto[] originatorDto, ReviewerDto[] reviewerDto, FYIUserDto[] fyiuserDto)
        {
            //update originator
            originatorDto.ToList().ForEach(x =>
            {
                x.CollaborationId = collaborationDto.Id;
                //x.Collaboration = null;
                if (x.Id == 0 && x.IsDeleted != true && string.IsNullOrEmpty(x.OriginatorUpn) == false)
                {
                    x = _originatorAppService.Create(x);
                }
                else
                {
                    if (x.IsDeleted == true)
                    {
                        var toDeleteUser = _originatorAppService.GetBy(d => d.OriginatorUpn == x.OriginatorUpn && d.RoundName == x.RoundName && d.IsDeleted == false).OrderByDescending(d => d.Id);
                        if (toDeleteUser != null && string.IsNullOrEmpty(x.OriginatorUpn) == false)
                        {
                            //int? originatorId = toDeleteUser.Id;
                            //_catsAppService.getOriginatorAppService().Delete((int)originatorId);
                            //x.Id = (int)originatorId;
                            toDeleteUser.ToList().ForEach(d => {
                                d.IsDeleted = true;
                                _originatorAppService.Update(d);
                            });
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(x.OriginatorUpn) != true)
                        {
                            x = _originatorAppService.Update(x);
                        }
                    }

                }
            });

            //update reviewer
            reviewerDto.ToList().ForEach(x =>
            {
                x.CollaborationId = collaborationDto.Id;
                //x.Collaboration = null;
                if (x.Id == 0 && x.IsDeleted != true && string.IsNullOrEmpty(x.ReviewerUPN) == false)
                {
                    x = _reviewerAppService.Create(x);
                }
                else
                {
                    if (x.IsDeleted == true)
                    {
                        var toDeleteUser = _reviewerAppService.GetBy(d => d.ReviewerUPN == x.ReviewerUPN && d.RoundName == x.RoundName && d.IsDeleted == false).OrderByDescending(d => d.Id);
                        if (toDeleteUser != null && string.IsNullOrEmpty(x.ReviewerUPN) == false)
                        {
                            //int? reviewerId = toDeleteUser.Id;
                            //_catsAppService.getReviewerAppService().Delete((int)reviewerId);
                            //x.Id = (int)reviewerId;
                            //_catsAppService.getReviewerAppService().Update(x);
                            toDeleteUser.ToList().ForEach(d => {
                                d.IsDeleted = true;
                                _reviewerAppService.Update(d);
                            });
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(x.ReviewerUPN) != true)
                        {
                            x = _reviewerAppService.Update(x);
                        }
                    }

                }
            });

            //update fyiuser
            fyiuserDto.ToList().ForEach(x =>
            {
                x.CollaborationId = collaborationDto.Id;
                //x.Collaboration = null;
                if (x.Id == 0 && x.IsDeleted != true && string.IsNullOrEmpty(x.FYIUpn) == false)
                {
                    x = _fYIUserAppService.Create(x);
                }
                else
                {
                    if (x.IsDeleted == true)
                    {
                        var toDeleteUser = _fYIUserAppService.GetBy(d => d.FYIUpn == x.FYIUpn && d.RoundName == x.RoundName && d.IsDeleted == false).OrderByDescending(d => d.Id);
                        if (toDeleteUser != null && string.IsNullOrEmpty(x.FYIUpn) == false)
                        {
                            //int? fyiId = toDeleteUser.Id;
                            //_catsAppService.getFYIUserAppService().Delete((int)fyiId);
                            //x.Id = (int)fyiId;
                            //_catsAppService.getFYIUserAppService().Update(x);
                            toDeleteUser.ToList().ForEach(d => {
                                d.IsDeleted = true;
                                _fYIUserAppService.Update(d);
                            });
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(x.FYIUpn) == false)
                        {
                            x = _fYIUserAppService.Update(x);
                        }
                    }

                }
            });

            //update collaboration
            //collaborationDto.
        }
    }

    public static class Extensions
    {


        public static IQueryable<Correspondence> EmpidLikeAny(this IQueryable<Correspondence> correspondences, params string[] words)
        {
            var parameter = Expression.Parameter(typeof(Correspondence), "x");
            PropertyInfo propertyInfo = typeof(Correspondence).GetProperty("collaboration", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo nestedProperty1 = propertyInfo.PropertyType.GetProperty("reviewers", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo nestedProperty2 = nestedProperty1.PropertyType.GetProperty("reviewerUpn", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            propertyInfo = nestedProperty2;

            // 1st level properties
            var parentProperties = typeof(Correspondence).GetProperties();
            foreach (var prop in parentProperties)
            {

                // get 2nd level properties
                if (prop.Name.ToLower() == "collaboration")
                {
                    var mainObjectsProperties = prop.PropertyType.GetProperties();
                    foreach (var property in mainObjectsProperties)
                    {
                        // 3rd level props
                        if (property.Name.ToLower() == "reviewers")
                        {
                            var leafProperties = property.PropertyType.GetProperties();
                            foreach (var leafProperty in leafProperties)
                            {
                                if (leafProperty.Name.ToLower() == "reviewerupn")
                                {
                                }
                            }
                        }
                    }
                }
            }




            var body = words.Select(word => Expression.Call(typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like),
                                                                                                    new[]
                                                                                                    {
                                                                                                        typeof(DbFunctions), typeof(string), typeof(string)
                                                                                                    }),
                                                            Expression.Constant(EF.Functions),
                                                            Expression.Property(parameter, typeof(Correspondence).GetProperty("Correspondence.Collaboration.Reviewers.ReviewerUPN")),
                                                            Expression.Constant(word)))
                            .Aggregate<MethodCallExpression, Expression>(null, (current, call) => current != null ? Expression.OrElse(current, call) : (Expression)call);
            var lambda = Expression.Lambda<Func<Correspondence, bool>>(body, parameter);

            return correspondences.Where(Expression.Lambda<Func<Correspondence, bool>>(body, parameter));
        }



        public static Expression<Func<Correspondence, bool>> EmpidLikeAny1(params string[] words)
        {
            var parameter = Expression.Parameter(typeof(Correspondence), "x");
            PropertyInfo propertyInfo = typeof(Correspondence).GetProperty("collaboration", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo nestedProperty1 = propertyInfo.PropertyType.GetProperty("reviewers", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo nestedProperty2 = nestedProperty1.PropertyType.GetProperty("reviewerUpn", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            propertyInfo = nestedProperty2;

            var body = words.Select(word => Expression.Call(typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like),
                                                                                                    new[]
                                                                                                    {
                                                                                                        typeof(DbFunctions), typeof(string), typeof(string)
                                                                                                    }),
                                                            Expression.Constant(EF.Functions),
                                                            Expression.Property(parameter, propertyInfo),
                                                            Expression.Constant(word)))
                            .Aggregate<MethodCallExpression, Expression>(null, (current, call) => current != null ? Expression.OrElse(current, call) : (Expression)call);
            var lambda = Expression.Lambda<Func<Correspondence, bool>>(body, parameter);

            return lambda;
        }

        public static Expression<Func<Correspondence, bool>> setExpressionSurRevProperties2(Reviewer x, bool isDeleted, params string[] words)//IQueryable<Reviewer> EmpidLikeAny(this IQueryable<Reviewer> products, params string[] words)
        {

            var parameterExp = Expression.Parameter(typeof(Reviewer));
            var propertyExpDeleted = Expression.Property(parameterExp, "IsDeleted");

            var body = words.Select(word => Expression.Call(typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like),
                                                                                                    new[]
                                                                                                    {
                                                                                                        typeof(DbFunctions), typeof(string), typeof(string)
                                                                                                    }),
                                                            Expression.Constant(EF.Functions),
                                                            Expression.Property(parameterExp, typeof(Reviewer).GetProperty(nameof(x.ReviewerUPN))),
                                                            Expression.Constant(word)))
                            .Aggregate<MethodCallExpression, Expression>(null, (current, call) => current != null ? Expression.OrElse(current, call) : (Expression)call);
            //return body;
            return Expression.Lambda<Func<Correspondence, bool>>(body, parameterExp);
            //return products.Where(Expression.Lambda<Func<Reviewer, bool>>(body, parameter));
        }

        private static bool setExpressionSurRevProperties(Reviewer x, string[] array = null)
        {
            var itemExpression = Expression.Parameter(typeof(Reviewer));
            Expression left = null;
            var gate = typeof(Reviewer).GetProperty("ReviewerUpn", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);//Reviewer.GetProperty(nameof(condition)).GetString();

            if (array.Length > 1)
            {
                var q = EF.Functions.Like(x.ReviewerUPN, "%" + array[0].Trim() + "%");
                array.ToList().ForEach(d =>
                {
                    q = q || EF.Functions.Like(x.ReviewerUPN, "%" + d.Trim() + "%");
                });
                return q;
            }
            else if (array.Length == 1)
            {
                return EF.Functions.Like(x.ReviewerUPN, "%" + array[0].Trim() + "%");
            }
            else
            {
                return EF.Functions.Like(x.ReviewerUPN, "%%"); ;
            }
        }
    }
}
