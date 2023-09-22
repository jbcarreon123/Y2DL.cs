using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using Y2DL.Models;
using Exception = System.Exception;

namespace Y2DL.Services.DiscordCommands;

public class PluginCommands : InteractionModuleBase<ShardedInteractionContext>
{
    [AutocompleteCommand("plugin", "plugininfo")]
    public async Task AutoCompletePlugin()
    {
        await AutoCompletePluginInfo(Context);
    }
    
    [SlashCommand("plugininfo", "Get plugin info")]
    public async Task PluginInfo([Summary("plugin", "The plugin's name"), Autocomplete] string pluginName)
    {
        try
        {
            var pluginManifest = PluginManager.GetPluginManifestByPluginId(pluginName);

            await RespondAsync(embed: new EmbedBuilder()
                .WithTitle(pluginManifest.Name)
                .WithUrl(pluginManifest.Repository)
                .WithDescription(pluginManifest.Description)
                .AddField("Author", pluginManifest.Author, true)
                .AddField("Version", pluginManifest.Version, true)
                .Build());
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while showing plugin info");
        }
    }

    public static async Task AutoCompletePluginInfo(ShardedInteractionContext Context)
    {
        string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();

        List<AutocompleteResult> resultList = new();
        try
        {
            foreach (var plugin in PluginManager.GetAllPluginManifests())
            {
                resultList.Add(new AutocompleteResult(plugin.Name, plugin.Id));
            }

            IEnumerable<AutocompleteResult> results = resultList.AsEnumerable().Where(x =>
                x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase));

            await ((SocketAutocompleteInteraction)Context.Interaction).RespondAsync(results.Take(25));
        }
        catch (Exception e)
        {
            Log.Warning(e, "Error occured");
        }
    }
}