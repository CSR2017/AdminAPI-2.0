// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

public interface IOdsApiCaller
{
    Task<List<OdsApiEndpointNameCount>> GetHealthCheckDataAsync(AdminConsoleInstance instance);
}

public class OdsApiCaller(IOdsApiClient odsApiClient, IAppSettingsOdsApiEndpoints appSettingsOdsApiEndpoints) : IOdsApiCaller
{
    private readonly IOdsApiClient _odsApiClient = odsApiClient;
    private readonly IAppSettingsOdsApiEndpoints _appSettingsOdsApiEndpoints = appSettingsOdsApiEndpoints;

    public async Task<List<OdsApiEndpointNameCount>> GetHealthCheckDataAsync(AdminConsoleInstance instance)
    {
        var tasks = new List<OdsApiEndpointNameCount>();

        foreach (var appSettingsOdsApiEndpoint in _appSettingsOdsApiEndpoints)
        {
            var odsResourceEndpointUrl = $"{instance.ResourceUrl}{Constants.EdFiUri}/{appSettingsOdsApiEndpoint}{Constants.OdsApiQueryParams}";

            tasks.Add(await GetCountPerEndpointAsync(
                appSettingsOdsApiEndpoint, instance.OauthUrl, instance.ClientId, instance.ClientSecret, odsResourceEndpointUrl));
        }

        return tasks;
    }

    protected async Task<OdsApiEndpointNameCount> GetCountPerEndpointAsync(string odsApiEndpoint, string authUrl, string clientId, string clientSecret, string odsEndpointUrl)
    {
        var result = new OdsApiEndpointNameCount()
        {
            OdsApiEndpointName = odsApiEndpoint,
        };
        var response = await _odsApiClient.OdsApiGet(authUrl, clientId, clientSecret, odsEndpointUrl);

        if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK && response.Headers != null && response.Headers.Contains(Constants.TotalCountHeader))
            result.OdsApiEndpointCount = int.Parse(response.Headers.GetValues(Constants.TotalCountHeader).First());
        else
            result.AnyErrros = true;

        return result;
    }
}
