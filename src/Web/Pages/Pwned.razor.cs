﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Learning.Blazor.Pages
{
    public partial class Pwned
    {
        private void NavigateToBreaches() => Navigation.NavigateTo("pwned/breaches");
        private void NavigateToPasswords() => Navigation.NavigateTo("pwned/passwords");
    }
}
