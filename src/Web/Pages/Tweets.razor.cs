﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Learning.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Learning.Blazor.Pages
{
    public sealed partial class Tweets : IAsyncDisposable
    {
        private readonly HashSet<TweetContents> _tweets = new();
        private readonly Stack<IDisposable> _subscriptions = new();

        private StreamingStatus? _streamingStatus;
        private string? _filter = null!;

        private string _streamingFontAwesomeClass =>
            HubConnection is { State: HubConnectionState.Connected } &&
            _streamingStatus is { IsStreaming: true }
                ? "fas fa-link has-text-primary"
                : "fas fa-unlink has-text-warning";

        [Inject]
        public SingleHubConnection HubConnection { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            _subscriptions.Push(HubConnection.SubscribeToStatusUpdated(OnStatusUpdated));
            _subscriptions.Push(HubConnection.SubscribeToTweetReceived(OnTweetReceived));

            await HubConnection.StartAsync();
            await HubConnection.JoinTweetsAsync();
            await HubConnection.StartTweetStreamAsync();
        }

        private Task OnStatusUpdated(Notification<StreamingStatus> status) =>
            InvokeAsync(() =>
            {
                _streamingStatus = status;
                StateHasChanged();
            });

        private Task OnTweetReceived(Notification<TweetContents> tweet) =>
            InvokeAsync(() =>
            {
                _ = _tweets.Add(tweet);
                StateHasChanged();
            });

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (HubConnection is not null)
            {
                await HubConnection.LeaveTweetsAsync();
            }

            while (_subscriptions.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }
}