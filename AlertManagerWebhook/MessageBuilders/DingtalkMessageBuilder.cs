using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class DingtalkMessageBuilder : IMessageBuilder<DingtalkMessage>
{
    public DingtalkMessage Build(AlertDetail alert)
    {
        // 根据告警级别和状态选择不同的颜色
        string titleColor;
        string severityColor;

        if (alert.IsFiring)
        {
            // 触发状态下根据告警级别选择颜色
            titleColor = alert.Severity.ToLower() switch
            {
                "critical" => "#FF0000", // 严重告警 - 红色
                "error" => "#FF0000",    // 错误告警 - 红色
                "warning" => "#FFA500",  // 警告告警 - 橙色
                "info" => "#00BFFF",     // 信息告警 - 蓝色
                "notice" => "#00BFFF",   // 通知告警 - 蓝色
                _ => "#FF0000"           // 默认 - 红色
            };

            // 告警状态颜色与标题颜色一致
            severityColor = titleColor;
        }
        else
        {
            // 恢复状态下默认绿色
            titleColor = "#008000";
            severityColor = "#008000";
        }

        var title = alert.IsFiring
            ? $"# <font color=\"{titleColor}\">🚨 触发告警</font>\n"
            : $"# <font color=\"{titleColor}\">✅ 告警恢复</font>\n";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine($"> **告警名称：** <font color=\"#FFA500\">{alert.Name}</font>  ");
        sb.AppendLine($"> **告警状态：** <font color=\"{severityColor}\">{alert.Severity}</font>  ");
        sb.AppendLine($"> **告警实例：** {alert.Instance}  ");
        if (!string.IsNullOrEmpty(alert.Host))
            sb.AppendLine($"> **主机名称：** {alert.Host}  ");
        if (!string.IsNullOrEmpty(alert.EnvName))
            sb.AppendLine($"> **环境名称：** {alert.EnvName}  ");
        if (!string.IsNullOrEmpty(alert.Project))
            sb.AppendLine($"> **项目名称：** {alert.Project}  ");
        sb.AppendLine($"> **触发时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss zzz}  ");

        if (!alert.IsFiring)
        {
            sb.AppendLine($"> **恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss zzz}  ");
        }

        sb.AppendLine($"---");
        sb.AppendLine(alert.IsFiring ? alert.Description : $"原告警内容：{alert.Description}");

        return new DingtalkMessage
        {
            Markdown = new DingtalkMarkdown
            {
                Title = alert.IsFiring ? "触发告警" : "告警恢复",
                Text = sb.ToString().TrimEnd()
            }
        };
    }
}
