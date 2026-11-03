using IoanCvApp.Application.Abstractions;
using IoanCvApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IoanCvApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IContentService, ContentService>();
        return services;
    }
}
