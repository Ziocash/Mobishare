using System;
using Microsoft.AspNetCore.Identity;
using OllamaSharp;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools;

public static class HttpClientContext
{
    private static readonly AsyncLocal<IHttpClientFactory> _HttpClientFactory = new();
    private static readonly AsyncLocal<Chat> _chat = new();
    private static readonly AsyncLocal<OllamaApiClient> _client = new();
    private static readonly AsyncLocal<UserManager<IdentityUser>> _userManager = new();
    public static IHttpClientFactory HttpClientFactory
    {
        get => _HttpClientFactory.Value ?? throw new InvalidOperationException("HttpClientFactory is not set in the current context.");
        set => _HttpClientFactory.Value = value;
    }

    public static Chat Chat
    {
        get => _chat.Value ?? throw new InvalidOperationException("Chat is not set in the current context.");
        set => _chat.Value = value;
    }
    public static OllamaApiClient Client
    {
        get => _client.Value ?? throw new InvalidOperationException("OllamaApiClient is not set in the current context.");
        set => _client.Value = value;
    }

    public static UserManager<IdentityUser> UserManagerController
    {
        get => _userManager.Value ?? throw new InvalidOperationException("UserManager is not set in the current context.");
        set => _userManager.Value = value;
    }
}