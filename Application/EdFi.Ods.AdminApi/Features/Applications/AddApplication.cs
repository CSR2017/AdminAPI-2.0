// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using EdFi.Admin.DataAccess.Contexts;
using EdFi.Ods.AdminApi.Infrastructure;
using EdFi.Ods.AdminApi.Infrastructure.Commands;
using EdFi.Ods.AdminApi.Infrastructure.Database.Commands;
using EdFi.Ods.AdminApi.Infrastructure.Documentation;
using FluentValidation;
using FluentValidation.Results;
using Swashbuckle.AspNetCore.Annotations;

namespace EdFi.Ods.AdminApi.Features.Applications;

public class AddApplication : IFeature
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        AdminApiEndpointBuilder.MapPost(endpoints, "/applications", Handle)
            .WithDefaultDescription()
            .WithRouteOptions(b => b.WithResponse<ApplicationResult>(201))
            .BuildForVersions(AdminApiVersions.V1);
    }

    public async Task<IResult> Handle(Validator validator, IAddApplicationCommand addApplicationCommand, IMapper mapper, IUsersContext db, Request request)
    {
        await validator.GuardAsync(request);
        GuardAgainstInvalidEntityReferences(request, db);
        var addedApplicationResult = addApplicationCommand.Execute(request);
        var model = mapper.Map<ApplicationResult>(addedApplicationResult);
        return Results.Created($"/applications/{model.ApplicationId}", model);
    }

    private void GuardAgainstInvalidEntityReferences(Request request, IUsersContext db)
    {
        if (null == db.Vendors.Find(request.VendorId))
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.VendorId), $"Vendor with ID {request.VendorId} not found.") });

        if (request.ProfileId.HasValue && db.Profiles.Find(request.ProfileId) == null)
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.ProfileId), $"Profile with ID {request.ProfileId} not found.") });

        if (null == db.OdsInstances.Find(request.OdsInstanceId))
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.OdsInstanceId), $"ODS instance with ID {request.OdsInstanceId} not found.") });
    }

    [SwaggerSchema(Title = "AddApplicationRequest")]
    public class Request : IAddApplicationModel
    {
        [SwaggerSchema(Description = FeatureConstants.ApplicationNameDescription, Nullable = false)]
        public string? ApplicationName { get; set; }

        [SwaggerSchema(Description = FeatureConstants.VedorIdDescription, Nullable = false)]
        public int VendorId { get; set; }

        [SwaggerSchema(Description = FeatureConstants.ClaimSetNameDescription, Nullable = false)]
        public string? ClaimSetName { get; set; }

        [SwaggerOptional]
        [SwaggerSchema(Description = FeatureConstants.ProfileIdDescription)]
        public int? ProfileId { get; set; }

        [SwaggerSchema(Description = FeatureConstants.EducationOrganizationIdsDescription, Nullable = false)]
        public IEnumerable<int>? EducationOrganizationIds { get; set; }

        [SwaggerSchema(Description = FeatureConstants.OdsInstanceIdDescription, Nullable = false)]
        public int OdsInstanceId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(m => m.ApplicationName)
             .NotEmpty();

            RuleFor(m => m.ApplicationName)
             .Must(BeWithinApplicationNameMaxLength)
             .WithMessage(FeatureConstants.ApplicationNameLengthValidationMessage)
             .When(x => x.ApplicationName != null);

            RuleFor(m => m.ClaimSetName)
                .NotEmpty()
                .WithMessage(FeatureConstants.ClaimSetNameValidationMessage);

            RuleFor(m => m.EducationOrganizationIds)
                .NotEmpty()
                .WithMessage(FeatureConstants.EdOrgIdsValidationMessage);

            RuleFor(m => m.VendorId).Must(id => id > 0).WithMessage(FeatureConstants.VendorIdValidationMessage);
            RuleFor(m => m.OdsInstanceId).Must(id => id > 0).WithMessage(FeatureConstants.OdsInstanceIdValidationMessage);
        }

        private bool BeWithinApplicationNameMaxLength<T>(IAddApplicationModel model, string? applicationName, ValidationContext<T> context)
        {
            var extraCharactersInName = applicationName!.Length - ValidationConstants.MaximumApplicationNameLength;
            if (extraCharactersInName <= 0)
            {
                return true;
            }
            context.MessageFormatter.AppendArgument("ApplicationName", applicationName);
            context.MessageFormatter.AppendArgument("ExtraCharactersInName", extraCharactersInName);
            return false;
        }
    }
}
