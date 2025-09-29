using AhmedOumezzine.EFCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Tests.Entity
{
    public class ChildEntity : BaseEntity
    {
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public ParentEntity Parent { get; set; }
    }
}
