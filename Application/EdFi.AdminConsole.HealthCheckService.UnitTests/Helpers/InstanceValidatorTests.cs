// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.AdminConsole.HealthCheckService.Features.AdminApi;
using EdFi.AdminConsole.HealthCheckService.Helpers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace EdFi.AdminConsole.HealthCheckService.UnitTests.Helpers;

public class Given_an_instance_returned_from_AdminApi
{
    private AdminConsoleInstance _instance = new AdminConsoleInstance();
    private ILogger<Given_an_instance_returned_from_AdminApi> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<Given_an_instance_returned_from_AdminApi>>();

        _instance.OauthUrl = "Some url";
        _instance.ResourceUrl = "Some url";
        _instance.ClientId = "Some url";
        _instance.ClientSecret = "Some url";
        _instance.Id = 1;
        _instance.TenantId = 1;
        _instance.InstanceName = "Some url";
    }

    [TestFixture]
    public class When_it_has_all_required_fields : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_valid()
        {
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_AuthenticationUrl : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_invalid()
        {
            _instance.OauthUrl = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ResourceUrl : Given_an_instance_returned_from_AdminApi
    {

        [Test]
        public void should_be_invalid()
        {
            _instance.ResourceUrl = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }


    [TestFixture]
    public class When_it_does_not_have_ClientId : Given_an_instance_returned_from_AdminApi
    {
        [Test]
        public void should_be_invalid()
        {
            _instance.ClientId = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_it_does_not_have_ClientSecret : Given_an_instance_returned_from_AdminApi
    {

        [Test]
        public void should_be_invalid()
        {
            _instance.ClientSecret = string.Empty;
            InstanceValidator.IsInstanceValid(_logger, _instance).ShouldBeFalse();
        }
    }
}
