// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService.Helpers;

public static class AdminApiConnectioDataValidator
{
    public static bool IsValid(ILogger logger, IAdminApiSettings adminApiSettings)
    {
        var messages = new List<string>();

        if (string.IsNullOrEmpty(adminApiSettings.AccessTokenUrl))
            messages.Add("AccessTokenUrl is required.");

        if (string.IsNullOrEmpty(adminApiSettings.AdminConsoleInstancesURL))
            messages.Add("AdminConsoleInstancesURL is required.");

        if (string.IsNullOrEmpty(adminApiSettings.AdminConsoleHealthCheckURL))
            messages.Add("AdminConsoleHealthCheckURL is required.");

        if (string.IsNullOrEmpty(adminApiSettings.ClientId))
            messages.Add("ClientId is required.");

        if (string.IsNullOrEmpty(adminApiSettings.Scope))
            messages.Add("Scope is required.");

        if (string.IsNullOrEmpty(adminApiSettings.GrantType))
            messages.Add("GrantType is required.");
        else
        {
            if (adminApiSettings.GrantType.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(adminApiSettings.ClientSecret))
                    messages.Add("Client Secret is required.");
            }
            else if (adminApiSettings.GrantType.Equals("password", StringComparison.OrdinalIgnoreCase))
            {

                if (string.IsNullOrEmpty(adminApiSettings.Username))
                    messages.Add("Username is required.");

                if (string.IsNullOrEmpty(adminApiSettings.Password))
                    messages.Add("Password is required.");
            }
        }

        if (messages != null && messages.Count > 0)
        {
            string concatenatedMessages = string.Concat(messages);
            logger.LogWarning("The AdminApiSettings section on the App Settings file has not been set properly. {Messages}", concatenatedMessages);
            return false;
        }

        return true;
    }
}
