namespace MadWorldEU.Byakko.Common;

/// <summary>
/// Represents a structured error response returned when a request cannot be fulfilled.
/// </summary>
public sealed class FailureResponse
{
    /// <summary>
    /// A machine-readable error code in the format <c>Domain.Reason</c> (e.g. <c>Asset.NotFound</c>).
    /// </summary>
    public required string Code { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code returned by the server (e.g. 400, 404, 409).
    /// </summary>
    public required int StatusCode { get; set; }
    
    /// <summary>
    /// A human-readable description of the failure.
    /// </summary>
    public required string Description { get; set; } = string.Empty;
}