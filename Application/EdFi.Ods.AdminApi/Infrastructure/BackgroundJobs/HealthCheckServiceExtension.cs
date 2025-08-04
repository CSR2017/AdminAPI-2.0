// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.HealthChecks.Commands;

namespace EdFi.Ods.AdminApi.Infrastructure.BackgroundJobs;

public static class HealthCheckServiceExtension
{
    public static void ConfigureHealthCheckServices(
        this WebApplicationBuilder builder,
        IConfiguration configuration
    )
    {
        builder.Services.AddOptions();
        builder.Services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        builder.Services.Configure<OdsApiSettings>(configuration.GetSection("HealthCheck:OdsApiSettings"));

        builder.Services.AddSingleton<IAppSettingsOdsApiEndpoints, AppSettingsOdsApiEndpoints>();
        builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

        builder.Services.AddTransient<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>();
        builder.Services.AddTransient<IAddHealthCheckCommand, AddHealthCheckCommand>();

        builder.Services.AddTransient<IOdsApiClient, OdsApiClient>();
        builder.Services.AddTransient<IOdsApiCaller, OdsApiCaller>();

        builder.Services
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
