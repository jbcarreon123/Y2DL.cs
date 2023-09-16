namespace Y2DL.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class Y2DLServiceAttribute : Attribute
{
    public string ServiceName { get; }

    public Y2DLServiceAttribute(string serviceName)
    {
        ServiceName = serviceName;
    }
}