using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Tests.Repository
{
    [TestClass]
    public class GetCountRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();

        [TestInitialize]
        public void TestInitialize()
        {
            RecreateDatabase();
        }

    }
}
