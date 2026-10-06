using CRISP.Application.Interfaces;
using CRISP.Infrastructure.Repositories;
using CRISP.Infrastructure.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IFrameworkRepository, FrameworkRepository>();
        services.AddScoped<IRuleEngineExecutor, RulesEngineExecutor>();
        return services;
    }
}
