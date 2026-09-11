using AhmedOumezzine.EFCore.Repository.Extensions;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AhmedOumezzine.EFCore.Repository.Tests;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void AddGenericRepository_ShouldReuseScopedDbContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<DiContext>();
        services.AddGenericRepository<DiContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DiContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var field = repository.GetType().GetField("_dbContext", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.AreSame(context, field!.GetValue(repository));
    }

    private sealed class DiContext(DbContextOptions<DiContext> options) : DbContext(options);
}
