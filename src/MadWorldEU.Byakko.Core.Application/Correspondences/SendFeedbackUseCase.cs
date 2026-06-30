namespace MadWorldEU.Byakko.Correspondences;

/// <summary>Use case for sending user feedback to the administrator.</summary>
public sealed class SendFeedbackUseCase(ICorrespondenceService correspondenceService, ICorrespondenceMetrics metrics)
{
    /// <summary>Sends the feedback from the given user to the administrator via the correspondence service.</summary>
    public async Task<Result> ExecuteAsync(SendFeedbackRequest request, string userId)
    {
        var userIdResult = UserId.CreateOrEmpty(userId);
        if (userIdResult.IsFailure) return Result.Failure(userIdResult.Error);
        
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return Result.Failure(emailResult.Error);
        
        var message = BuildBody(request.Message, emailResult.Value, userIdResult.Value);
        
        var result = await correspondenceService.SendToAdministratorAsync("Byakko - New feedback", message);
        if (result.IsSuccess)
        {
            metrics.RecordFeedbackSent();
        }

        return result;
    }

    private static string BuildBody(string message, Email email, UserId userId)
    {
        return $"""
                A user has submitted feedback via the Byakko portal.

                --- Metadata ---
                User ID:     {userId.Value}
                Reply-to:    {email.Value}

                --- Message ---
                {message}
                """;
    }
}