﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

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
        public SharedHubConnection HubConnection { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            _subscriptions.Push(
                HubConnection.SubscribeToStatusUpdated(OnStatusUpdatedAsync));
            _subscriptions.Push(
                HubConnection.SubscribeToTweetReceived(OnTweetReceivedAsync));

            await HubConnection.StartAsync(this);
            await HubConnection.JoinTweetsAsync();
            await HubConnection.StartTweetStreamAsync();
        }

        private Task OnStatusUpdatedAsync(Notification<StreamingStatus> status) =>
            InvokeAsync(() =>
            {
                _streamingStatus = status;
                StateHasChanged();
            });

        private Task OnTweetReceivedAsync(Notification<TweetContents> tweet) =>
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
                await HubConnection.StopAsync(this);
            }

            while (_subscriptions.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }
}
