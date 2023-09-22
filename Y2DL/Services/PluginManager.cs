using System.Reflection;
using Serilog;
using Y2DL.Models;
using Y2DL.Plugins.Interfaces;
using Y2DL.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Y2DL.Services;

public class PluginManager
{
    public static PluginManifest GetPluginManifestByPluginName(string name)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        
        string folderPath = "Plugins";
        string[] pluginsFolderPath = Directory.GetDirectories(folderPath);

        return deserializer.Deserialize<PluginManifest>(pluginsFolderPath.First(x =>
            deserializer.Deserialize<PluginManifest>(x + "/PluginManifest.yaml").Name.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) >
            0) + "/PluginManifest.yaml");
    }
    
    public static PluginManifest GetPluginManifestByPluginId(string id)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        
        string folderPath = "Plugins";
        string[] pluginsFolderPath = Directory.GetDirectories(folderPath);

        return deserializer.Deserialize<PluginManifest>(File.ReadAllText(pluginsFolderPath.First(x =>
            deserializer.Deserialize<PluginManifest>(File.ReadAllText(x + "/PluginManifest.yaml")).Id == id) + "/PluginManifest.yaml"));
    }

    public static List<PluginManifest> GetAllPluginManifests()
    {
        List<PluginManifest> manifests = new List<PluginManifest>();
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        
        string folderPath = "Plugins";
        string[] pluginsFolderPath = Directory.GetDirectories(folderPath);

        foreach (var pluginsFolder in pluginsFolderPath)
        { 
            manifests.Add(deserializer.Deserialize<PluginManifest>(File.ReadAllText(pluginsFolder + "/PluginManifest.yaml")));
        }

        return manifests;
    }
    
    public static async Task LoadPluginsAsync(InteractionHandler commands, IServiceProvider serviceProvider)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        
        string folderPath = "Plugins";
        string[] pluginsFolderPath = Directory.GetDirectories(folderPath);
        
        Log.Information("Loading plugins...");

        foreach (string pluginsFolder in pluginsFolderPath)
        {
            var pluginManifest =
                deserializer.Deserialize<PluginManifest>(File.ReadAllText(pluginsFolder + "/PluginManifest.yaml"));

            try
            {
                var dllFile = Path.GetFullPath(pluginsFolder + "\\" + pluginManifest.FileName);

                Assembly assembly = Assembly.LoadFile(dllFile);

                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IY2DLPlugin).IsAssignableFrom(t));
                var pluginType = pluginTypes.ToArray()[0];
                var pluginInstance = (IY2DLPlugin)Activator.CreateInstance(pluginType);

                var initializeMethod = pluginType.GetMethod("Initialize");
                if (initializeMethod != null)
                {
                    initializeMethod.Invoke(pluginInstance, new[] { serviceProvider });
                }

                await commands.InitializePluginInteractionAsync(assembly);

                Log.Information("Loaded plugin {0}", pluginManifest.Name);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Can't load plugin {0}", pluginManifest.Name);
            }
        }
    }
}