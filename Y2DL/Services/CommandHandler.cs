﻿using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Y2DL.Models;

namespace Y2DL.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly Config _config;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandler(DiscordSocketClient client, InteractionService commands, Config config, IServiceProvider serviceProvider)
    {
        _client = client;
        _commands = commands;
        _config = config;
        _serviceProvider = serviceProvider;
    }
    
    public async Task InitializeCommandsAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        await _commands.RegisterCommandsGloballyAsync();

        // Bind InteractionCreated
        _client.InteractionCreated += HandleInteraction;

        // Handle execution results
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _commands.ContextCommandExecuted += ContextCommandExecuted;
        _commands.ComponentCommandExecuted += ComponentCommandExecuted;
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        if (!_config.Services.Commands.Enabled)
        {
            await arg.RespondAsync(embed:
                new EmbedBuilder()
                    .WithTitle("Commands service is disabled.")
                    .WithDescription("Enable it in the config.")
                    .WithColor(Color.Red)
                    .Build(), ephemeral: true);
            return;
        }
        
        try
        {
            SocketInteractionContext ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
    
    public InteractionService GetInteractionService()
    {
        return _commands;
    }
}