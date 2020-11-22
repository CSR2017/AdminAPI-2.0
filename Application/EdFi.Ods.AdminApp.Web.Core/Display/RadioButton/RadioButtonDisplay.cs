﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.Ods.AdminApp.Web.Display.RadioButton
{
    public class RadioButtonDisplay<T> where T: IRadioButton
    {
        public bool IsEnabled { get; set; }

        public T RadioButton { get; set; }
    }
}