// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

public interface IOdsApiClient
{
    Task<ApiResponse> OdsApiGet(string authenticationUrl, string clientId, string clientSecret, string odsEndpointUrl);
}

public class OdsApiClient : IOdsApiClient
{
    private readonly IAppHttpClient _appHttpClient;
    protected readonly ILogger _logger;
    protected readonly AppSettings _options;

    private string _accessToken;

    public OdsApiClient(
        IAppHttpClient appHttpClient,
        ILogger logger,
        IOptions<AppSettings> options
    )
    {
        _appHttpClient = appHttpClient;
        _logger = logger;
        _options = options.Value;
        _accessToken = string.Empty;
    }

    public async Task<ApiResponse> OdsApiGet(
        string authenticationUrl,
        string clientId,
        string clientSecret,
        string odsEndpointUrl
    )
    {
        ApiResponse response = new ApiResponse(HttpStatusCode.InternalServerError, string.Empty);
        await GetAccessToken(authenticationUrl, clientId, clientSecret);

        if (!string.IsNullOrEmpty(_accessToken))
        {
            response = await _appHttpClient.SendAsync(odsEndpointUrl,
                HttpMethod.Get,
                null as StringContent,
                new AuthenticationHeaderValue("bearer", _accessToken)
            );
        }

        return response;
    }

    protected async Task GetAccessToken(string accessTokenUrl, string clientId, string clientSecret)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            FormUrlEncodedContent content;

            content = new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Grant_type", "client_credentials"),
                }
            );

            var encodedKeySecret = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");

            var apiResponse = await _appHttpClient.SendAsync(
                accessTokenUrl,
                HttpMethod.Post,
                content,
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret))
            );

            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonToken = JToken.Parse(apiResponse.Content);
                _accessToken = jsonToken["access_token"].ToString();
            }
            else
            {
                _logger.LogError("Not able to get Ods Api Access Token");
            }
        }
    }
}
