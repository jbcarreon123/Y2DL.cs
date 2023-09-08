using Newtonsoft.Json;

namespace Y2DL.Models;

public class OAPIResponse
{
    [JsonProperty("items")] public List<Items> Items { get; set; }
}

public class Items
{
    [JsonProperty("short")] public Short Short { get; set; }
}

public class Short
{
    [JsonProperty("available")] public bool Available;
}