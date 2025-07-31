// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.Common.Features;
using EdFi.Ods.AdminApi.Common.Infrastructure;
using EdFi.Ods.AdminApi.Infrastructure.BackgroundJobs;
using Quartz;

namespace EdFi.Ods.AdminApi.Features.HealthCheck;

public class HealthCheckTrigger : IFeature
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var config = endpoints.ServiceProvider.GetRequiredService<IConfiguration>();
        bool enabled = config.GetValue<bool>("AppSettings:EnableAdminConsoleAPI");
        if (enabled)
        {
            AdminApiEndpointBuilder.MapPost(endpoints, "/healthcheck/trigger", TriggerHealthCheck)
            .WithRouteOptions(b => b.WithResponseCode(202))
            .AllowAnonymous()
            .BuildForVersions(AdminApiVersions.AdminConsole);
        }
    }

    internal static async Task<IResult> TriggerHealthCheck(ISchedulerFactory schedulerFactory)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey("HealthCheckJob");

        if (!await scheduler.CheckExists(jobKey))
        {
            var jobDetail = JobBuilder.Create<HealthCheckJob>()
                .WithIdentity(jobKey)
                .Build();
            await scheduler.AddJob(jobDetail, replace: true);
        }

        // Fire-and-forget: schedule a one-time immediate trigger
        var trigger = TriggerBuilder.Create()
            .ForJob(jobKey)
            .WithIdentity($"ImmediateTrigger-{Guid.NewGuid()}")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(trigger);

        // Return accepted immediately without waiting for execution
        return Results.Accepted("/healthcheck/trigger", new { Title = "Health check process accepted and triggered. The latest results will be available shortly, and processing details will be logged for your reference.", Status = 202 });
    }
}
