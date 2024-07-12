namespace DDNSHosted;

/// <summary>
/// DDNS 系统设置
/// </summary>
public sealed class DDNSConfig
{
    /// <summary>
    /// 要应用 DDNS 的网络连接的名称
    /// </summary>
    public string NetworkName { get; set; } = string.Empty;

    public bool EnableIPv4 { get; set; } = false;
    public bool IsIPv4Nat { get; set; } = true;
    public bool EnableIPv6 { get; set; } = true;
    public bool IsIPv6Nat { get; set; } = false;

    /// <summary>
    /// 应根据 IPv6 前缀过滤 IPv6 地址
    /// </summary>
    /// <remarks>
    /// 注意：这不是 IPv6 地址的前缀，而是 IPv6 地址的前缀的子集。
    /// </remarks>
    public string UsedIPv6Prefix { get; set; } = string.Empty;
    public Dictionary<string, DDNSConfigHost> Domains { get; set; } = [];
    public long? TTL { get; set; } = 600;
}