using Microsoft.Extensions.Options;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Mod.CatsV3.Domain.Entities
{
    public class SurrogateReviewer : FullAuditedEntity
    {
        public string CATSUserSPID { get; set; }
        public string CATSUser { get; set; }
        public string CATSUserUPN { get; set; }
        public string SurrogateSPUserID { get; set; }
        public string Surrogate { get; set; }
        public string SurrogateUPN { get; set; }
    }
}
