// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService;

public static class Startup
{
    public static IServiceCollection ConfigureTransformLoadServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions();
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<AdminApiSettings>(configuration.GetSection("AdminApiSettings"));
        services.Configure<OdsApiSettings>(configuration.GetSection("OdsApiSettings"));

#pragma warning disable CS8603 // Possible null reference return.
        services.AddSingleton<ILogger>(sp => sp.GetService<ILogger<Application>>());
#pragma warning restore CS8603 // Possible null reference return.

        services.AddSingleton<IAppSettingsOdsApiEndpoints, AppSettingsOdsApiEndpoints>();

        services.AddTransient<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();

        services.AddTransient<IAdminApiClient, AdminApiClient>();
        services.AddTransient<IOdsApiClient, OdsApiClient>();

        services.AddTransient<IAdminApiCaller, AdminApiCaller>();
        services.AddTransient<IOdsApiCaller, OdsApiCaller>();

        services.AddTransient<IHostedService, Application>();

        services
            .AddHttpClient<IAppHttpClient, AppHttpClient>(
                "AppHttpClient",
                x =>
                {
                    x.Timeout = TimeSpan.FromSeconds(500);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                if (
                    configuration?.GetSection("AppSettings")?["IgnoresCertificateErrors"]?.ToLower() == "true"
                )
                {
                    return IgnoresCertificateErrorsHandler();
                }
                return handler;
            });

        services.AddTransient<AdminApiClient>();

        return services;
    }

    private static HttpClientHandler IgnoresCertificateErrorsHandler()
    {
        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
#pragma warning disable S4830 // Server certificates should be verified during SSL/TLS connections
            ServerCertificateCustomValidationCallback = (
                httpRequestMessage,
                cert,
                cetChain,
                policyErrors
            ) =>
            {
                return true;
            }
        };
#pragma warning restore S4830

        return handler;
    }
}
