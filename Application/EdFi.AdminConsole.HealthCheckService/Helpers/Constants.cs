// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.AdminConsole.HealthCheckService.Helpers
{
    public struct Constants
    {
        public const string OdsApiQueryParams = "?offset=0&limit=0&totalCount=true";

        public const string TotalCountHeader = "total-count";

        public const string TenantHeader = "tenant";

        public const string CompletedInstances = "?status=Completed";

        public const string EdFiUri = "ed-fi";

        public const int RetryStartingDelayMilliseconds = 500;
    }
}
