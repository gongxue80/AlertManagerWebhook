namespace AlertManagerWebhook.Models;

using System.Text.Json.Serialization;

public record LarkMessage
{
    [JsonPropertyName("msg_type")]
    public string MsgType { get; set; } = "interactive";
    [JsonPropertyName("card")]
    public LarkCard Card { get; set; } = new();
}

public record LarkCard
{
    [JsonPropertyName("config")]
    public LarkCardConfig Config { get; set; } = new();
    [JsonPropertyName("header")]
    public LarkCardHeader Header { get; set; } = new();
    [JsonPropertyName("elements")]
    public LarkCardElement[] Elements { get; set; } = Array.Empty<LarkCardElement>();
}

public record LarkCardConfig
{
    [JsonPropertyName("wide_screen_mode")]
    public bool WideScreenMode { get; set; }
}

public record LarkCardHeader
{
    [JsonPropertyName("title")]
    public LarkCardHeaderTitle Title { get; set; } = new();
    [JsonPropertyName("template")]
    public string Template { get; set; } = string.Empty;
}

public record LarkCardHeaderTitle
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "plain_text";
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public record LarkCardElement
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "div";
    [JsonPropertyName("text")]
    public LarkCardElementText Text { get; set; } = new();
}

public record LarkCardElementText
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = "lark_md";
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
