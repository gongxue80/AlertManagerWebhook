using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class LarkMessageBuilder : IMessageBuilder
{
    public object Build(AlertDetail alert)
    {
        var isFiring = alert.IsFiring;
        var title = isFiring ? "🚨 告警触发" : "✅ 告警恢复";
        var statusText = isFiring ? alert.Severity : "恢复";
        var timeTitle = isFiring ? "触发" : "开始";
        var alertDetailText = isFiring ? alert.Description : $"原告警：{alert.Description}";

        var elements = new List<LarkCardElement>
        {
            new()
            {
                Tag = "div",
                Text = new LarkCardElementText
                {
                    Content =
                        $"**告警名称：** {alert.Name}\n"
                        + $"**告警状态：** {statusText}"
                        + (
                            !string.IsNullOrEmpty(alert.EnvName)
                                ? $"\n**环境：** {alert.EnvName}"
                                : ""
                        )
                        + (
                            !string.IsNullOrEmpty(alert.Project)
                                ? $"\n**项目：** {alert.Project}"
                                : ""
                        ),
                },
            },
            new()
            {
                Tag = "div",
                Text = new LarkCardElementText
                {
                    Content =
                        $"**实例：** {alert.Instance}"
                        + (!string.IsNullOrEmpty(alert.Host) ? $"\n**主机：** {alert.Host}" : ""),
                },
            },
            new()
            {
                Tag = "div",
                Text = new LarkCardElementText
                {
                    Content =
                        $"**{timeTitle}时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss zzz}"
                        + (
                            !isFiring
                                ? $"\n**恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss zzz}"
                                : ""
                        ),
                },
            },
            new()
            {
                Tag = "div",
                Text = new LarkCardElementText { Content = $"**告警详情：**\n{alertDetailText}" },
            },
        };

        return new LarkMessage
        {
            Card = new LarkCard
            {
                Config = new LarkCardConfig { WideScreenMode = true },
                Header = new LarkCardHeader
                {
                    Title = new LarkCardHeaderTitle { Content = title },
                    Template = AlertSeverity.GetLarkTemplateColor(alert),
                },
                Elements = elements.ToArray(),
            },
        };
    }
}
