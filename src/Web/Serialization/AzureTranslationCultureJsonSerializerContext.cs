﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Learning.Blazor.Extensions;
using Learning.Blazor.Models;

namespace Learning.Blazor.Serialization
{
    [JsonSerializable(typeof(AzureTranslationCultures))]
    internal partial class AzureTranslationCultureJsonSerializerContext
         : JsonSerializerContext
    {
        internal static JsonTypeInfo<AzureTranslationCultures> DefaultTypeInfo =>
            new AzureTranslationCultureJsonSerializerContext(
                DefaultJsonSerialization.Options).AzureTranslationCultures;
    }
}