// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using EdFi.Ods.AdminApi.Infrastructure;
using EdFi.Ods.AdminApi.Infrastructure.Database.Queries;

namespace EdFi.Ods.AdminApi.Features.Applications;

public class ReadApplicationsByOdsInstance : IFeature
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var url = "odsinstances/{id}/applications";

        AdminApiEndpointBuilder.MapGet(endpoints, url, GetOdsInstanceApplications)
            .WithDescription("Retrieves applications assigned to a specific ODS instance based on the resource identifier.")
            .WithRouteOptions(b => b.WithResponse<ApplicationModel[]>(200))
            .BuildForVersions(AdminApiVersions.V1);
    }

    internal Task<IResult> GetOdsInstanceApplications(IGetApplicationsByOdsInstanceIdQuery getApplicationByOdsInstanceIdQuery, IMapper mapper, int id)
    {
        var odsInstanceApplications = mapper.Map<List<ApplicationModel>>(getApplicationByOdsInstanceIdQuery.Execute(id));
        return Task.FromResult(Results.Ok(odsInstanceApplications));
    }
}
