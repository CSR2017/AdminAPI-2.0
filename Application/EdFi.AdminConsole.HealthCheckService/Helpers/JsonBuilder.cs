// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using System.Text.Json.Nodes;

namespace EdFi.AdminConsole.HealthCheckService.Helpers;

public static class JsonBuilder
{
    public static JsonObject BuildJsonObject(IEnumerable<OdsApiEndpointNameCount> healthCheckData)
    {
        JsonObject healthCheckDocument = new();

        if (healthCheckData != null)
        {
            healthCheckDocument.Add(new KeyValuePair<string, JsonNode?>("healthy", !healthCheckData.Any(r => r.AnyErrros)));
            foreach (var countPerEndpoint in healthCheckData)
            {
                healthCheckDocument.Add(new KeyValuePair<string, JsonNode?>(countPerEndpoint.OdsApiEndpointName, countPerEndpoint.OdsApiEndpointCount));
            }
        }

        return healthCheckDocument;
    }
}
