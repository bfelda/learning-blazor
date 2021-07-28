﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Learning.Blazor.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using static System.Text.Encoding;

namespace Learning.Blazor.Api.Extensions
{
    internal static class DistributedCacheExtensions
    {
        /// <summary>
        /// The default <see cref="DistributedCacheEntryOptions.AbsoluteExpirationRelativeToNow"/> is 5 mins.
        /// </summary>
        internal static async Task<TItem> GetOrCreateAsync<TItem>(
            this IDistributedCache distributedCache,
            string key,
            Func<DistributedCacheEntryOptions, Task<TItem>> factory,
            CancellationToken token = default)
            where TItem : class
        {
            TItem result;

            var bytes = await distributedCache.GetAsync(key, token);
            if (bytes is { Length: > 0 })
            {
                var json = UTF8.GetString(bytes);
                result = json.FromJson<TItem>()!;
            }
            else
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }.SetSlidingExpiration(TimeSpan.FromMinutes(1));

                result = await factory(options).ConfigureAwait(false);

                var json = result.ToJson()!;
                bytes = UTF8.GetBytes(json);

                await distributedCache.SetAsync(key, bytes, options, token);
            }

            return result;
        }
    }
}
