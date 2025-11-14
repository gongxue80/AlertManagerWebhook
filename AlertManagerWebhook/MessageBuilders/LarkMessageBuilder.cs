using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class LarkMessageBuilder : IMessageBuilder<LarkMessage>
{
    /// <summary>
    /// 构建 Lark 消息对象
    /// </summary>
    public LarkMessage? Build(AlertDetail alert)
    {
        var isFiring = alert.IsFiring;
        var title = isFiring ? "🚨 告警触发" : "✅ 告警恢复";
        var statusText = isFiring ? alert.Severity : "恢复";
        var timeTitle = isFiring ? "触发" : "开始";
        var alertDetailText = isFiring ? alert.Description : $"原告警：{alert.Description}";

        // 构建结构化的卡片内容
        var elements = new List<LarkCardElement>
        {
            // 告警基本信息区域 - 使用分栏展示
            new LarkCardElement
            {
                Tag = "div",
                Fields = new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**告警名称：** {alert.Name}" } },
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**告警状态：** {statusText}" } }
                }
                .Concat(!string.IsNullOrEmpty(alert.EnvName) ? new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**环境：** {alert.EnvName}" } }
                } : Array.Empty<LarkCardElementField>())
                .Concat(!string.IsNullOrEmpty(alert.Project) ? new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**项目：** {alert.Project}" } }
                } : Array.Empty<LarkCardElementField>())
                .ToArray()
            },

            // 实例和主机信息 - 使用分栏
            new LarkCardElement
            {
                Tag = "div",
                Fields = new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**实例：** {alert.Instance}" } }
                }
                .Concat(!string.IsNullOrEmpty(alert.Host) ? new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**主机：** {alert.Host}" } }
                } : Array.Empty<LarkCardElementField>())
                .ToArray()
            },
            // 时间信息 - 使用分栏
            new LarkCardElement
            {
                Tag = "div",
                Fields = new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**{timeTitle}时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss zzz}" } }
                }
                .Concat(!isFiring ? new[]
                {
                    new LarkCardElementField { Text = new LarkCardElementText { Content = $"**恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss zzz}" } }
                } : Array.Empty<LarkCardElementField>())
                .ToArray()
            },
            // 告警描述
            new LarkCardElement
            {
                Tag = "div",
                Text = new LarkCardElementText
                {
                    Content = $"**告警详情：**\n{alertDetailText}"
                }
            }
        };

        // 构建并返回 Lark 消息对象
        // 根据告警级别和状态选择不同的颜色模板
        string templateColor;
        if (isFiring)
        {
            // 触发状态下根据告警级别选择颜色
            templateColor = alert.Severity.ToLower() switch
            {
                "critical" => "red", // 严重告警 - 红色
                "error" => "red",    // 错误告警 - 红色
                "warning" => "orange", // 警告告警 - 橙色
                "info" => "blue",    // 信息告警 - 蓝色
                "notice" => "blue",  // 通知告警 - 蓝色
                _ => "red"           // 默认 - 红色
            };
        }
        else
        {
            // 恢复状态下默认绿色
            templateColor = "green";
        }

        return new LarkMessage
        {
            Card = new LarkCard
            {
                Config = new LarkCardConfig { WideScreenMode = true },
                Header = new LarkCardHeader
                {
                    Title = new LarkCardHeaderTitle { Content = title },
                    Template = templateColor
                },
                Elements = elements.ToArray()
            }
        };
    }
}