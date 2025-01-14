﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

using Learning.Blazor.TwitterServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tweetinvi;
using Tweetinvi.Streaming;

namespace Learning.Blazor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTwitterServices(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddSingleton<ITwitterService, DefaultTwitterService>()
            .AddSingleton<IFilteredStream>(_ =>
            {
                TwitterClient client = new(
                    configuration["Authentication:Twitter:ConsumerKey"],
                    configuration["Authentication:Twitter:ConsumerSecret"],
                    configuration["Authentication:Twitter:AccessToken"],
                    configuration["Authentication:Twitter:AccessTokenSecret"]);

                var stream = client.Streams.CreateFilteredStream();
                stream.StallWarnings = true;

                return stream;
            });
}
