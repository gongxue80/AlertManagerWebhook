using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class MessageBuilderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MessageBuilderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取指定类型的消息构建器
    /// </summary>
    public IMessageBuilder<TMessage> GetMessageBuilder<TMessage>(Receiver receiverType) where TMessage : class
    {
        return receiverType switch
        {
            Receiver.Lark when typeof(TMessage) == typeof(LarkMessage) =>
                (IMessageBuilder<TMessage>)_serviceProvider.GetRequiredService<LarkMessageBuilder>(),
            Receiver.Dingtalk when typeof(TMessage) == typeof(DingtalkMessage) =>
                (IMessageBuilder<TMessage>)_serviceProvider.GetRequiredService<DingtalkMessageBuilder>(),
            _ => throw new NotSupportedException($"Unsupported receiver type {receiverType} for message type {typeof(TMessage)}")
        };
    }
}
