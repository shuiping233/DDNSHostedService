# DDNSHostedService
## 介绍
- 基于.net8.0主机服务和阿里云SDK开发的DDNS工具，用于获取IPv4和IPv6地址并更新到阿里云的域名解析记录中。

## 功能
- [x] 从本机特定网络连接或者通过公网ip服务器获取IPv4和IPv6地址
- [x] 使用本机特定网络连接中获取IP地址时，监听本机特定网络连接的ip地址变化并更新IP地址
- [x] 获取IP地址后，更新到阿里云的域名解析记录中
- [ ] 记录日志到windows事件中或者保存到log文件中

## 运行环境
- [.NET Runtime 8.0.x](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- windows / linux (未经测试)

## 部署
- 从[Release](https://github.com/shuiping233/DDNSHostedService/releases)下载编译好的二进制文件压缩包，或者拉取此项目进行编译

- 模版配置文件在`appsettings.json.example`中，请根据模板文件配置示例和说明修改配置内容，然后将`appsettings.json.example`重命名成`appsettings.json`
### windows
- 修改`AddService.ps1`的`BinaryPathName`，将其值修改为实际`DDNSHostedService.exe`实际存放的路径，其余值视实际情况按需修改，然后运行此脚本
- 使用ps1脚本添加服务后，使用powershell启动服务
```powershell
# 如果创建服务时修改了服务名称，请替换此处的服务名称
Start-Service -Name "Aliyun DDNS Hosted Service"
```
### linux
- 暂未编写

## 配置文件解析


|键|值类型|描述|
|-|-|-|
|Logging.DDNSHosted.DDNSHostedService|string|日志打印等级，可以填入`Trace`,`Debug`,`Information`,`Warning`,`Error`,`Critical`|
|DDNS.NetworkName|string|要监听的网络连接的名称|
|DDNS.EnableIPv4|bool|若此值为`true`，将ipv4地址更新到域名解析记录中|
|DDNS.EnableIPv6|bool|若此值为`true`，将ipv6地址更新到域名解析记录中|
|DDNS.IsIPv4Nat|bool|若此值为`true`,将通过外网的公网ip服务去获取本机实际出口ipv4地址|
|DDNS.IsIPv6Nat|bool|若此值为`true`,将通过外网的公网ip服务去获取本机实际出口ipv6地址|
|DDNS.UsedIPv6Prefix|string|获取ipv6地址时根据此值筛选出具有特定前缀的ipv6地址，不使用此功能则值可为空字符串`""`|
|DDNS.Domains|dictionary<MainDomainName,dictionary<Hosts,list\<Host\>>>|可填入多个key-value对，key为主域名，例如`baidu.com`（这里的主域名无论是多少级域名均可）,value为一个dictionary（第二层），而第二层dictionary中key为`Hosts`,value为一个列表，列表每个元素为Host名（子域）；第二层dictionary还可以填写可选key`TTL`值为int，可单独定义特定域名解析记录的TTL，优先级比`DDNS.TTL`高|
|DDNS.TTL|long|域名解析记录的TTL，[`解析结果在Local DNS中的缓存时间`](https://help.aliyun.com/zh/dns/set-ttl?spm=a2c4g.11186623.0.0.af4f585916w0LZ)|
|AlibabaCloud.AccessKeyId|string|阿里云的AccessKeyId|
|AlibabaCloud.AliyunAccessKeySecret|string|阿里云的AccessKeySecret|
|AlibabaCloud.Endpoint|string|阿里云云解析服务的服务器域名，详见[阿里云云解析服务区域列表](https://next.api.aliyun.com/product/Alidns)|



## 特别感谢
- 本项目完全由 [舰队的偶像-岛风酱](https://github.com/frg2089) 编写，非常感谢岛风大佬的辛勤付出和耐心教导。还要感谢他给我提供了域名，由此才有此项目的诞生。