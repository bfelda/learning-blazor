﻿

using System.Security.Claims;
using System.Timers;
using Learning.Blazor.Extensions;
using Learning.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SystemTimer = System.Timers.Timer;

namespace Learning.Blazor.Pages
{
    public sealed partial class Chat : IAsyncDisposable
    {
        private readonly Dictionary<Guid, ActorMessage> _messages = new();
        private readonly HashSet<Actor> _usersTyping = new();
        private readonly Stack<IDisposable> _subscriptions = new();
        private readonly SystemTimer _debouceTimer = new()
        {
            Interval = 750,
            AutoReset = false
        };

        private Guid? _messageId = null!;
        private string? _message = null!;
        private bool _isTyping = false;
        private ClaimsPrincipal _user = null!;

        private ElementReference _messageInput;

        private const string DefaultRoomName = "public";

        [Parameter]
        public string? Room { get; set; } = DefaultRoomName;

        [Inject]
        public SharedHubConnection HubConnection { get; set; } = null!;

        public Chat() => _debouceTimer.Elapsed += OnDebouceElapsed;

        protected override async Task OnInitializedAsync()
        {
            _subscriptions.Push(
                HubConnection.SubscribeToMessageReceived(OnMessageReceivedAsync));
            _subscriptions.Push(
                HubConnection.SubscribeToUserTyping(OnUserTypingAsync));

            await HubConnection.StartAsync(this);
            await HubConnection.JoinChatAsync(Room ?? DefaultRoomName);

            await _messageInput.FocusAsync();
        }

        async Task OnMessageReceivedAsync(Notification<ActorMessage> message) =>
            await InvokeAsync(
                async () =>
                {
                    _messages[message.Payload.Id] = message;
                //if (message.IsChatBot && message.SayJoke)
                //{
                //    var lang = message.Lang;
                //    var voice = _voices?.FirstOrDefault(v => v.Name == _voice);
                //    if (voice is not null)
                //    {
                //        if (!voice.Lang.StartsWith(lang))
                //        {
                //            var firstLocaleMatchingVoice = _voices.FirstOrDefault(v => v.Lang.StartsWith(lang));
                //            if (firstLocaleMatchingVoice is not null)
                //            {
                //                lang = firstLocaleMatchingVoice.Lang[0..2];
                //            }
                //        }
                //    }

                //    await JavaScript.SpeakAsync(message.Text, _voice, _voiceSpeed, lang);
                //}

                await JavaScript.ScrollIntoViewAsync(".chat-list");

                    StateHasChanged();
                });

        async Task OnUserTypingAsync(Notification<ActorAction> actorAction) =>
            await InvokeAsync(() =>
            {
                var (_, (user, isTyping)) = actorAction;
                _ = isTyping
                    ? _usersTyping.Add(new(user))
                    : _usersTyping.Remove(new(user));

                StateHasChanged();
            });

        private async void OnDebouceElapsed(object? _, ElapsedEventArgs e) =>
            await SetIsTyping(false);

        private async Task OnKeyUp(KeyboardEventArgs args)
        {
            if (args is { Key: "Enter" } and { Code: "Enter" })
            {
                await SendMessage();
            }
        }

        async Task SendMessage()
        {
            if (_message is { Length: > 0 })
            {
                await HubConnection.PostOrUpdateMessageAsync(
                    Room ?? DefaultRoomName, _message, _messageId);

                _message = null;
                _messageId = null;

                StateHasChanged();
            }
        }

        async Task InitiateDebounceUserIsTyping()
        {
            _debouceTimer.Stop();
            _debouceTimer.Start();

            await SetIsTyping(true);
        }

        async Task SetIsTyping(bool isTyping)
        {
            if (_isTyping && isTyping)
            {
                return;
            }

            await HubConnection.ToggleUserTypingAsync(_isTyping = isTyping);
        }

        bool OwnsMessage(string user) => _user?.Identity?.Name == user;

        async Task StartEdit(ActorMessage message)
        {
            if (!OwnsMessage(message.UserName))
            {
                return;
            }

            await InvokeAsync(
                async () =>
                {
                    _messageId = message.Id;
                    _message = message.Text;

                    await _messageInput.FocusAsync();

                    StateHasChanged();
                });
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (HubConnection is not null)
            {
                await HubConnection.LeaveChatAsync(Room ?? DefaultRoomName);
                await HubConnection.StopAsync(this);
            }

            while (_subscriptions.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }
}
