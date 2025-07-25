// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService.Features.AdminApi;

public class AdminConsoleInstance
{
    public int TenantId { get; set; } = 0;

    public string TenantName { get; set; } = string.Empty;

    public int Id { get; set; } = 0;

    public int OdsInstanceId { get; set; } = 0;

    public string InstanceName { get; set; } = string.Empty;

    public string ResourceUrl { get; set; } = string.Empty;

    public string OauthUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public enum InstanceStatus
{
    Pending,
    Completed,
    InProgress,
    Pending_Delete,
    Deleted,
    Delete_Failed,
    Error
}
