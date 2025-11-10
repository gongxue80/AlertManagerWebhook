# AlertManagerWebhook

一个用于接收 AlertManager 告警并转发到钉钉、飞书的 .NET Webhook 服务。

## 功能简介

- 支持 AlertManager Webhook 格式
- 支持钉钉、飞书消息格式自动转换

## 快速开始

### 1. 环境准备

- .NET 8.0 运行环境（Ubuntu 可用 `sudo apt install dotnet-runtime-8.0`）

### 2. 构建与发布
```bash
# 发布为 Linux 可运行包
make pub
# 或
dotnet publish -c Release -r linux-x64 --self-contained false
```

### 3. 配置
- 修改 `appsettings.json`，填写钉钉和飞书 Webhook 地址。
- 本地开发敏感信息建议用环境变量或 user-secrets 管理。
```bash
dotnet user-secrets init
dotnet user-secrets --project AlertManagerWebhook set "DingtalkUrl" "your dingtalk url"
dotnet user-secrets --project AlertManagerWebhook set "LarkUrl" "your Lark url"
```

### 4. 运行
```bash
cd AlertManagerWebhook/bin/Release/net9.0/linux-x64/publish
# 运行服务
 dotnet AlertManagerWebhook.dll
# 或自定义端口
 ASPNETCORE_URLS="http://0.0.0.0:5001" dotnet AlertManagerWebhook.dll
```

### 5. 部署到服务器

- 推荐用 systemd 管理服务，见下方示例。
- 可用 Nginx 做反向代理。

### 6. systemd 示例
```
[Unit]
Description=AlertManager Webhook Service
After=network.target

[Service]
WorkingDirectory=/opt/alertmanager-webhook
ExecStart=/usr/bin/dotnet /opt/alertmanager-webhook/AlertManagerWebhook.dll
Restart=always
User=www-data
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
```

## 安全建议

- 不要将敏感信息提交到仓库。
- 生产环境敏感信息建议用环境变量或服务器本地配置。

## License

MIT
