namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="DownloadAssetContentUseCase"/> error paths.</summary>
public sealed class DownloadAssetContentUseCaseTests
{
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IContentStorage _contentStorage = Substitute.For<IContentStorage>();

    private static Asset BuildAsset()
    {
        var clock = Substitute.For<IClock>();
        clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.New().Returns(Guid.NewGuid());

        return Asset.Create(
            clock,
            guidGenerator,
            Name.Create("test.txt").Value,
            ContentType.Create("text/plain").Value,
            UserId.Create(Guid.NewGuid().ToString()).Value
        ).Value;
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetNotFound_ShouldReturnNotFoundError()
    {
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Failure<Asset>(AssetErrors.NotFound)));

        var useCase = new DownloadAssetContentUseCase(_assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(Guid.NewGuid().ToString());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.NotFound);
    }

    [Test]
    public async Task ExecuteAsync_WhenStorageDownloadFails_ShouldReturnStorageError()
    {
        var asset = BuildAsset();
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));
        var storageError = Error.Create("Storage.DownloadFailed", "Download failed.");
        _contentStorage.DownloadAsync(Arg.Any<AssetPath>())
            .Returns(Task.FromResult(Result.Failure<Stream>(storageError)));

        var useCase = new DownloadAssetContentUseCase(_assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(asset.Id.Value.ToString());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(storageError);
    }
}