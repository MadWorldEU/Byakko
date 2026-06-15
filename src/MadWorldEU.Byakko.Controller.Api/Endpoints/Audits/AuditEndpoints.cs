using MadWorldEU.Byakko.Audits;
using MadWorldEU.Byakko.Configurations;

namespace MadWorldEU.Byakko.Endpoints.Audits;

internal static class AuditEndpoints
{
    internal static void AddAuditEndpoints(this WebApplication app)
    {
        var auditEndpoints = app.MapGroup("/audits")
            .WithTags("Audits");

        auditEndpoints.MapGet("/{entityId}", async (string entityId, GetAuditLogsUseCase useCase) =>
            {
                var result = await useCase.QueryAsync(entityId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => Results.BadRequest(error.Description)
                );
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("GetAuditLogs");
    }
}