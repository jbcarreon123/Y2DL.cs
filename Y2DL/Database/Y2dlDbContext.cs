// IT WON'T HAPPEN AGAIN, i said.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Y2DL.Database;

public class Y2dlDbContext : DbContext 
{
    public DbSet<DynamicChannelInfoMessages> DynamicChannelInfoMessages { get; set; }
    
    public string DbPath { get; set; }
    
    public Y2dlDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "y2dl.db");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class DynamicChannelInfoMessages
{
    public ulong ChannelId { get; set; } 
    public ulong MessageId { get; set; }
    public string YoutubeChannelId { get; set; }
        
    [Key]
    public string Hash { get; set; }
}