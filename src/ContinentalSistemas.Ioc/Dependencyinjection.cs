using ContinentalSistemas.Application.Interfaces.Repositories;
using ContinentalSistemas.Application.Interfaces.Services;
using ContinentalSistemas.Application.Services;
using ContinentalSistemas.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContinentalSistemas.Ioc;

public static class Dependencyinjection
{
    public static IServiceCollection AddConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IExtracaoVendasService, ExtracaoVendasService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IWinthorVendasRepository, WinthorVendasRepository>();

        return services;
    }
}