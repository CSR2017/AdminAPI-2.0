// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService.Infrastructure;

public interface IHttpRequestMessageBuilder
{
    HttpRequestMessage GetHttpRequestMessage(string uriString, HttpMethod method, StringContent? content);

    HttpRequestMessage GetHttpRequestMessage(string uriString, HttpMethod method, FormUrlEncodedContent? content);
}

public class HttpRequestMessageBuilder : IHttpRequestMessageBuilder
{
    public HttpRequestMessage GetHttpRequestMessage(string uriString, HttpMethod method, StringContent? content)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(uriString),
            Method = method,
            Content = content
        };

        return request;
    }

    public HttpRequestMessage GetHttpRequestMessage(string uriString, HttpMethod method, FormUrlEncodedContent? content)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(uriString),
            Method = method,
            Content = content
        };

        return request;
    }
}
