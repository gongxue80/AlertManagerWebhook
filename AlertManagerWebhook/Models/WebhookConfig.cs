using System.Text.Json.Serialization;

namespace AlertManagerWebhook.Models;

public class WebhookConfig
{
    /// <summary>
    /// Lark webhook base URL
    /// </summary>
    public string LarkBaseUrl { get; set; } = "https://open.feishu.cn/open-apis/bot/v2/hook/";

    /// <summary>
    /// Dingtalk webhook base URL
    /// </summary>
    public string DingtalkBaseUrl { get; set; } = "https://oapi.dingtalk.com/robot/send?access_token=";
}
