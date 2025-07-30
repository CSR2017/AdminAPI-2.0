// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using Microsoft.Extensions.Logging;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.Instances.Queries;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.Tenants;
using System.Dynamic;
using EdFi.Ods.AdminApi.AdminConsole.Features.Tenants;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.DataAccess.Models;
using EdFi.Ods.AdminApi.AdminConsole.Features.WorkerInstances;
using Newtonsoft.Json;
using System.Text;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.HealthChecks.Commands;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

namespace EdFi.AdminConsole.HealthCheckService;

public interface IHealthCheckService
{
    Task Run();
}

public class HealthCheckService(ILogger<HealthCheckService> logger,
    IAdminConsoleTenantsService adminConsoleTenantsService,
    IGetInstancesQuery getInstancesQuery,
    IAddHealthCheckCommand addHealthCheckCommand,
    IOdsApiCaller odsApiCaller)
    : IHealthCheckService
{
    private readonly ILogger _logger = logger;
    private readonly IAdminConsoleTenantsService _adminConsoleTenantsService = adminConsoleTenantsService;
    private readonly IGetInstancesQuery _getInstancesQuery = getInstancesQuery;
    private readonly IOdsApiCaller _odsApiCaller = odsApiCaller;
    private readonly IAddHealthCheckCommand _addHealthCheckCommand = addHealthCheckCommand;

    public async Task Run()
    {
        try
        {
            /// Step 1. Get tenants data from Admin API - Admin Console extension.
            _logger.LogInformation("Starting HealthCheck Service...");
            _logger.LogInformation("Get tenants on Admin Api.");
            var tenants = await _adminConsoleTenantsService.GetTenantsAsync(true);

            if (tenants.Count == 0)
                _logger.LogInformation("No tenants returned from Admin Api.");
            else
            {
                foreach (var tenantName in tenants.Select(GetTenantName))
                {
                    _logger.LogInformation("TenantName:{TenantName}", tenantName);

                    /// Step 2. Get instances data from Admin API - Admin Console extension.
                    var instances = await _getInstancesQuery.Execute(tenantName, "Completed");

                    if (instances == null || !instances.Any())
                    {
                        _logger.LogInformation("No instances found on Admin Api.");
                    }
                    else
                    {
                        foreach (var instance in instances)
                        {
                            /// Step 3. For each instance, Get the HealthCheck data from ODS API
                            _logger.LogInformation(
                                "Processing instance with name: {InstanceName}",
                                instance.InstanceName ?? "<No Name>"
                            );
                            var adminConsoleInstance = ConvertToAdminConsoleInstance(instance);
                            if (InstanceValidator.IsInstanceValid(_logger, adminConsoleInstance))
                            {
                                var healthCheckData = await _odsApiCaller.GetHealthCheckDataAsync(adminConsoleInstance);

                                if (healthCheckData != null && healthCheckData.Count > 0)
                                {
                                    _logger.LogInformation("HealCheck data obtained.");

                                    var healthCheckDocument = JsonBuilder.BuildJsonObject(healthCheckData);

                                    /// Step 4. Post the HealthCheck data to the Admin API
                                    HealthCheckCommandModel healthCheckCommandModel = new(
                                        instance.TenantId,
                                        instance.Id,
                                        healthCheckDocument.ToString()
                                    );
                                    _logger.LogInformation("Posting HealthCheck data to Admin Api.");

                                    await _addHealthCheckCommand.Execute(healthCheckCommandModel);
                                }
                                else
                                {
                                    _logger.LogInformation(
                                        "No HealthCheck data has been collected for instance with name: {InstanceName}",
                                        instance.InstanceName
                                    );
                                }
                            }
                        }
                    }
                }

                _logger.LogInformation("Process completed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while running the HealthCheck Service.");
        }

        static string GetTenantName(TenantModel tenant)
        {
            if (tenant.Document is ExpandoObject expandoObject &&
                expandoObject is IDictionary<string, object> dict &&
                dict.TryGetValue("name", out var nameValue) &&
                nameValue is string name)
            {
                return name;
            }
            return string.Empty;
        }

        static AdminConsoleInstance ConvertToAdminConsoleInstance(Instance instance)
        {
            string? ClientId = null;
            string? ClientSecret = null;

            if (instance.Credentials != null)
            {
                var credentials = JsonConvert.DeserializeObject<InstanceWorkerModelDto>(Encoding.UTF8.GetString(instance.Credentials));
                ClientId = credentials?.ClientId;
                ClientSecret = credentials?.Secret;
            }

            return new AdminConsoleInstance
            {
                Id = instance.Id,
                ResourceUrl = instance.ResourceUrl ?? string.Empty,
                OauthUrl = instance.OAuthUrl ?? string.Empty,
                ClientId = ClientId ?? string.Empty,
                ClientSecret = ClientSecret ?? string.Empty
            };
        }
    }
}

public class HealthCheckCommandModel(int tenantId, int instanceId, string document) : IAddHealthCheckModel
{
    public int TenantId { get; set; } = tenantId;
    public int InstanceId { get; set; } = instanceId;
    public string Document { get; set; } = document;
    public int DocId { get; set; }
    public int EdOrgId { get; set; }
}
