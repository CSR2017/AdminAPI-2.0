// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService;

public interface IAdminApiSettings
{
    string AdminConsoleTenantsURL { get; set; }
    string AdminConsoleInstancesURL { get; set; }
    string AdminConsoleHealthCheckURL { get; set; }
    string AccessTokenUrl { get; set; }
    string Username { get; set; }
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    string Password { get; set; }
    string GrantType { get; set; }
    string Scope { get; set; }
}

public sealed class AdminApiSettings : IAdminApiSettings
{
    public string AdminConsoleTenantsURL { get; set; } = string.Empty;
    public string AdminConsoleInstancesURL { get; set; } = string.Empty;
    public string AdminConsoleHealthCheckURL { get; set; } = string.Empty;
    public string AccessTokenUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string GrantType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
