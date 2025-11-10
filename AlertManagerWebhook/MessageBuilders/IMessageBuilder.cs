using AlertManagerWebhook.Models;
namespace AlertManagerWebhook.MessageBuilders;

public interface IMessageBuilder<TMessage>
{
    TMessage? Build(AlertDetail alert);
}
