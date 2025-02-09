﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Learning.Blazor.ComponentModels;

public class PasswordsComponentModel
{
    [Required]
    public string? PlainTextPassword { get; set; } = null!;
}
