using AlertManagerWebhook.MessageBuilders;
using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.Tests.MessageBuilders;

public class DingtalkMessageBuilderTests
{
    [Fact]
    public void Build_Should_Create_Firing_Alert_Message()
    {
        var alertDetail = new AlertDetail
        {
            IsFiring = true,
            Name = "HighMemoryUsage",
            Severity = "warning",
            EnvName = "staging",
            Project = "myapp",
            Instance = "10.0.0.2:9100",
            Host = "appserver-01",
            Description = "Memory usage has exceeded 85%",
            StartsAt = DateTime.Now.AddMinutes(-5),
            EndsAt = DateTime.Now
        };

        var builder = new DingtalkMessageBuilder();
        var result = Assert.IsType<DingtalkMessage>(builder.Build(alertDetail));

        Assert.Equal("触发告警", result.Markdown.Title);
        Assert.Contains("# <font color=\"#FFA500\">🚨 触发告警</font>", result.Markdown.Text);
        Assert.Contains("warning", result.Markdown.Text);
        Assert.Contains("#FFA500", result.Markdown.Text);
        Assert.Contains("10.0.0.2:9100", result.Markdown.Text);
    }

    [Fact]
    public void Build_Should_Create_Resolved_Alert_Message()
    {
        var alertDetail = new AlertDetail
        {
            IsFiring = false,
            Name = "HighMemoryUsage",
            Severity = "warning",
            EnvName = "staging",
            Project = "myapp",
            Instance = "10.0.0.2:9100",
            Host = "appserver-01",
            Description = "Memory usage has exceeded 85%",
            StartsAt = DateTime.Now.AddMinutes(-30),
            EndsAt = DateTime.Now.AddMinutes(-5)
        };

        var builder = new DingtalkMessageBuilder();
        var result = Assert.IsType<DingtalkMessage>(builder.Build(alertDetail));

        Assert.Equal("告警恢复", result.Markdown.Title);
        Assert.Contains("# <font color=\"#008000\">✅ 告警恢复</font>", result.Markdown.Text);
        Assert.Contains("#008000", result.Markdown.Text);
        Assert.Contains("恢复时间：", result.Markdown.Text);
        Assert.Contains("原告警内容：", result.Markdown.Text);
    }

    [Fact]
    public void Build_Should_Handle_Missing_Fields()
    {
        var alertDetail = new AlertDetail
        {
            IsFiring = true,
            Name = "SimpleAlert",
            Severity = "info",
            EnvName = string.Empty,
            Project = string.Empty,
            Instance = "localhost:9100",
            Host = string.Empty,
            Description = "Simple alert message",
            StartsAt = DateTime.Now,
            EndsAt = DateTime.Now
        };

        var builder = new DingtalkMessageBuilder();
        var result = Assert.IsType<DingtalkMessage>(builder.Build(alertDetail));

        Assert.DoesNotContain("环境名称：", result.Markdown.Text);
        Assert.DoesNotContain("项目名称：", result.Markdown.Text);
        Assert.DoesNotContain("主机名称：", result.Markdown.Text);
    }
}
