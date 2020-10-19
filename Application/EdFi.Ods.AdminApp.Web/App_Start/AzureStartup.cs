// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Castle.Windsor;
using EdFi.Ods.AdminApp.Management.Helpers;
using EdFi.Ods.AdminApp.Web._Installers;
using log4net;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("Azure", typeof(EdFi.Ods.AdminApp.Web.AzureStartup))]
namespace EdFi.Ods.AdminApp.Web
{
    public class AzureStartup : StartupBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AzureStartup));

        private static readonly AppSettings _appSettings = ConfigurationHelper.GetAppSettings();

        protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
        {
            container.Install(new AzureInstaller());
        }

        protected override void ConfigureLogging(IAppBuilder app)
        {
            log4net.Config.XmlConfigurator.Configure();

            var applicationInsightsInstrumentationKey = _appSettings.ApplicationInsightsInstrumentationKey;

            if (applicationInsightsInstrumentationKey != null)
            {
                TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsInstrumentationKey;
                _logger.DebugFormat("Found AppInsights instrumentation key in Web.config -- AppInsights will capture traces");
            }
            else
            {
                _logger.DebugFormat("No instrumentation key found in Web.config -- AppInsights will NOT capture traces");
            }
        }
    }
}
