
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TaskFlowConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'TaskFlowConnection' was not found.");

        services.AddDbContext<TaskFlowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TaskFlowDbContext>();
        return services;
    }
}
