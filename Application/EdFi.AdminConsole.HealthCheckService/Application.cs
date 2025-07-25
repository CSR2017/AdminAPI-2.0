// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService;

public interface IApplication
{
    Task Run();
}

public class Application(
    ILogger logger,
    IAdminApiCaller adminApiCaller,
    IOdsApiCaller odsApiCaller
    ) : IApplication, IHostedService
{
    private readonly ILogger _logger = logger;
    private readonly IAdminApiCaller _adminApiCaller = adminApiCaller;
    private readonly IOdsApiCaller _odsApiCaller = odsApiCaller;

    public async Task Run()
    {
        /// Step 1. Get tenants data from Admin API - Admin Console extension.
        _logger.LogInformation("Get tenants on Admin Api.");
        var tenants = await _adminApiCaller.GetTenantsAsync();

        if (!tenants.Any())
            _logger.LogInformation("No tenants returned from Admin Api.");
        else
        {
            foreach (var tenantName in tenants.Select(tenant => tenant.Document.Name))
            {
                /// Step 2. Get instances data from Admin API - Admin Console extension.
                var instances = await _adminApiCaller.GetInstancesAsync(tenantName);

                if (instances == null || !instances.Any())
                {
                    _logger.LogInformation("No instances found on Admin Api.");
                }
                else
                {
                    foreach (var instance in instances)
                    {
                        /// Step 3. For each instance, Get the HealthCheck data from ODS API
                        _logger.LogInformation("Processing instance with name: {InstanceName}", instance.InstanceName ?? "<No Name>");

                        if (InstanceValidator.IsInstanceValid(_logger, instance))
                        {
                            var healthCheckData = await _odsApiCaller.GetHealthCheckDataAsync(instance);

                            if (healthCheckData != null && healthCheckData.Count > 0)
                            {
                                _logger.LogInformation("HealCheck data obtained.");

                                var healthCheckDocument = JsonBuilder.BuildJsonObject(healthCheckData);

                                /// Step 4. Post the HealthCheck data to the Admin API
                                var healthCheckPayload = new AdminApiHealthCheckPost()
                                {
                                    TenantId = instance.TenantId,
                                    InstanceId = instance.Id,
                                    Document = healthCheckDocument.ToString(),
                                };

                                _logger.LogInformation("Posting HealthCheck data to Admin Api.");

                                await _adminApiCaller.PostHealthCheckAsync(healthCheckPayload, tenantName);
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "No HealthCheck data has been collected for instance with name: {InstanceName}", instance.InstanceName
                                );
                            }
                        }
                    }
                }
            }

            _logger.LogInformation("Process completed.");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
