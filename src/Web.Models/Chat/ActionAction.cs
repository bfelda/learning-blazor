﻿// Copyright (c) 2021 David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Learning.Blazor.Models
{
    public record ActorAction(string UserName, bool IsTyping)
        : Actor(UserName);
}
