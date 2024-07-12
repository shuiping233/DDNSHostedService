using AlibabaCloud.SDK.Alidns20150109.Models;
using System.Net;

namespace DDNSHosted;

public sealed record class DomainRecords(
    string DomainName,
    string Type,
    string Value,
    string RecordId,
    string RR)
{
    public UpdateDomainRecordRequest CreateUpdateDomainRecordRequest(IPAddress address, long? ttl = default) => new()
        {
            RecordId = RecordId,
            RR = RR,
            Type = Type,
            Value = address.ToString(),
            TTL = ttl
        };
}
