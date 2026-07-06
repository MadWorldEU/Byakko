using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.Configurations;

namespace MadWorldEU.Byakko.Endpoints.Accounts;

internal static class AccountsEndpoints
{
    internal static void AddAccountsEndpoints(this WebApplication app)
    {
        var accountEndpoints = app.MapGroup("/accounts")
            .WithTags("Accounts");

        accountEndpoints.MapGet("/", async (int page, GetAccountsPendingDeletionUseCase useCase) =>
            {
                var result = await useCase.QueryAsync(page);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("GetAccountsPendingDeletion");

        accountEndpoints.MapPost("/{userId}/confirm-deletion", async (string userId, ConfirmDeletionAccountUseCase useCase) =>
            {
                var result = await useCase.ExecuteAsync(userId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AccountErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.Code == AccountErrors.DeletionNotRequested.Code
                            ? error.ToConflict()
                            : error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("ConfirmDeletionAccount");

        accountEndpoints.MapPost("/me", async (ClaimsPrincipal user, CreateMyAccountUseCase useCase) =>
            {
                var userId = user.GetUserId();

                var result = await useCase.ExecuteAsync(userId);
                return result.Match(
                    onSuccess: response => Results.Created($"/accounts/me", response),
                    onFailure: error => error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.User)
            .WithName("CreateMyAccount");

        accountEndpoints.MapGet("/me", async (ClaimsPrincipal user, GetMyAccountUseCase useCase) =>
            {
                var userId = user.GetUserId();

                var result = await useCase.QueryAsync(userId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AccountErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.User)
            .WithName("GetMyAccount");

        accountEndpoints.MapPost("/me/deletion-request", async (ClaimsPrincipal user, RequestDeletionMyAccountUseCase useCase) =>
            {
                var userId = user.GetUserId();

                var result = await useCase.ExecuteAsync(userId);
                return result.Match(
                    onSuccess: Results.Ok,
                    onFailure: error => error.Code == AccountErrors.NotFound.Code
                        ? error.ToNotFound()
                        : error.Code == AccountErrors.NotActive.Code
                            ? error.ToConflict()
                            : error.ToBadRequest()
                );
            })
            .RequireAuthorization(AuthorizationPolicies.User)
            .WithName("RequestDeletionMyAccount");
    }
}