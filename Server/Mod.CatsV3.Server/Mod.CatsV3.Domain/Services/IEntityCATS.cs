using Mod.Framework.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Services
{
    public interface IEntityCATS : IEntity<int>, IEntityCATS<int>
    {
    }

    public interface IEntityCATS<TPrimaryKey> :  IEntity<TPrimaryKey>
    {
        //DateTime CreatedTime { get; set; }
        //string ModifiedBy { get; set; }
        //DateTime ModifiedTime { get; set; }
        //string DeletedBy { get; set; }
        //DateTime? DeletedTime { get; set; }
        //bool IsDeleted { get; set; }
        string CreatedBy { get; set; }
    }
}
