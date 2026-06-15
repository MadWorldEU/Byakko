namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="GetAssetMetadataUseCase"/> error paths.</summary>
public sealed class GetAssetMetadataUseCaseTests
{
    private readonly IAssetRepository _repository = Substitute.For<IAssetRepository>();

    [Test]
    public async Task ExecuteAsync_WhenAssetIdIsInvalid_ShouldReturnFailure()
    {
        var useCase = new GetAssetMetadataUseCase(_repository);

        var result = await useCase.QueryAsync("not-a-guid");

        result.IsFailure.ShouldBeTrue();
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetNotFound_ShouldReturnNotFoundError()
    {
        _repository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Failure<Asset>(AssetErrors.NotFound)));

        var useCase = new GetAssetMetadataUseCase(_repository);

        var result = await useCase.QueryAsync(Guid.NewGuid().ToString());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.NotFound);
    }
}