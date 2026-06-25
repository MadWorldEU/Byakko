namespace MadWorldEU.Byakko.Correspondences;

/// <summary>Use case for sending user feedback to the administrator.</summary>
public sealed class SendFeedbackUseCase(ICorrespondenceService correspondenceService)
{
    /// <summary>Sends the feedback from the given user to the administrator via the correspondence service.</summary>
    public async Task<Result> ExecuteAsync(SendFeedbackRequest request, string userId)
    {
        return await correspondenceService.SendToAdministratorAsync("Feedback", "Feedback received");
    }
}