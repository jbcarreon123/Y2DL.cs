namespace Y2DL.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DiscordCommandsAttribute : Attribute
{
    public string ServiceName { get; }

    public DiscordCommandsAttribute(string serviceName)
    {
        ServiceName = serviceName;
    }
}