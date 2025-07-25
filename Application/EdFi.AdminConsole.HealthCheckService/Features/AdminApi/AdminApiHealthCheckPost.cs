// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public class AdminApiHealthCheckPost
{
    public int DocId { get; set; } = 0;
    public int InstanceId { get; set; } = 0;
    public int EdOrgId { get; set; } = 0;
    public int TenantId { get; set; } = 0;
    public string Document { get; set; } = string.Empty;

}
