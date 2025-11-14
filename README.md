# AlertManagerWebhook

一个用于接收 AlertManager 告警并转发到钉钉、飞书的 .NET Webhook 服务。

## 功能简介

- 支持 AlertManager Webhook 格式
- 自动转换为钉钉、飞书消息格式
- 支持告警恢复通知
- 支持多告警批量处理
- 支持自定义配置

## 快速开始

### 1. 环境准备

- .NET 8.0 运行环境
  - Ubuntu: `sudo apt install dotnet-runtime-8.0`
  - Windows: 下载并安装 .NET 8.0 SDK from [官方网站](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

### 2. 构建与发布

```bash
# 快速发布为 Linux 可运行包
make pub

# 或使用完整命令
dotnet publish -c Release -r linux-x64 --self-contained false

# 发布为 Windows 可运行包
dotnet publish -c Release -r win-x64 --self-contained false

# 发布为其他平台可运行包
dotnet publish -c Release -r <runtime-id> --self-contained false
```

### 3. 配置 AlertManager

在 AlertManager 配置文件 `alertmanager.yml` 中添加以下配置：

```yaml
# 全局配置
global:
  resolve_timeout: 5m

# 路由配置
route:
  group_by: ['alertname']
  group_wait: 30s
  group_interval: 5m
  repeat_interval: 5m
  receiver: 'webhook-notifications'

# 接收者配置
receivers:
- name: 'webhook-notifications'
  webhook_configs:
  # 飞书告警
  - url: 'http://localhost:5000/lark/YOUR_LARK_WEBHOOK_TOKEN'
    send_resolved: true

  # 钉钉告警
  - url: 'http://localhost:5000/dingtalk/YOUR_DINGTALK_WEBHOOK_TOKEN'
    send_resolved: true
```

### 4. 运行服务

```bash
# 进入发布目录
cd AlertManagerWebhook/bin/Release/net8.0/linux-x64/publish

# 运行服务（默认端口 5000）
dotnet AlertManagerWebhook.dll

# 自定义端口运行
ASPNETCORE_URLS="http://0.0.0.0:8080" dotnet AlertManagerWebhook.dll

# 在后台运行（Linux）
nohup dotnet AlertManagerWebhook.dll > webhook.log 2>&1 &
```

### 5. 部署到服务器

#### systemd 示例

```ini
[Unit]
Description=AlertManager Webhook Service
After=network.target

[Service]
WorkingDirectory=/opt/alertmanager-webhook
ExecStart=/usr/bin/dotnet /opt/alertmanager-webhook/AlertManagerWebhook.dll
Restart=always
User=www-data
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

```bash
# 安装服务
sudo cp alertmanager-webhook.service /etc/systemd/system/

# 重新加载配置
sudo systemctl daemon-reload

# 启动服务
sudo systemctl start alertmanager-webhook

# 设置开机自启
sudo systemctl enable alertmanager-webhook
```

#### Nginx 反向代理示例

```nginx
server {
    listen 80;
    server_name webhook.example.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## 配置文件

### 支持的配置项

创建 `appsettings.json` 文件自定义配置：

```json
{
  "WebhookConfig": {
    "LarkBaseUrl": "https://open.larksuite.com/open-apis/bot/v2/hook/",
    "DingtalkBaseUrl": "https://oapi.dingtalk.com/robot/send?access_token="
  }
}
```

- `LarkBaseUrl`: 飞书机器人 webhook 基础 URL
- `DingtalkBaseUrl`: 钉钉机器人 webhook 基础 URL

### 环境变量配置

也可以通过环境变量配置：

```bash
# Linux
export WebhookConfig:LarkBaseUrl="https://custom-url.com/"
dotnet AlertManagerWebhook.dll

# Windows
set WebhookConfig:LarkBaseUrl="https://custom-url.com/"
dotnet AlertManagerWebhook.dll
```

## 支持的接收器

### Lark (飞书)

URL 格式：`http://localhost:5000/lark/{token}`

### Dingtalk (钉钉)

URL 格式：`http://localhost:5000/dingtalk/{token}`

## 测试

项目包含单元测试，可通过以下命令运行：

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test AlertManagerWebhook.Tests/AlertManagerWebhook.Tests.csproj
```

## 开发

```bash
# 开发模式运行
make dev

# 或直接使用 dotnet watch
dotnet watch --project AlertManagerWebhook
```

## 安全建议

- 不要将敏感信息提交到代码仓库
- 生产环境建议使用 HTTPS
- 限制 Webhook 服务的访问 IP
- 定期轮换机器人 tokens

## License

MIT
