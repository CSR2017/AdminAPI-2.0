// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService;
using Quartz;

namespace EdFi.Ods.AdminApi.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class HealthCheckJob(IHealthCheckService healthCheckService, ILogger<HealthCheckJob> logger) : IJob
{
    private readonly IHealthCheckService _healthCheckService = healthCheckService;
    private readonly ILogger<HealthCheckJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Running scheduled health check...");
        await _healthCheckService.RunAsync(context.CancellationToken);
    }
}
