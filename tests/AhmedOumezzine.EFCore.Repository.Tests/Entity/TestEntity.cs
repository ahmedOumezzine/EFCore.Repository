using AhmedOumezzine.EFCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Tests.Entity
{
    // Entité de test
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
