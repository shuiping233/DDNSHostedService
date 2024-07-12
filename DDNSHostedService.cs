using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DDNSHosted;

public sealed class DDNSHostedService(
    HttpClient httpClient,
    ILogger<DDNSHostedService> logger,
    IOptionsSnapshot<DDNSConfig> config,
    IDDNSService ddns
    ) : IHostedService
{
    private const string GetWanIPv6Uri = "https://ipv6.jsonip.com/";
    private const string GetWanIPv4Uri = "https://ipv4.jsonip.com/";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("""
            监听的网络连接名称: {}
            启用IPv4: {}{}
            启用IPv6: {}{}
            """,
            config.Value.NetworkName,
            config.Value.EnableIPv4,
            config.Value.IsIPv4Nat ? "(Nat)" : string.Empty,
            config.Value.EnableIPv6,
            config.Value.IsIPv6Nat ? "(Nat)" : string.Empty);

        NetworkChange.NetworkAddressChanged += async (o, e) =>
            await NetworkChange_NetworkAddressChangedAsync(o, e);

        await NetworkChange_NetworkAddressChangedAsync(null, EventArgs.Empty);
    }

    private async Task UpdateIPAddressAsync()
    {
        logger.LogInformation("网络地址发生改变");

        var network = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(i => string.Equals(
                i.Name,
                config.Value.NetworkName,
                StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new InvalidOperationException($"找不到与 \"{config.Value.NetworkName}\" 匹配的网络连接");

        logger.LogTrace("找到匹配的网络连接 \"{networkName}\"", network.Name);

        var ips = network
            .GetIPProperties()
            .UnicastAddresses
            .Where(i => i.Address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
            .Select(i => i.Address)
            .Where(i => !i.IsIPv6LinkLocal
                && (i.AddressFamily is not AddressFamily.InterNetworkV6
                || i.ToString().StartsWith(config.Value.UsedIPv6Prefix)))
            .GroupBy(i => i.AddressFamily)
            .ToDictionary(i => i.Key);

        if (config.Value.EnableIPv6 && ips.TryGetValue(AddressFamily.InterNetworkV6, out var ipv6s))
        {
            IPAddress address;
            if (config.Value.IsIPv6Nat)
            {
                address = await httpClient.GetFromJsonAsync<JsonIPResponse>(GetWanIPv6Uri)
                    ?? throw new InvalidOperationException("无法获取外网 IPv6 地址");

                logger.LogInformation("ipv6.jsonip.com:    {}", address);
            }
            else
            {
                logger.LogInformation("发现的 IPv6:\r\n    {}", string.Join("\r\n    ", ipv6s.Select(i => $"[{i}]")));

                address = ipv6s.Last();
            }
            await ddns.UpdateIPv6(address);
        }

        if (config.Value.EnableIPv4 && ips.TryGetValue(AddressFamily.InterNetwork, out var ipv4s))
        {
            IPAddress address;
            if (config.Value.IsIPv4Nat)
            {
                address = await httpClient.GetFromJsonAsync<JsonIPResponse>(GetWanIPv4Uri)
                    ?? throw new InvalidOperationException("无法获取外网 IPv4 地址");

                logger.LogInformation("ipv4.jsonip.com:    {}", address);
            }
            else
            {
                logger.LogInformation("发现的 IPv4:\r\n    {}", string.Join("\r\n    ", ipv4s));

                address = ipv4s.Last();
            }
            await ddns.UpdateIPv4(address);
        }
    }

    private async Task NetworkChange_NetworkAddressChangedAsync(object? sender, EventArgs e)
    {
        try
        {
            await UpdateIPAddressAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新 IP 地址失败");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
