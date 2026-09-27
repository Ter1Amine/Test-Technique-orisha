using Microsoft.Extensions.DependencyInjection;
using Reception.Application.Interfaces;
using Reception.Application.Services;

namespace Reception.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandService, CommandService>();
        services.AddScoped<IPaletteReceiptService, PaletteReceiptService>();
        services.AddScoped<ICartonReceiptService, CartonReceiptService>();
        services.AddScoped<IProductReceiptService, ProductReceiptService>();
        return services;
    }
}
