using System.Diagnostics;
using System.Reflection;
using Discord.WebSocket;

namespace Y2DL.Models;

public class BotInfo
{
    public string Name = Program.GetCurrentUser().Username;
    public string AvatarUrl = Program.GetCurrentUser().GetAvatarUrl();
    public ulong UserId = Program.GetCurrentUser().Id;
    
    public string Y2DLVersion
    {
        get {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }

    public long ServerCount = Program.GetCurrentUserClient().Guilds.Count;

    public long AllServersUserCount
    {
        get
        {
            long users = 0;

            foreach (var s in Program.GetCurrentUserClient().Guilds)
                users += s.MemberCount;
            
            return users;
        }
    }

    public long GuildCount
    {
        get => ServerCount;
    }

    public long AllGuildsUserCount
    {
        get => AllServersUserCount;
    }

    public ulong GuildId { get; set; }
    public string GuildName { get; set; }
}