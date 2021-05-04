using Microsoft.EntityFrameworkCore;
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class CollaborationAppService : CrudAppServiceCATS<CollaborationDto, Collaboration>, ICollaborationAppService
    {
        ICollaborationRepository _repository;
        public CollaborationAppService(ICollaborationRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
            _repository = repository;
        }

        IEnumerable<CollaborationDto> ICollaborationAppService.GetAll(string filter, string[] offices)
        {

            if (filter == "Open & Closed")
            {
                var entities = _repository.GetAll(x => x.Correspondence.IsDeleted == false);

                return entities.Select(MapToDto).ToList();
            }
            else
            {
                var entities = _repository.GetAll(x => x.Correspondence.IsDeleted == false && x.Correspondence.LetterStatus == filter && offices.Contains(x.Correspondence.LeadOfficeName));

                return entities.Select(MapToDto).ToList();
            }

        }

        public IEnumerable<CollaborationDto> GetAll(string filter, string search, string[] offices, Boolean isPendingCopied)
        {
            search = string.IsNullOrEmpty(search) ? "" : search;
            Boolean hasDate = false;
            DateTime dateTime = new DateTime();
            CheckIfDate(search, ref hasDate, ref dateTime);

            var predicate = PredicateBuilder.False<Collaboration>();

            if (filter == "Open & Closed")
            {
                if (hasDate)
                {
                    var entities = Repository.GetAll(x => x.Correspondence.IsDeleted != true &&
                       (
                           ((DateTime)x.CurrentRoundEndDate).Date == dateTime.Date ||
                           ((DateTime)x.CurrentRoundStartDate).Date == dateTime.Date ||
                           x.CreatedTime.Date == dateTime.Date ||
                           x.ModifiedTime.Date == dateTime.Date ||
                           x.Correspondence.CreatedTime.Date == dateTime.Date ||
                           x.Correspondence.ModifiedTime.Date == dateTime.Date ||
                           x.Correspondence.DueforSignatureByDate == dateTime.Date ||
                           x.Correspondence.LetterDate == dateTime.Date ||
                           x.Correspondence.LetterReceiptDate == dateTime.Date ||
                           x.Correspondence.PADDueDate == dateTime.Date
                        )
                    );

                    return entities.Select(MapToDto).ToList();
                }
                else
                {
                    var entities = Repository.GetAll(x => (x.Correspondence.IsDeleted != true) &&
                       (
                            EF.Functions.Like(x.Correspondence.CATSID, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LeadOfficeName, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.CorrespondentName, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LetterTypeName, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.OtherSigners, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.ReviewStatus, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.CurrentReview, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LetterSubject, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.FiscalYear, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LetterCrossReference, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.CreatedBy, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.ModifiedBy, "%" + search.Trim() + "%") 
                       )
                   );

                    //get copied data
                    if (isPendingCopied)
                    {
                        foreach (var office in offices)
                        {
                            predicate = predicate.Or(x => EF.Functions.Like(x.Correspondence.CopiedOfficeName, "%" + office + "%"));
                        }
                        var entitiesCopied = Repository.GetAll(predicate);

                        entitiesCopied.ForEach(entity =>
                        {
                            if (entities.Any(x => x.CATSID != entity.CATSID))
                            {
                                entities.Add(entity);
                            }
                        });
                    }

                    return entities.Select(MapToDto).ToList();
                }
            }
            else
            {
                if (hasDate)
                {
                    var entities = Repository.GetAll(x => x.Correspondence.IsDeleted != true &&
                       (
                           ((DateTime)x.CurrentRoundEndDate).Date == dateTime.Date ||
                           ((DateTime)x.CurrentRoundStartDate).Date == dateTime.Date ||
                           x.CreatedTime.Date == dateTime.Date ||
                           x.ModifiedTime.Date == dateTime.Date ||
                           x.Correspondence.CreatedTime.Date == dateTime.Date ||
                           x.Correspondence.ModifiedTime.Date == dateTime.Date ||
                           x.Correspondence.DueforSignatureByDate == dateTime.Date ||
                           x.Correspondence.LetterDate == dateTime.Date ||
                           x.Correspondence.LetterReceiptDate == dateTime.Date ||
                           x.Correspondence.PADDueDate == dateTime.Date 
                        )
                    );

                    return entities.Select(MapToDto).ToList();
                }
                else
                {
                    var entities = Repository.GetAll(x => (EF.Functions.Like(x.Correspondence.LetterStatus, "%" + filter.Trim() + "%") && x.Correspondence.IsDeleted == false)
                    &&
                       (
                            EF.Functions.Like(x.Correspondence.CATSID, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LeadOfficeName, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.CorrespondentName, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LetterTypeName, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.OtherSigners, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.ReviewStatus, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.CurrentReview, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LetterSubject, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.FiscalYear, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.LetterCrossReference, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.CreatedBy, "%" + search.Trim() + "%") ||
                            EF.Functions.Like(x.Correspondence.ModifiedBy, "%" + search.Trim() + "%")
                        )
                    );

                    //get copied data
                    if (isPendingCopied)
                    {
                        foreach (var office in offices)
                        {
                            predicate = predicate.Or(x => EF.Functions.Like(x.Correspondence.CopiedOfficeName, "%" + office + "%"));
                        }
                        var entitiesCopied = Repository.GetAll(predicate);

                        entitiesCopied.ForEach(entity =>
                        {
                            if (entities.Any(x => x.CATSID != entity.CATSID))
                            {
                                entities.Add(entity);
                            }
                        });
                    }

                    return entities.Select(MapToDto).ToList();
                }
            }
        }
        public IEnumerable<CollaborationDto> GetCorrespondence(int correspondenceId)
        {
            var entities = _repository.GetAll(x => x.Correspondence.Id == correspondenceId);

            return entities.Select(MapToDto).ToList();
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

        public IEnumerable<CollaborationDto> GetAll(string filter, string search, string[] offices)
        {
            throw new NotImplementedException();
        }

        
    }
}
