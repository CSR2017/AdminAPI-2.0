// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using Microsoft.Extensions.Options;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests;

public class Testing
{
    public static IOptions<AppSettings> GetAppSettings()
    {
        IOptions<AppSettings> options = Options.Create(new AppSettings());
        return options;
    }

    public static IOptions<OdsApiSettings> GetOdsApiSettings()
    {
        OdsApiSettings odsApiSettings = new()
        {
            Endpoints = Endpoints
        };
        IOptions<OdsApiSettings> options = Options.Create(odsApiSettings);
        return options;
    }

    public static List<string> Endpoints { get { return ["firstEndPoint", "secondEndpoint", "thirdEndPoint"]; } }

    public static List<OdsApiEndpointNameCount> HealthCheckData
    {
        get
        {
            return
            [
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "firstEndPoint",
                    OdsApiEndpointCount = 3,
                    AnyErrros = false
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "secondEndpoint",
                    OdsApiEndpointCount = 8,
                    AnyErrros = false
                },
                new OdsApiEndpointNameCount()
                {
                    OdsApiEndpointName = "thirdEndPoint",
                    OdsApiEndpointCount = 5,
                    AnyErrros = false
                }
            ];
        }
    }

    public const string Tenants =
    @"[{
        ""TenantId"": 1,
        ""Document"":
        {
            ""EdfiApiDiscoveryUrl"": ""https://api.ed-fi.org/v7.1/api6/"",
            ""Name"" : ""tenant1""
        }
      },{
        ""TenantId"": 2,
        ""Document"":
        {
            ""EdfiApiDiscoveryUrl"": ""https://api.ed-fi.org/v7.2/api6/"",
            ""Name"" : ""tenant2""
        }
    }]";

    public static List<AdminConsoleInstance> AdminApiInstances
    {
        get
        {
            return
            [
                new()
                {
                    Id = 1,
                    OdsInstanceId = 1,
                    TenantId = 1,
                    TenantName = "tenant1",
                    InstanceName = "instance 1",
                    ClientId = "one client",
                    ClientSecret = "one secret",
                    OauthUrl = "http://www.myserver.com/connect/token",
                    ResourceUrl = "http://www.myserver.com/data/v3/",
                    Status = "Completed",
                },
                new()
                {
                    Id = 2,
                    OdsInstanceId = 2,
                    TenantId = 1,
                    TenantName = "tenant1",
                    InstanceName = "instance 2",
                    ClientId = "another client",
                    ClientSecret = "another secret",
                    OauthUrl = "http://www.myserver.com/connect/token",
                    ResourceUrl = "http://www.myserver.com/data/v3/",
                    Status = "Completed",
                }
            ];
        }
    }

    public const string Instances =
    @"[{
        ""id"": 1,
        ""odsInstanceId"": 1,
        ""tenantId"": 1,
        ""tenantName"": ""tenant1"",
        ""instanceName"": ""instance 1"",
        ""clientId"": ""one client"",
        ""clientSecret"": ""one secret"",
        ""resourceUrl"": ""http://www.myserver.com/data/v3/"",
        ""oauthUrl"": ""http://www.myserver.com/connect/token"",
        ""status"": ""Completed""
      },{
        ""id"": 2,
        ""odsInstanceId"": 2,
        ""tenantId"": 1,
        ""tenantName"": ""tenant1"",
        ""instanceName"": ""instance 2"",
        ""clientId"": ""another client"",
        ""clientSecret"": ""another secret"",
        ""resourceUrl"": ""http://www.myserver.com/data/v3/"",
        ""oauthUrl"": ""http://www.myserver.com/connect/token"",
        ""status"": ""Completed""
    }]";
}
