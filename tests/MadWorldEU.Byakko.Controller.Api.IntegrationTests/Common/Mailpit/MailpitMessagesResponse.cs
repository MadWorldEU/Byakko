using System.Text.Json.Serialization;

namespace MadWorldEU.Byakko.Common.Mailpit;

internal sealed class MailpitMessagesResponse
{
    [JsonPropertyName("messages")]
    public List<MailpitMessage> Messages { get; init; } = [];
}

internal sealed class MailpitMessage
{
    [JsonPropertyName("Subject")]
    public string Subject { get; init; } = string.Empty;
}