using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Returns the upload limits and current file usage for the requesting user.</summary>
public sealed class GetUserUploadLimitsUseCase(IAssetRepository repository, IOptions<AssetSettings> settings)
{
    /// <summary>Executes the use case for the given <paramref name="userId"/>.</summary>
    public async Task<Result<GetUserUploadLimitsResponse>> QueryAsync(string userId)
    {
        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure) return userIdResult.Error;

        var countResult = await repository.GetCountOfActiveAssetsAsync(userIdResult.Value);
        if (countResult.IsFailure) return countResult.Error;

        return new GetUserUploadLimitsResponse
        {
            MaxFiles = settings.Value.MaxFilesEachUser,
            MaxUploadSizeInBytes = settings.Value.MaxUploadSizeInBytes,
            ActiveFiles = countResult.Value
        };
    }
}