// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Features.OdsApi;

public class Given_an_ods_environment_with_single_tenant
{
    private ILogger<OdsApiClient> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<OdsApiClient>>();
    }

    public class When_HealthCheck_data_is_requested : Given_an_ods_environment_with_single_tenant
    {
        [Test]
        public async Task should_return_successfully()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var adminApiInstance = Testing.AdminApiInstances.First();
            var encodedKeySecret = Encoding.ASCII.GetBytes($"{adminApiInstance.ClientId}:{adminApiInstance.ClientSecret}");
            var headers = new HttpResponseMessage().Headers;
            headers.Add(Constants.TotalCountHeader, "5");

            A.CallTo(() => httpClient.SendAsync(
                adminApiInstance.OauthUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret))))
                .Returns(new ApiResponse(HttpStatusCode.OK, "{ \"access_token\": \"123\"}"));

            A.CallTo(() => httpClient.SendAsync(adminApiInstance.ResourceUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, headers));

            var odsApiClient = new OdsApiClient(httpClient, _logger, Testing.GetAppSettings());

            var response = await odsApiClient.OdsApiGet(
                adminApiInstance.OauthUrl, adminApiInstance.ClientId, adminApiInstance.ClientSecret, adminApiInstance.ResourceUrl);

            response.Headers.ShouldNotBeNull();
            response.Headers.Any(o => o.Key == Constants.TotalCountHeader).ShouldBe(true);
            response.Headers.GetValues(Constants.TotalCountHeader).First().ShouldBe("5");
        }
    }

    public class When_HealthCheck_data_is_requested_without_token : Given_an_ods_environment_with_single_tenant
    {
        [Test]
        public async Task InternalServerError_is_returned()
        {
            var httpClient = A.Fake<IAppHttpClient>();
            var adminApiInstance = Testing.AdminApiInstances.First();
            var encodedKeySecret = Encoding.ASCII.GetBytes($"{adminApiInstance.ClientId}:{adminApiInstance.ClientSecret}");

            var headers = new HttpResponseMessage().Headers;
            headers.Add(Constants.TotalCountHeader, "5");

            A.CallTo(() => httpClient.SendAsync(
                adminApiInstance.OauthUrl, HttpMethod.Post, A<FormUrlEncodedContent>.Ignored, new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret))))
                .Returns(new ApiResponse(HttpStatusCode.InternalServerError, string.Empty));

            A.CallTo(() => httpClient.SendAsync(adminApiInstance.ResourceUrl, HttpMethod.Get, null as StringContent, new AuthenticationHeaderValue("bearer", "123")))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, headers));

            var odsApiClient = new OdsApiClient(httpClient, _logger, Testing.GetAppSettings());

            var response = await odsApiClient.OdsApiGet(adminApiInstance.OauthUrl, adminApiInstance.ClientId, adminApiInstance.ClientSecret, adminApiInstance.ResourceUrl);

            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }
    }
}
