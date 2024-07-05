using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AhmedOumezzine.EFCore.Repository.Extensions
{
    /// <summary>
    /// Contain all the service collection extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add generic repository services to the .NET Dependency Injection container.
        /// </summary>
        /// <typeparam name="TDbContext">Your EF Core <see cref="DbContext"/>.</typeparam>
        /// <param name="services">The type to be extended.</param>
        /// <param name="lifetime">The life time of the service.</param>
        /// <returns>Returns <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <see langword="null"/>.</exception>
        public static IServiceCollection AddGenericRepository<TDbContext>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TDbContext : DbContext
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Add(new ServiceDescriptor(
                typeof(IRepository),
                serviceProvider =>
                {
                    TDbContext dbContext = ActivatorUtilities.CreateInstance<TDbContext>(serviceProvider);
                    return new Repository<TDbContext>(dbContext);
                },
                lifetime));

            return services;
        }
    }
}