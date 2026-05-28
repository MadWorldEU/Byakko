namespace MadWorldEU.Byakko.Development;

public sealed class GetAccountResponse
{
    public IReadOnlyList<ClaimDto> Claims { get; init; } = [];
}