// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdFi.Ods.AdminApp.Management.Aws
{
    [System.Obsolete("Review usages.")]
    public class GetAwsProductionApiProvisionWarningQuery : IGetProductionApiProvisioningWarningsQuery
    {
        public Task<ProductionApiProvisioningWarnings> Execute(CloudOdsInstance cloudOdsInstance)
        {
            var result = new ProductionApiProvisioningWarnings
            {
                Warnings = new List<string>()
            };

            return Task.FromResult(result);
        }
    }
}
