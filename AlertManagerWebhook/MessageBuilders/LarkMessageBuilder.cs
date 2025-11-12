using System.Text;
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

        // 用 StringBuilder 构建内容，分块插入
        var sb = new StringBuilder();
        sb.AppendLine($"**告警名称：** {alert.Name}");
        sb.AppendLine($"**告警状态：** {alert.Severity}");
        sb.AppendLine("___");
        sb.AppendLine($"**告警实例：** {alert.Instance}");
        if (!string.IsNullOrEmpty(alert.Host))
            sb.AppendLine($"**主机名称：** {alert.Host}");
        if (!string.IsNullOrEmpty(alert.EnvName))
            sb.AppendLine($"**环境名称：** {alert.EnvName}");
        if (!string.IsNullOrEmpty(alert.Project))
            sb.AppendLine($"**项目名称：** {alert.Project}");
        sb.AppendLine("___");
        sb.AppendLine($"**触发时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss zzz}");

        if (!isFiring)
        {
            sb.AppendLine($"**恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss zzz}");
        }

        sb.AppendLine(isFiring ? alert.Description : $"原告警内容：{alert.Description}");

        // 构建 Lark 消息对象
        return new LarkMessage
        {
            Card = new LarkCard
            {
                Config = new LarkCardConfig { WideScreenMode = true },
                Header = new LarkCardHeader
                {
                    Title = new LarkCardHeaderTitle { Content = title },
                    Template = isFiring ? "red" : "green"
                },
                Elements =
                [
                    new LarkCardElement
                    {
                        Text = new LarkCardElementText { Content = sb.ToString().TrimEnd(), Tag = "lark_md" }
                    }
                ]
            }
        };
    }
}
