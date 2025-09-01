using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class AddRepositoryTests : BaseRepositoryTests
    {
        private Repository<TestDbContext> _repository;
        private Mock<DbSet<TestEntity>> _mockSet;

        [TestInitialize]
        public void Setup()
        {
            _mockSet = CreateMockDbSet(new List<TestEntity>());
            SetupDbContext(_mockSet);
            _repository = new Repository<TestDbContext>(_mockContext.Object);
        }

        [TestMethod]
        public async Task AddAndSaveAsync_Entity_AddsEntityAndReturnsKey()
        {
            // Arrange
            var entity = new TestEntity { Name = "Test" };

            // Act
            var keys = await _repository.AddAndSaveAsync(entity);

            // Assert
            _mockSet.Verify(m => m.Add(It.IsAny<TestEntity>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(1, keys.Length);
        }
    }
}
