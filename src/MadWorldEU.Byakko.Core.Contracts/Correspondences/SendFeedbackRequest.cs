namespace MadWorldEU.Byakko.Correspondences;

/// <summary>Request payload for submitting user feedback.</summary>
public sealed class SendFeedbackRequest
{
    /// <summary>Email address of the user submitting feedback.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Feedback message body.</summary>
    public string Message { get; set; } = string.Empty;
}