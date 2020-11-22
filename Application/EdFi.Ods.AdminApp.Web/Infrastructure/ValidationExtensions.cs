// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
#if NET48
using EdFi.Ods.Common.Utils.Extensions;
#else
using EdFi.Common.Utils.Extensions;
#endif
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using log4net;

namespace EdFi.Ods.AdminApp.Web.Infrastructure
{
    public static class ValidationExtensions
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ValidationExtensions));

        public static IRuleBuilderInitial<T, TProperty> SafeCustom<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, Action<TProperty, CustomContext> action)
        {
            return ruleBuilder.Custom((command, context) =>
            {
                try
                {
                    action(command, context);
                }
                catch (Exception exception)
                {
                    const string errorMsg = "A validation rule encountered an unexpected error. Check the Application Log for troubleshooting information.";
                    context.AddFailure(errorMsg);
                    _logger.Error(errorMsg, exception);
                }
            });
        }

        public static void AddFailures(this CustomContext context, ValidationResult result)
        {
            result.Errors.Select(x => x.ErrorMessage).ForEach(context.AddFailure);
        }
    }
}
