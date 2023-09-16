using Newtonsoft.Json;

namespace Y2DL.Models;

public class PlaylistItems
{
    [JsonProperty("items")]
    public List<Item> Items { get; set; }
}

public class Item
{
    [JsonProperty("snippet")]
    public Snippet Snippet { get; set; }
}

public class Snippet
{
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("resourceId")]
    public ResourceId ResourceId { get; set; }
}

public class ResourceId
{
    [JsonProperty("videoId")]
    public string VideoId { get; set; } 
}