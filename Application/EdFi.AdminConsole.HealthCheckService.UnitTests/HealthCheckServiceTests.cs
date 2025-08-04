// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Dynamic;
using System.Text;
using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Features.OdsApi;
using EdFi.Ods.AdminApi.AdminConsole.Features.Tenants;
using EdFi.Ods.AdminApi.AdminConsole.Features.WorkerInstances;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.DataAccess.Models;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.HealthChecks.Commands;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.Instances.Queries;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Services.Tenants;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NUnit.Framework;
using Shouldly;
using EdFi.AdminConsole.HealthCheckService.UnitTests.Helpers;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests;

public class Given_a_health_check_service
{
    public static List<TenantModel> CreateTestTenants()
    {
        var tenant = new TenantModel { TenantId = 1 };
        var expandoObject = new ExpandoObject();
        var dict = (IDictionary<string, object?>)expandoObject;
        dict["name"] = "TestTenant";
        tenant.Document = expandoObject;
        return [tenant];
    }

    [TestFixture]
    public class When_running_health_check_with_valid_tenants_and_instances : Given_a_health_check_service
    {
        private ILogger<HealthCheckService> _logger;
        private IAdminConsoleTenantsService _adminConsoleTenantsService;
        private IGetInstancesQuery _getInstancesQuery;
        private IAddHealthCheckCommand _addHealthCheckCommand;
        private IOdsApiCaller _odsApiCaller;
        private HealthCheckService _healthCheckService;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<HealthCheckService>>();
            _adminConsoleTenantsService = A.Fake<IAdminConsoleTenantsService>();
            _getInstancesQuery = A.Fake<IGetInstancesQuery>();
            _addHealthCheckCommand = A.Fake<IAddHealthCheckCommand>();
            _odsApiCaller = A.Fake<IOdsApiCaller>();
            _cancellationToken = CancellationToken.None;

            // Setup test data
            var tenants = CreateTestTenants();
            var instances = CreateTestInstances();
            var healthCheckData = CreateTestHealthCheckData();

            var savedHealthCheck = new HealthCheck
            {
                DocId = 1,
                InstanceId = 1,
                EdOrgId = 255,
                TenantId = 1,
                Document = JsonSerializer.Serialize(healthCheckData).ToString()
            };

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .Returns(tenants);

            A.CallTo(() => _getInstancesQuery.Execute("TestTenant", "Completed"))
                .Returns(instances);

            A.CallTo(() => _odsApiCaller.GetHealthCheckDataAsync(A<AdminConsoleInstance>.That.Matches(i => i.Id == 1)))
                .Returns(Task.FromResult(healthCheckData));

            A.CallTo(() => _addHealthCheckCommand.Execute(A<HealthCheckCommandModel>.Ignored))
                .Returns(savedHealthCheck);

            _healthCheckService = new HealthCheckService(
                _logger,
                _adminConsoleTenantsService,
                _getInstancesQuery,
                _addHealthCheckCommand,
                _odsApiCaller);
        }

        [Test]
        public async Task Should_call_get_tenants_service()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Should_call_get_instances_query_for_each_tenant()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _getInstancesQuery.Execute("TestTenant", "Completed"))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Should_call_ods_api_caller_for_valid_instances()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _odsApiCaller.GetHealthCheckDataAsync(A<AdminConsoleInstance>.That.Matches(i => i.Id == 1)))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Should_call_add_health_check_command_with_correct_data()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _addHealthCheckCommand.Execute(A<HealthCheckCommandModel>.That.Matches(cmd =>
                cmd.TenantId == 1 &&
                cmd.InstanceId == 1 &&
                !string.IsNullOrEmpty(cmd.Document))))
                .MustHaveHappenedOnceExactly();
        }

        private static List<Instance> CreateTestInstances()
        {
            var credentials = new InstanceWorkerModelDto
            {
                ClientId = "test-client-id",
                Secret = "test-secret"
            };

            return
            [
                new Instance
                {
                    Id = 1,
                    TenantId = 1,
                    InstanceName = "TestInstance",
                    ResourceUrl = "http://test-resource-url.com",
                    OAuthUrl = "http://test-oauth-url.com",
                    Credentials = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(credentials))
                }
            ];
        }

        private static List<OdsApiEndpointNameCount> CreateTestHealthCheckData()
        {
            return
            [
                new OdsApiEndpointNameCount { OdsApiEndpointName="students", OdsApiEndpointCount=100 },
                new OdsApiEndpointNameCount { OdsApiEndpointName="schools", OdsApiEndpointCount=10 },
                new OdsApiEndpointNameCount { OdsApiEndpointName="staff", OdsApiEndpointCount=50 }
            ];
        }
    }

    [TestFixture]
    public class When_running_health_check_with_no_tenants : Given_a_health_check_service
    {
        private IAdminConsoleTenantsService _adminConsoleTenantsService;
        private IGetInstancesQuery _getInstancesQuery;
        private IAddHealthCheckCommand _addHealthCheckCommand;
        private IOdsApiCaller _odsApiCaller;
        private HealthCheckService _healthCheckService;
        private CancellationToken _cancellationToken;
        public TestLoggerProvider _testLoggerProvider;
        public ILogger<HealthCheckService> _logger;

        [TearDown]
        public void TearDown()
        {
            _testLoggerProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _adminConsoleTenantsService = A.Fake<IAdminConsoleTenantsService>();
            _getInstancesQuery = A.Fake<IGetInstancesQuery>();
            _addHealthCheckCommand = A.Fake<IAddHealthCheckCommand>();
            _odsApiCaller = A.Fake<IOdsApiCaller>();
            _cancellationToken = CancellationToken.None;

            _testLoggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(_testLoggerProvider));
            _logger = loggerFactory.CreateLogger<HealthCheckService>();

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .Returns([]);

            _healthCheckService = new HealthCheckService(
                _logger,
                _adminConsoleTenantsService,
                _getInstancesQuery,
                _addHealthCheckCommand,
                _odsApiCaller);
        }

        [Test]
        public async Task Should_log_no_tenants_message()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            var information = _testLoggerProvider.Entries.FirstOrDefault(e =>
            e.LogLevel == LogLevel.Information &&
            e.Message != null && e.Message.Contains("No tenants returned from Admin Api"));

            information.ShouldNotBeNull();
        }

        [Test]
        public async Task Should_not_call_get_instances_query()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _getInstancesQuery.Execute(A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Test]
        public async Task Should_not_call_ods_api_caller()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _odsApiCaller.GetHealthCheckDataAsync(A<AdminConsoleInstance>.Ignored))
                .MustNotHaveHappened();
        }

        [Test]
        public async Task Should_not_call_add_health_check_command()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _addHealthCheckCommand.Execute(A<HealthCheckCommandModel>.Ignored))
                .MustNotHaveHappened();
        }
    }

    [TestFixture]
    public class When_running_health_check_with_no_instances : Given_a_health_check_service
    {
        private IAdminConsoleTenantsService _adminConsoleTenantsService;
        private IGetInstancesQuery _getInstancesQuery;
        private IAddHealthCheckCommand _addHealthCheckCommand;
        private IOdsApiCaller _odsApiCaller;
        private HealthCheckService _healthCheckService;
        private CancellationToken _cancellationToken;
        public TestLoggerProvider _testLoggerProvider;
        public ILogger<HealthCheckService> _logger;

        [TearDown]
        public void TearDown()
        {
            _testLoggerProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<HealthCheckService>>();
            _adminConsoleTenantsService = A.Fake<IAdminConsoleTenantsService>();
            _getInstancesQuery = A.Fake<IGetInstancesQuery>();
            _addHealthCheckCommand = A.Fake<IAddHealthCheckCommand>();
            _odsApiCaller = A.Fake<IOdsApiCaller>();
            _cancellationToken = CancellationToken.None;

            _testLoggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(_testLoggerProvider));
            _logger = loggerFactory.CreateLogger<HealthCheckService>();

            var tenants = CreateTestTenants();

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .Returns(tenants);

            A.CallTo(() => _getInstancesQuery.Execute("TestTenant", "Completed"))
                .Returns([]);

            _healthCheckService = new HealthCheckService(
                _logger,
                _adminConsoleTenantsService,
                _getInstancesQuery,
                _addHealthCheckCommand,
                _odsApiCaller);
        }

        [Test]
        public async Task Should_log_no_instances_message()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            var information = _testLoggerProvider.Entries.FirstOrDefault(e =>
            e.LogLevel == LogLevel.Information &&
            e.Message != null && e.Message.Contains("No instances found on Admin Api"));

            information.ShouldNotBeNull();
        }

        [Test]
        public async Task Should_not_call_ods_api_caller()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _odsApiCaller.GetHealthCheckDataAsync(A<AdminConsoleInstance>.Ignored))
                .MustNotHaveHappened();
        }

        [Test]
        public async Task Should_not_call_add_health_check_command()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _addHealthCheckCommand.Execute(A<HealthCheckCommandModel>.Ignored))
                .MustNotHaveHappened();
        }
    }

    [TestFixture]
    public class When_running_health_check_with_invalid_instance : Given_a_health_check_service
    {
        private ILogger<HealthCheckService> _logger;
        private IAdminConsoleTenantsService _adminConsoleTenantsService;
        private IGetInstancesQuery _getInstancesQuery;
        private IAddHealthCheckCommand _addHealthCheckCommand;
        private IOdsApiCaller _odsApiCaller;
        private HealthCheckService _healthCheckService;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<HealthCheckService>>();
            _adminConsoleTenantsService = A.Fake<IAdminConsoleTenantsService>();
            _getInstancesQuery = A.Fake<IGetInstancesQuery>();
            _addHealthCheckCommand = A.Fake<IAddHealthCheckCommand>();
            _odsApiCaller = A.Fake<IOdsApiCaller>();
            _cancellationToken = CancellationToken.None;

            var tenants = CreateTestTenants();
            var invalidInstances = CreateInvalidTestInstances();

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .Returns(tenants);

            A.CallTo(() => _getInstancesQuery.Execute("TestTenant", "Completed"))
                .Returns(invalidInstances);

            _healthCheckService = new HealthCheckService(
                _logger,
                _adminConsoleTenantsService,
                _getInstancesQuery,
                _addHealthCheckCommand,
                _odsApiCaller);
        }

        [Test]
        public async Task Should_not_call_ods_api_caller_for_invalid_instance()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _odsApiCaller.GetHealthCheckDataAsync(A<AdminConsoleInstance>.Ignored))
                .MustNotHaveHappened();
        }

        [Test]
        public async Task Should_not_call_add_health_check_command_for_invalid_instance()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _addHealthCheckCommand.Execute(A<HealthCheckCommandModel>.Ignored))
                .MustNotHaveHappened();
        }

        private static List<Instance> CreateInvalidTestInstances()
        {
            return
            [
                new Instance
                {
                    Id = 1,
                    TenantId = 1,
                    InstanceName = "InvalidInstance",
                    ResourceUrl = "", // Invalid - empty resource URL
                    OAuthUrl = "", // Invalid - empty OAuth URL
                    Credentials = null // Invalid - no credentials
                }
            ];
        }
    }

    [TestFixture]
    public class When_running_health_check_with_no_health_check_data : Given_a_health_check_service
    {
        private ILogger<HealthCheckService> _logger;
        private IAdminConsoleTenantsService _adminConsoleTenantsService;
        private IGetInstancesQuery _getInstancesQuery;
        private IAddHealthCheckCommand _addHealthCheckCommand;
        private IOdsApiCaller _odsApiCaller;
        private HealthCheckService _healthCheckService;
        private CancellationToken _cancellationToken;
        private TestLoggerProvider _testLoggerProvider;

        [TearDown]
        public void TearDown()
        {
            _testLoggerProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _adminConsoleTenantsService = A.Fake<IAdminConsoleTenantsService>();
            _getInstancesQuery = A.Fake<IGetInstancesQuery>();
            _addHealthCheckCommand = A.Fake<IAddHealthCheckCommand>();
            _odsApiCaller = A.Fake<IOdsApiCaller>();
            _cancellationToken = CancellationToken.None;

            _testLoggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(_testLoggerProvider));
            _logger = loggerFactory.CreateLogger<HealthCheckService>();

            var tenants = CreateTestTenants();
            var instances = CreateTestInstances();

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .Returns(tenants);

            A.CallTo(() => _getInstancesQuery.Execute("TestTenant", "Completed"))
                .Returns(instances);

            A.CallTo(() => _odsApiCaller.GetHealthCheckDataAsync(A<AdminConsoleInstance>.Ignored))
                .Returns([]); // Empty health check data

            _healthCheckService = new HealthCheckService(
                _logger,
                _adminConsoleTenantsService,
                _getInstancesQuery,
                _addHealthCheckCommand,
                _odsApiCaller);
        }

        [Test]
        public async Task Should_log_no_health_check_data_message()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            var information = _testLoggerProvider.Entries.FirstOrDefault(e =>
            e.LogLevel == LogLevel.Information &&
            e.Message != null && e.Message.Contains("No HealthCheck data has been collected"));

            information.ShouldNotBeNull();
        }

        [Test]
        public async Task Should_not_call_add_health_check_command()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            A.CallTo(() => _addHealthCheckCommand.Execute(A<HealthCheckCommandModel>.Ignored))
                .MustNotHaveHappened();
        }

        private static List<Instance> CreateTestInstances()
        {
            var credentials = new InstanceWorkerModelDto
            {
                ClientId = "test-client-id",
                Secret = "test-secret"
            };

            return
            [
                new Instance
                {
                    Id = 1,
                    TenantId = 1,
                    InstanceName = "TestInstance",
                    ResourceUrl = "http://test-resource-url.com",
                    OAuthUrl = "http://test-oauth-url.com",
                    Credentials = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(credentials))
                }
            ];
        }
    }

    [TestFixture]
    public class When_running_health_check_with_exception : Given_a_health_check_service
    {
        private ILogger<HealthCheckService> _logger;
        private IAdminConsoleTenantsService _adminConsoleTenantsService;
        private IGetInstancesQuery _getInstancesQuery;
        private IAddHealthCheckCommand _addHealthCheckCommand;
        private IOdsApiCaller _odsApiCaller;
        private HealthCheckService _healthCheckService;
        private CancellationToken _cancellationToken;
        private TestLoggerProvider _testLoggerProvider;

        [TearDown]
        public void TearDown()
        {
            _testLoggerProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _adminConsoleTenantsService = A.Fake<IAdminConsoleTenantsService>();
            _getInstancesQuery = A.Fake<IGetInstancesQuery>();
            _addHealthCheckCommand = A.Fake<IAddHealthCheckCommand>();
            _odsApiCaller = A.Fake<IOdsApiCaller>();
            _cancellationToken = CancellationToken.None;

            _testLoggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(_testLoggerProvider));
            _logger = loggerFactory.CreateLogger<HealthCheckService>();

            A.CallTo(() => _adminConsoleTenantsService.GetTenantsAsync(true))
                .Throws(new Exception("Test exception"));

            _healthCheckService = new HealthCheckService(
                _logger,
                _adminConsoleTenantsService,
                _getInstancesQuery,
                _addHealthCheckCommand,
                _odsApiCaller);
        }

        [Test]
        public async Task Should_log_error_when_exception_occurs()
        {
            await _healthCheckService.RunAsync(_cancellationToken);

            var error = _testLoggerProvider.Entries.FirstOrDefault(e =>
            e.LogLevel == LogLevel.Error &&
            e.Message != null && e.Message.Contains("An error occurred while running the HealthCheck Service.")
            && e.Exception != null && e.Exception.Message == "Test exception");

            error.ShouldNotBeNull();
        }

        [Test]
        public async Task Should_not_throw_exception()
        {
            Should.NotThrow(async () => await _healthCheckService.RunAsync(_cancellationToken));
        }
    }
}

public class Given_a_health_check_command_model
{
    [TestFixture]
    public class When_creating_health_check_command_model : Given_a_health_check_command_model
    {
        [Test]
        public void Should_set_properties_correctly()
        {
            const int TenantId = 1;
            const int InstanceId = 2;
            const string Document = "test document";

            var model = new HealthCheckCommandModel(TenantId, InstanceId, Document);

            model.TenantId.ShouldBe(TenantId);
            model.InstanceId.ShouldBe(InstanceId);
            model.Document.ShouldBe(Document);
            model.DocId.ShouldBe(0); // Default value
            model.EdOrgId.ShouldBe(0); // Default value
        }

        [Test]
        public void Should_implement_IAddHealthCheckModel()
        {
            var model = new HealthCheckCommandModel(1, 2, "document");

            model.ShouldBeAssignableTo<IAddHealthCheckModel>();
        }

        [Test]
        public void Should_allow_property_modification()
        {
            var model = new HealthCheckCommandModel(1, 2, "document")
            {
                TenantId = 10,
                InstanceId = 20,
                Document = "new document",
                DocId = 30,
                EdOrgId = 40
            };

            model.TenantId.ShouldBe(10);
            model.InstanceId.ShouldBe(20);
            model.Document.ShouldBe("new document");
            model.DocId.ShouldBe(30);
            model.EdOrgId.ShouldBe(40);
        }
    }
}
