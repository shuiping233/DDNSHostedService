using System.Net;

namespace DDNSHosted;

public interface IDDNSService
{
    Task UpdateIPv4(IPAddress ipv4);
    Task UpdateIPv6(IPAddress ipv6);
}
