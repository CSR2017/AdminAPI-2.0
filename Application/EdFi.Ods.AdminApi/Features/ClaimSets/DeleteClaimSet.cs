// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.Infrastructure;
using EdFi.Ods.AdminApi.Infrastructure.ClaimSetEditor;
using EdFi.Ods.AdminApi.Infrastructure.ErrorHandling;
using EdFi.Ods.AdminApi.Infrastructure.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace EdFi.Ods.AdminApi.Features.ClaimSets;

public class DeleteClaimSet : IFeature
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        AdminApiEndpointBuilder.MapDelete(endpoints, "/claimsets/{id}", Handle)
            .WithDefaultDescription()
            .WithRouteOptions(b => b.WithResponseCode(200, FeatureConstants.DeletedSuccessResponseDescription))
            .BuildForVersions(AdminApiVersions.V2);
    }

    public Task<IResult> Handle(IDeleteClaimSetCommand deleteClaimSetCommand, [FromServices] IGetClaimSetByIdQuery getClaimSetByIdQuery, IGetApplicationsByClaimSetIdQuery getApplications, int id)
    {
        CheckClaimSetExists(id, getClaimSetByIdQuery);
        CheckAgainstDeletingClaimSetsWithApplications(id, getApplications);

        try
        {
            deleteClaimSetCommand.Execute(new DeleteClaimSetModel { Id = id });
        }
        catch (AdminApiException exception)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(id), exception.Message) });
        }

        return Task.FromResult(Results.Ok("ClaimSet".ToJsonObjectResponseDeleted()));
    }

    private static void CheckClaimSetExists(int id, IGetClaimSetByIdQuery query)
    {
        try
        {
            query.Execute(id);
        }
        catch (AdminApiException)
        {
            throw new NotFoundException<int>("claimset", id);
        }
    }

    private static void CheckAgainstDeletingClaimSetsWithApplications(int id, IGetApplicationsByClaimSetIdQuery getApplications)
    {
        var associatedApplicationsCount = getApplications.Execute(id).Count();
        if (associatedApplicationsCount > 0)
            throw new ValidationException(new[] { new ValidationFailure(nameof(id),
                $"Cannot delete this claim set. This claim set has {associatedApplicationsCount} associated application(s).") });
    }
}
