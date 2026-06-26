using MadWorldEU.Byakko.Configurations;
using MadWorldEU.Byakko.Correspondences;

namespace MadWorldEU.Byakko.Endpoints.Correspondences;

internal static class CorrespondenceEndpoints
{
    internal static void AddCorrespondenceEndpoints(this WebApplication app)
    {
        var correspondenceEndpoints = app.MapGroup("/correspondences")
            .WithTags("Correspondences");

        correspondenceEndpoints.MapPost("/feedback", async (SendFeedbackRequest request, ClaimsPrincipal user, SendFeedbackUseCase useCase) =>
            {
                var userId = user.GetUserId();

                var result = await useCase.ExecuteAsync(request, userId);
                return result.Match(
                    onSuccess: () => Results.Ok(),
                    onFailure: error => error.ToBadRequest()
                );
            })
            .WithName("SendFeedback")
            .RequireRateLimiting(RateLimiterPolicies.PublicPost);
    }
}