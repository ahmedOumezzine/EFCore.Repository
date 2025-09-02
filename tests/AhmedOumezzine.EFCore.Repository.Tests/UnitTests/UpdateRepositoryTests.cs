using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class UpdateRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEntitySuccessfully()
        {
            using var context = GetDbContext(nameof(UpdateAsync_UpdatesEntitySuccessfully));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "OldName" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            product.Name = "NewName";
            var affected = await repo.UpdateAsync(product);

            Assert.Equal(1, affected);

            var updated = await context.Products.FirstAsync();
            Assert.Equal("NewName", updated.Name);
        }

        [Fact]
        public async Task UpdateOnlyAsync_UpdatesSpecifiedProperties()
        {
            using var context = GetDbContext(nameof(UpdateOnlyAsync_UpdatesSpecifiedProperties));
            var repo = new Repository<TestDbContext>(context);

            // Crée et ajoute le produit initial
            var product = new Product { Name = "OldName", Description = "OldDesc" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Modifie uniquement la propriété à mettre à jour
            product.Name = "NewName";
            // Ne touche pas à Description ici

            // Appelle UpdateOnlyAsync pour ne mettre à jour que Name
            var affected = await repo.UpdateOnlyAsync(product, new[] { "Name" });
            Assert.Equal(1, affected);

            // Recharge l'entité depuis la base (AsNoTracking pour simuler lecture SQL réelle)
            var updated = await context.Products.AsNoTracking().FirstAsync();
            Assert.Equal("NewName", updated.Name);      // Name mis à jour
            Assert.Equal("OldDesc", updated.Description); // Description reste inchangée
        }

        [Fact]
        public async Task UpdateIfExistsAsync_ReturnsTrueIfExists()
        {
            using var context = GetDbContext(nameof(UpdateIfExistsAsync_ReturnsTrueIfExists));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "Test" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            product.Name = "Updated";
            var result = await repo.UpdateIfExistsAsync(product);

            Assert.True(result);
            var updated = await context.Products.FirstAsync();
            Assert.Equal("Updated", updated.Name);
        }

        [Fact]
        public async Task UpdateIfExistsAsync_ReturnsFalseIfNotExists()
        {
            using var context = GetDbContext(nameof(UpdateIfExistsAsync_ReturnsFalseIfNotExists));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Id = Guid.NewGuid(), Name = "NonExistent" }; // Id not in DB
            var result = await repo.UpdateIfExistsAsync(product);

            Assert.False(result);
        }

        [Fact]
        public async Task TryUpdateAsync_ReturnsTrueOnSuccess()
        {
            using var context = GetDbContext(nameof(TryUpdateAsync_ReturnsTrueOnSuccess));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "Old" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            product.Name = "New";
            var success = await repo.TryUpdateAsync(product);

            Assert.True(success);
            var updated = await context.Products.FirstAsync();
            Assert.Equal("New", updated.Name);
        }

        [Fact]
        public async Task TryUpdateAsync_ReturnsFalseOnFailure()
        {
            using var context = GetDbContext(nameof(TryUpdateAsync_ReturnsFalseOnFailure));
            var repo = new Repository<TestDbContext>(context);

            Product product = null; // invalid
            var success = await repo.TryUpdateAsync(product);

            Assert.False(success);
        }
    }
}