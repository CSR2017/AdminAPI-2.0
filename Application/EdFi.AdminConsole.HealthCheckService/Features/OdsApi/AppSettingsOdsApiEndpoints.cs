// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Options;
using System.Collections;

namespace EdFi.AdminConsole.HealthCheckService.Features.OdsApi;

public interface IAppSettingsOdsApiEndpoints : IEnumerable<string>
{

}

public class AppSettingsOdsApiEndpoints : IAppSettingsOdsApiEndpoints
{
    private readonly List<string> endpoints;

    public AppSettingsOdsApiEndpoints(IOptions<OdsApiSettings> odsApiOptions)
    {
        endpoints = new List<string>();
        endpoints.AddRange(odsApiOptions.Value.Endpoints);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return endpoints.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
