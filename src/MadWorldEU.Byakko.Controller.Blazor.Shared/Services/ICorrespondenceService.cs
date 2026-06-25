using MadWorldEU.Byakko.Common;
using MadWorldEU.Byakko.Correspondences;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the Correspondences API endpoints for use in Blazor WebAssembly applications.</summary>
public interface ICorrespondenceService
{
    /// <summary>Sends user feedback to the administrator.</summary>
    Task<ResultResponse<EmptyResponse>> SendFeedbackAsync(SendFeedbackRequest request, bool isAuthenticated);
}