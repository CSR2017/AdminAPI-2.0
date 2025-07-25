// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public interface IAdminApiClient
{
    Task<ApiResponse> AdminApiGet(string url, string? tenant);

    Task<ApiResponse> AdminApiPost(string url, string? tenant, object? body = null);
}

public class AdminApiClient(
    IAppHttpClient appHttpClient,
    ILogger logger,
    IOptions<AdminApiSettings> adminApiOptions
    ) : IAdminApiClient
{
    private readonly IAppHttpClient _appHttpClient = appHttpClient;
    protected readonly ILogger _logger = logger;
    private readonly AdminApiSettings _adminApiOptions = adminApiOptions.Value;
    private string _accessToken = string.Empty;

    public async Task<ApiResponse> AdminApiGet(string url, string? tenant)
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, "Unknown error.");
        await GetAccessToken();

        if (!string.IsNullOrEmpty(_accessToken))
        {
            StringContent? content = null;
            if (!string.IsNullOrEmpty(tenant))
            {
                content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                content.Headers.Add("tenant", tenant);
            }

            response = await _appHttpClient.SendAsync(url,
                HttpMethod.Get,
                content,
                new AuthenticationHeaderValue("bearer", _accessToken)
            );
        }

        return response;
    }

    public async Task<ApiResponse> AdminApiPost(string url, string? tenant, object? body = null)
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, "Unknown error.");
        await GetAccessToken();

        if (!string.IsNullOrEmpty(_accessToken))
        {
            StringContent? content = new StringContent(body != null ? JsonConvert.SerializeObject(body) : string.Empty, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(tenant))
            {
                content.Headers.Add("tenant", tenant);
            }

            response = await _appHttpClient.SendAsync(
                url,
                HttpMethod.Post,
                content,
                new AuthenticationHeaderValue("bearer", _accessToken)
            );
        }

        return response;
    }

    protected async Task GetAccessToken()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>
                {
                    new("username", _adminApiOptions.Username),
                    new("client_id", _adminApiOptions.ClientId),
                    new("client_secret", _adminApiOptions.ClientSecret),
                    new("password", _adminApiOptions.Password),
                    new("grant_type", _adminApiOptions.GrantType),
                    new("scope", _adminApiOptions.Scope)
                }
            );

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var apiResponse = await _appHttpClient.SendAsync(
                _adminApiOptions.AccessTokenUrl,
                HttpMethod.Post,
                content,
                null
            );

            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonToken = JToken.Parse(apiResponse.Content);
                _accessToken = jsonToken["access_token"].ToString();
            }
            else
            {
                _logger.LogError("Not able to get Admin Api Access Token. Status Code: {0}", apiResponse.StatusCode.ToString());
            }
        }
    }
}
