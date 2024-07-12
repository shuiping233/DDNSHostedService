$params = @{
    Name = "Aliyun DDNS Hosted Service"
    BinaryPathName = 'C:\Program Files\DDNSHosted\DDNSHostedService.exe'
    DisplayName = "Aliyun DDNS Hosted Service"
    StartupType = "Automatic"
    Description = "DDNS域名更新服务，用于更新在阿里云注册域名的解析记录"
  }
  New-Service @params