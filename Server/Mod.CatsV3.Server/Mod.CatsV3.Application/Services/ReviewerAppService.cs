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
    public class ReviewerAppService : CrudAppServiceCATS<ReviewerDto, Reviewer>, IReviewerAppService
    {
        ICorrespondenceRepository _correspondenceRepository;
        public ReviewerAppService(IReviewerRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session, ICorrespondenceRepository correspondenceRepository) : base(repository, objectMapper, logger, session)
        {
            _correspondenceRepository = correspondenceRepository; 
        }

        public IEnumerable<ReviewerDto> GetAll(string upn, string option, string round)
        {
            var entities = Repository.GetAll(x => x.IsDeleted == false &&  x.RoundActivity == option && x.RoundName.Contains(round) && EF.Functions.Like(x.ReviewerUPN, "%" + upn + "%"));
            return entities.Select(MapToDto).ToList();
        }
        public IEnumerable<ReviewerDto> GetAll(string upn, string round)
        {
            var entities = Repository.GetAll(x => x.IsDeleted == false && x.RoundName == round && EF.Functions.Like(x.ReviewerUPN, "%" + upn + "%"));
            return entities.Select(MapToDto).ToList();
        }

        public IEnumerable<ReviewerDto> GetAll(string upn, List<string> userDlGroup, string option, string round)
        {
            if (option.ToLower() == "completed")
            {
                var entities = Repository.GetAll(x => 
                    x.IsDeleted == false && 
                    x.RoundActivity == option &&
                    EF.Functions.Like(x.RoundCompletedByUpn.Trim(), "%" + upn + "%") &&
                    EF.Functions.Like(x.RoundName, "%" + round.Trim() + "%") &&
                    (EF.Functions.Like(x.ReviewerUPN, "%" + upn.Trim() + "%") || userDlGroup.Contains(x.ReviewerUPN) == true));
                return entities.Select(MapToDto).ToList();
            }
            else
            {
                var revStatus = option.ToLower().Contains("draft") == true ? "Draft" : "In Progress";
                var entities = Repository.GetAll(x => 
                    x.IsDeleted == false &&
                    EF.Functions.Like(x.RoundActivity, "%" + option.Trim() + "%") &&
                    //EF.Functions.Like(x.RoundName, "%" + round + "%") &&
                    (EF.Functions.Like(x.ReviewerUPN, "%" + upn.Trim() + "%") || userDlGroup.Contains(x.ReviewerUPN) == true) && 
                    x.Collaboration.Correspondence.LetterStatus.Trim() == "Open" &&
                    x.Collaboration.Correspondence.ReviewStatus.Trim() == revStatus.Trim());
                return entities.Select(MapToDto).ToList();
            }
        }

        public IEnumerable<ReviewerDto> GetAll(dynamic item)
        {
            string catsid = item.CATSID;
            string roundname = item.Round;
            string status = item.Status;

            var myitem = _correspondenceRepository.GetAll(x => EF.Functions.Like(x.CATSID, "%" + catsid + "%")).FirstOrDefault();

            var entities = Repository.GetAll(x => x.CollaborationId == myitem.Collaboration.Id && EF.Functions.Like(x.RoundActivity, "%" + status + "%") && EF.Functions.Like(x.RoundName, "%" + roundname + "%"));
            return entities.Select(MapToDto).ToList();
        }
    }
}
