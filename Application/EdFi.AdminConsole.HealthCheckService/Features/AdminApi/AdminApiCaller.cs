// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Text;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiCaller
{
    Task<IEnumerable<AdminConsoleTenant>> GetTenantsAsync();
    Task<IEnumerable<AdminConsoleInstance>> GetInstancesAsync(string? tenant);
    Task PostHealthCheckAsync(AdminApiHealthCheckPost instanceHealthCheckData, string? tenant);
}

public class AdminApiCaller(ILogger logger, IAdminApiClient adminApiClient, IOptions<AdminApiSettings> adminApiOptions) : IAdminApiCaller
{
    private readonly ILogger _logger = logger;
    private readonly IAdminApiClient _adminApiClient = adminApiClient;
    private readonly AdminApiSettings _adminApiOptions = adminApiOptions.Value;

    public async Task<IEnumerable<AdminConsoleTenant>> GetTenantsAsync()
    {
        if (AdminApiConnectioDataValidator.IsValid(_logger, _adminApiOptions))
        {
            var response = await _adminApiClient.AdminApiGet(_adminApiOptions.AdminConsoleTenantsURL, null);
            var tenants = new List<AdminConsoleTenant>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                var tenantsJObject = JsonConvert.DeserializeObject<IEnumerable<JObject>>(response.Content);
                if (tenantsJObject != null)
                {
                    foreach (var jObjectItem in tenantsJObject)
                    {
                        try
                        {
                            var jsonString = jObjectItem.ToString();
                            if (jsonString.StartsWith("{{") && jsonString.EndsWith("}}"))
                            {
                                jsonString = jsonString[1..^1];
                            }
                            var tenant = JsonConvert.DeserializeObject<AdminConsoleTenant>(jsonString);
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Not able to process tenant.");
                        }
                    }
                }
            }
            return tenants;
        }
        else
        {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return [];
        }
    }

    public async Task<IEnumerable<AdminConsoleInstance>> GetInstancesAsync(string? tenant)
    {
        if (AdminApiConnectioDataValidator.IsValid(_logger, _adminApiOptions))
        {
            var response = await _adminApiClient.AdminApiGet(_adminApiOptions.AdminConsoleInstancesURL + Constants.CompletedInstances, tenant);
            var instances = new List<AdminConsoleInstance>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                var instancesJObject = JsonConvert.DeserializeObject<IEnumerable<JObject>>(response.Content);
                if (instancesJObject != null)
                {
                    foreach (var jObjectItem in instancesJObject)
                    {
                        try
                        {
                            var instance = JsonConvert.DeserializeObject<AdminConsoleInstance>(jObjectItem.ToString());
                            if (instance != null)
                                instances.Add(instance);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Not able to process instance.");
                        }
                    }
                }
            }
            return instances;
        }
        else
        {
            _logger.LogError("AdminApi Settings has not been set properly.");
            return [];
        }
    }

    public async Task PostHealthCheckAsync(AdminApiHealthCheckPost instanceHealthCheckData, string? tenant)
    {
        var response = await _adminApiClient.AdminApiPost(_adminApiOptions.AdminConsoleHealthCheckURL, tenant, instanceHealthCheckData);

        if (response.StatusCode is not System.Net.HttpStatusCode.Created and not System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Not able to post HealthCheck data to Ods Api. Tenant Id: {TenantId}.", instanceHealthCheckData.TenantId);
            _logger.LogError("Status Code returned is: {StatusCode}.", response.StatusCode);
        }
    }
}
