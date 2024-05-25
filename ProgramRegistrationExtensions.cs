using Microsoft.OpenApi.Models;
using TimeTable.Core;

namespace TimeTable;

public static class ProgramRegistrationExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection service, string title)
    {
        service.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
        });
        return service;
    }

    public static IServiceCollection Cors(this IServiceCollection services, string cors)
    {
        if (!cors.IsNullOrEmpty())
            services.AddCors(
                options => options.AddPolicy(
                    "Default",
                    policyBuilder => policyBuilder
                        .WithOrigins(
                            cors.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );
        return services;
    }
}