// IT WON'T HAPPEN AGAIN, i said.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Y2DL.Models;

namespace Y2DL.Database;

public class Y2dlDbContext : DbContext 
{
    public DbSet<DynamicChannelInfoMessages> DynamicChannelInfoMessages { get; set; }
    public DbSet<ChannelReleasesLatestVideo> ChannelReleasesLatestVideos { get; set; }
    
    public string DbPath { get; set; }
    
    public Y2dlDbContext()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        DbPath = Path.Combine(currentDirectory, "Y2DL_Database.db");
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

public class ChannelReleasesLatestVideo
{
    [Key]
    public string? Id { get; set; }
    public string? ChannelId { get; set; } 
}