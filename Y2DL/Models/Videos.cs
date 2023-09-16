using Newtonsoft.Json;

namespace Y2DL.Models;

public class Videos
{
    [JsonProperty("items")]
    public List<VideoItem> Items { get; set; }
}

public class VideoItem
{
    [JsonProperty("short")]
    public Short Short { get; set; } 
}

public class VideoShort
{
    [JsonProperty("available")]
    public bool Available = false;
}