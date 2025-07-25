// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Text.Json.Serialization;

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public class AdminConsoleTenant
{
    public int TenantId { get; set; } = 0;
    [JsonPropertyName("document")]
    public AdminConsoleTenantDocument Document { get; set; } = new AdminConsoleTenantDocument();
}


public class AdminConsoleTenantDocument
{
    public string EdfiApiDiscoveryUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
