using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Backend.Domain.Shared;                       
using Backend.Domain.Catalog.Repositories;         
using Backend.Domain.Sales.Repositories;          
using Backend.Domain.Users.Repositories;          
using Backend.Domain.Reviews.Repositories;         

using Backend.Infrastructure.Repositories;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Persistence;

using Backend.Application;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.MaxBatchSize(100);
                npgsqlOptions.EnableRetryOnFailure(3);
            });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITypeRepository, TypeRepository>();
        services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();

        services.AddApplication();

        return services;
    }
}