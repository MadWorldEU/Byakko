using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="DownloadAssetContentUseCase"/> error paths.</summary>
public sealed class DownloadAssetContentUseCaseTests
{
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IEncryptionService _encryptionService = Substitute.For<IEncryptionService>();
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IContentStorage _contentStorage = Substitute.For<IContentStorage>();
    private readonly IAssetMetrics _metrics = Substitute.For<IAssetMetrics>();

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
            UserId.Create(Guid.NewGuid().ToString()).Value,
            ValidityPeriod.Create(30).Value
        ).Value;
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetNotFound_ShouldReturnNotFoundError()
    {
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Failure<Asset>(AssetErrors.NotFound)));

        var useCase = new DownloadAssetContentUseCase(_clock, _encryptionService, _assetRepository, _contentStorage, _metrics);

        var result = await useCase.QueryAsync(Guid.NewGuid().ToString(), password: "");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.NotFound);
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetIsExpired_ShouldReturnExpiredError()
    {
        var asset = BuildAsset();
        _clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0) + Duration.FromDays(31));
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));

        var useCase = new DownloadAssetContentUseCase(_clock, _encryptionService, _assetRepository, _contentStorage, _metrics);

        var result = await useCase.QueryAsync(asset.Id.Value.ToString(), password: "");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.Expired);
    }

    [Test]
    public async Task ExecuteAsync_WhenStorageDownloadFails_ShouldReturnStorageError()
    {
        var asset = BuildAsset();
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));
        var storageError = Error.Create("Storage.DownloadFailed", "Download failed.");
        _contentStorage.DownloadAsync(Arg.Any<AssetPath>())
            .Returns(Task.FromResult(Result.Failure<Stream>(storageError)));

        var useCase = new DownloadAssetContentUseCase(_clock, _encryptionService, _assetRepository, _contentStorage, _metrics);

        var result = await useCase.QueryAsync(asset.Id.Value.ToString(), password: "");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(storageError);
    }
}