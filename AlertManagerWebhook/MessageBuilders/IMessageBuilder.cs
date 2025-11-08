using AlertManagerWebhook.Models;
namespace AlertManagerWebhook.MessageBuilders;

public interface IMessageBuilder
{
    object? Build(Notification notification);
}
