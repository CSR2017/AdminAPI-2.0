// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Helpers;

public class Given_a_set_of_healthCheck_data
{
    private List<OdsApiEndpointNameCount> _endpoointCounts = new List<OdsApiEndpointNameCount>();

    [SetUp]
    public void SetUp()
    {
        _endpoointCounts = new List<OdsApiEndpointNameCount>()
            {
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "Some endpoint",
                    OdsApiEndpointCount = 2,
                    AnyErrros = false
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "Other endpoint",
                    OdsApiEndpointCount = 3,
                    AnyErrros = false
                }
            };
    }

    [TestFixture]
    public class When_a_json_is_built : Given_a_set_of_healthCheck_data
    {
        [Test]
        public void should_be_valid()
        {
            var expectedHealthCheckJsonObjectPayload = "{\"healthy\": true,\"Some endpoint\": 2,\"Other endpoint\": 3}";

            var healthCheckJsonObjectPayload = JsonBuilder.BuildJsonObject(_endpoointCounts);

            JObject.Parse(healthCheckJsonObjectPayload.ToString()).ShouldBeEquivalentTo(JObject.Parse(expectedHealthCheckJsonObjectPayload.ToString()));
        }
    }

    [TestFixture]
    public class When_a_json_is_built_with_errors : Given_a_set_of_healthCheck_data
    {
        [Test]
        public void should_be_invalid()
        {
            var expectedHealthCheckJsonObjectPayload = "{\"healthy\": false,\"Some endpoint\": 2,\"Other endpoint\": 3,\"One more endpoint\": 0}";

            var endpoointCountsWithErrors = _endpoointCounts;
            endpoointCountsWithErrors.Add(new OdsApiEndpointNameCount
            {
                OdsApiEndpointName = "One more endpoint",
                OdsApiEndpointCount = 0,
                AnyErrros = true
            });

            var healthCheckJsonObjectPayload = JsonBuilder.BuildJsonObject(endpoointCountsWithErrors);

            JObject.Parse(healthCheckJsonObjectPayload.ToString()).ShouldBeEquivalentTo(JObject.Parse(expectedHealthCheckJsonObjectPayload.ToString()));
        }
    }
}
