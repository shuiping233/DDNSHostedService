namespace DDNSHosted;

public sealed class DDNSConfigHost
{
    public string[] Hosts { get; set; } = [];
    public long? TTL { get; internal set; }
}