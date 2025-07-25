// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace EdFi.AdminConsole.HealthCheckService.Helpers;

public static class InstanceValidator
{
    public static bool IsInstanceValid(ILogger logger, Instance instance)
    {
        var messages = new List<string>();

        if (instance == null)
            messages.Add("instance cannot be empty.");
        else
        {
            if (string.IsNullOrEmpty(instance.OAuthUrl))
                messages.Add("AuthenticationUrl is required.");

            if (string.IsNullOrEmpty(instance.ResourceUrl))
                messages.Add("ResourceUrl is required.");
        }

        if (messages != null && messages.Count > 0)
        {
            string concatenatedMessages = string.Concat(messages);
            logger.LogWarning("The instance {Name} obtained from Admin API is not properly formed. {Messages}", instance?.InstanceName, concatenatedMessages);
            return false;
        }

        return true;
    }
}
