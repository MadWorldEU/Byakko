using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Services;

/// <summary>Wraps the general Storage API endpoints for use in Blazor WebAssembly applications.</summary>
public interface IStorageService
{
    /// <summary>Returns aggregate statistics for all active assets in storage.</summary>
    Task<ResultResponse<GetStorageStatisticsResponse>> GetStorageStatisticsAsync();
}