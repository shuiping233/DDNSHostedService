using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.TeaUtil.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DDNSHosted;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton<HttpClient>()
    .AddSingleton(provider =>
    {
        Config config = new();
        builder.Configuration.Bind("AlibabaCloud", config);

        return new Client(config);
    })
    .AddSingleton<RuntimeOptions>()
    .AddSingleton<IDDNSService, AliyunDDNSService>()
    .AddHostedService<DDNSHostedService>()
    .AddWindowsService()
    .Configure<DDNSConfig>(builder.Configuration.GetSection("DDNS"));

IHost host = builder.Build();
host.Run();