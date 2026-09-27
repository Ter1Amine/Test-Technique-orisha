using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reception.Infrastructure.Persistence;

namespace Reception.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReceptionDatabase") 
            ?? throw new InvalidOperationException("Connection string 'ReceptionDatabase' was not found.");

        services.AddDbContext<ReceptionDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }
}
