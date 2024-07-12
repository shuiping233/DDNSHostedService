using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using AlibabaCloud.TeaUtil.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace DDNSHosted;

public sealed class AliyunDDNSService(
    ILogger<AliyunDDNSService> logger,
    Client client,
    RuntimeOptions runtimeOptions,
    IOptionsSnapshot<DDNSConfig> config
    ) : IDDNSService
{
    /// <summary>
    /// 获取 domain id 和 解析记录
    /// </summary>
    /// <param name="domain">域名</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">使用未配置的域名时抛出</exception>
    private IEnumerable<DomainRecords> GetDomainRecords(string domain)
    {
        if (!config.Value.Domains.ContainsKey(domain))
            throw new InvalidOperationException($"未配置 domain: {domain}");

        DescribeDomainRecordsRequest request = new()
        {
            DomainName = domain
        };

        logger.LogInformation("尝试获取 domain id 和 解析记录");

        var response = client.DescribeDomainRecordsWithOptions(request, runtimeOptions);

        logger.LogInformation(
            "获取 domain id 成功，共 {count} 个记录",
            response.Body.DomainRecords.Record.Count);

        return response.Body.DomainRecords.Record
            .Select(i => new DomainRecords(
                i.DomainName,
                i.Type,
                i.Value,
                i.RecordId,
                i.RR)
            );
    }

    public Task UpdateIPv6(IPAddress ipv6)
    {
        foreach (var record in config
            .Value
            .Domains
            .Keys
            .SelectMany(GetDomainRecords)
            .Where(i => i.Type == "AAAA"))
        {
            // IPv6 不区分大小写
            if (string.Equals(record.Value, ipv6.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation(
                    "{domain} 的 {rr} 已经是 {ipv6}，无需更新",
                    record.DomainName,
                    record.RR,
                    record.Value);
                continue;
            }

            var request = record.CreateUpdateDomainRecordRequest(
                ipv6,
                config.Value.Domains[record.DomainName].TTL ?? config.Value.TTL);

            var response = client.UpdateDomainRecordWithOptions(request, runtimeOptions);

            logger.LogInformation(
                "更新 {domain} 的 {rr} 成功，记录 id: {recordId}",
                record.DomainName,
                record.RR,
                record.RecordId);
        }

        return Task.CompletedTask;
    }


    public async Task UpdateIPv4(IPAddress ipv4)
    {
        foreach (var domain in config.Value.Domains.Keys)
        {
            foreach (var record in GetDomainRecords(domain))
            {
                if (record.Value == ipv4.ToString())
                {
                    logger.LogInformation(
                        "{domain} 的 {rr} 已经是 {ipv4}，无需更新",
                        domain,
                        record.RR,
                        ipv4);
                    continue;
                }
                if (record.Type == "A")
                {
                    var request = record.CreateUpdateDomainRecordRequest(ipv4,
                config.Value.Domains[record.DomainName].TTL ?? config.Value.TTL);

                    var response = client.UpdateDomainRecordWithOptions(request, runtimeOptions);

                    logger.LogInformation(
                        "更新 {domain} 的 {rr} 成功，记录 id: {recordId}",
                        domain,
                        record.RR,
                        record.RecordId);
                }

            }
        };
    }

}
