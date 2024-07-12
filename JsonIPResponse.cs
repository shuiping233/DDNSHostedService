using System.Net;
using System.Text.Json.Serialization;

namespace DDNSHosted;

public sealed record class JsonIPResponse(
    [property: JsonPropertyName("ip")] string Ip
)
{
    public static implicit operator IPAddress(JsonIPResponse jsonIPResponse) => IPAddress.Parse(jsonIPResponse.Ip);
}
