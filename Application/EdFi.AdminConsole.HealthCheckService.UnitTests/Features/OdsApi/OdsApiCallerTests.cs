// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using EdFi.AdminConsole.HealthCheckService.Infrastructure;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Features.OdsApi;

public class Given_an_ods_api
{
    [TestFixture]
    public class When_HealthCheckData_is_returned_from_api : Given_an_ods_api
    {
        private IOdsApiClient _odsApiClient;
        private OdsApiCaller _odsApiCaller;

        [SetUp]
        public void SetUp()
        {
            _odsApiClient = A.Fake<IOdsApiClient>();

            var adminApiInstance = Testing.AdminApiInstances.First();

            var httpResponse1 = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponse1.Headers.Add(Constants.TotalCountHeader, "3");

            var httpResponse2 = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponse2.Headers.Add(Constants.TotalCountHeader, "8");

            var httpResponse3 = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponse3.Headers.Add(Constants.TotalCountHeader, "5");

            A.CallTo(() => _odsApiClient.OdsApiGet(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, "http://www.myserver.com/data/v3/ed-fi/firstEndPoint?offset=0&limit=0&totalCount=true"))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, httpResponse1.Headers));

            A.CallTo(() => _odsApiClient.OdsApiGet(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, "http://www.myserver.com/data/v3/ed-fi/secondEndpoint?offset=0&limit=0&totalCount=true"))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, httpResponse2.Headers));

            A.CallTo(() => _odsApiClient.OdsApiGet(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, "http://www.myserver.com/data/v3/ed-fi/thirdEndPoint?offset=0&limit=0&totalCount=true"))
                .Returns(new ApiResponse(HttpStatusCode.OK, string.Empty, httpResponse3.Headers));

            _odsApiCaller = new OdsApiCaller(_odsApiClient, new AppSettingsOdsApiEndpoints(Testing.GetOdsApiSettings()));
        }

        [Test]
        public async Task should_return_stronglytyped_healthCheck_data()
        {
            var healthCheckData = await _odsApiCaller.GetHealthCheckDataAsync(Testing.AdminApiInstances.First());
            healthCheckData.ShouldBeEquivalentTo(Testing.HealthCheckData);
        }
    }
}
