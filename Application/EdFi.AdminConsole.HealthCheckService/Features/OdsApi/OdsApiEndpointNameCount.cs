// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

public class OdsApiEndpointNameCount
{
    public string OdsApiEndpointName { get; set; } = string.Empty;

    public int OdsApiEndpointCount { get; set; } = 0;

    public bool AnyErrros { get; set; } = false;
}
