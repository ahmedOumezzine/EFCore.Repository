using AhmedOumezzine.EFCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    // TestEntity.cs
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
