using Discord.WebSocket;

namespace Y2DL.Models;

public class BotInfo
{
    public string Name = Program.GetCurrentUser().Username;
    public string AvatarUrl = Program.GetCurrentUser().GetAvatarUrl();
    public ulong UserId = Program.GetCurrentUser().Id;

    public ulong GuildId { get; set; }
    public string GuildName { get; set; }
}