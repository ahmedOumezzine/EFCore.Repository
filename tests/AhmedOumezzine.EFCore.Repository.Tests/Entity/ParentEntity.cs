using AhmedOumezzine.EFCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Tests.Entity
{
    public class ParentEntity : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<ChildEntity> Children { get; set; } = new List<ChildEntity>();
    }
}
