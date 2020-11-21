// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using NUnit.Framework;
using static EdFi.Ods.AdminApp.Management.Tests.Testing;

namespace EdFi.Ods.AdminApp.Management.Tests.Infrastructure
{
    [TestFixture]
    public class AutomapperTests
    {
        [Test]
        public void Assert_config_is_valid()
        {
            Scoped<IMapper>(mapper => mapper.ConfigurationProvider.AssertConfigurationIsValid());
        }
    }
}
