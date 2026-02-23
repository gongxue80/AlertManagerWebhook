namespace AlertManagerWebhook.Models;

using System.Text.Json.Serialization;

public record LarkMessage
{
    [JsonPropertyName("msg_type")]
    public string MsgType { get; init; } = "interactive";

    [JsonPropertyName("card")]
    public LarkCard Card { get; init; } = new();
}

public record LarkCard
{
    [JsonPropertyName("config")]
    public LarkCardConfig Config { get; init; } = new();

    [JsonPropertyName("header")]
    public LarkCardHeader Header { get; init; } = new();

    [JsonPropertyName("elements")]
    public LarkCardElement[] Elements { get; init; } = Array.Empty<LarkCardElement>();
}

public record LarkCardConfig
{
    [JsonPropertyName("wide_screen_mode")]
    public bool WideScreenMode { get; init; }
}

public record LarkCardHeader
{
    [JsonPropertyName("title")]
    public LarkCardHeaderTitle Title { get; init; } = new();

    [JsonPropertyName("template")]
    public string Template { get; init; } = string.Empty;
}

public record LarkCardHeaderTitle
{
    [JsonPropertyName("tag")]
    public string Tag { get; init; } = "plain_text";

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}

public record LarkCardElement
{
    [JsonPropertyName("tag")]
    public string Tag { get; init; } = "div";

    [JsonPropertyName("text")]
    public LarkCardElementText Text { get; init; } = new();

    [JsonPropertyName("fields")]
    public LarkCardElementField[]? Fields { get; init; }
    // Actions will be added later
}

public record LarkCardElementText
{
    [JsonPropertyName("tag")]
    public string Tag { get; init; } = "lark_md";

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}

public record LarkCardElementField
{
    [JsonPropertyName("is_short")]
    public bool IsShort { get; init; } = true;

    [JsonPropertyName("text")]
    public LarkCardElementText Text { get; init; } = new();
}
