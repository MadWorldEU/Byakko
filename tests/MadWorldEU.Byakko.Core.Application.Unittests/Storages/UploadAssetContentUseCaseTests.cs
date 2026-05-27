namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="UploadAssetContentUseCase"/> error paths.</summary>
public sealed class UploadAssetContentUseCaseTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IContentStorage _contentStorage = Substitute.For<IContentStorage>();

    private static Asset BuildAsset(string name = "test.txt", string contentType = "text/plain")
    {
        var clock = Substitute.For<IClock>();
        clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.New().Returns(Guid.NewGuid());

        return Asset.Create(
            clock,
            guidGenerator,
            Name.Create(name).Value,
            ContentType.Create(contentType).Value,
            UserId.Create(OwnerId.ToString()).Value,
            ValidityPeriod.Create(30).Value
        ).Value;
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetNotFound_ShouldReturnNotFoundError()
    {
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Failure<Asset>(AssetErrors.NotFound)));

        var useCase = new UploadAssetContentUseCase(_clock, _assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(
            Guid.NewGuid().ToString(), Stream.Null, 100,
            OwnerId.ToString(), "test.txt", "text/plain");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.NotFound);
    }

    [Test]
    public async Task ExecuteAsync_WhenUserIsNotOwner_ShouldReturnForbiddenError()
    {
        var asset = BuildAsset();
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));

        var useCase = new UploadAssetContentUseCase(_clock, _assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(
            asset.Id.Value.ToString(), Stream.Null, 100,
            OtherUserId.ToString(), "test.txt", "text/plain");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.Forbidden);
    }

    [Test]
    public async Task ExecuteAsync_WhenFileNameDoesNotMatch_ShouldReturnFileNameMismatchError()
    {
        var asset = BuildAsset(name: "original.txt");
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));

        var useCase = new UploadAssetContentUseCase(_clock, _assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(
            asset.Id.Value.ToString(), Stream.Null, 100,
            OwnerId.ToString(), "different.txt", "text/plain");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.FileNameMismatch);
    }

    [Test]
    public async Task ExecuteAsync_WhenContentTypeDoesNotMatch_ShouldReturnContentTypeMismatchError()
    {
        var asset = BuildAsset(contentType: "text/plain");
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));

        var useCase = new UploadAssetContentUseCase(_clock, _assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(
            asset.Id.Value.ToString(), Stream.Null, 100,
            OwnerId.ToString(), "test.txt", "application/pdf");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.ContentTypeMismatch);
    }

    [Test]
    public async Task ExecuteAsync_WhenSizeAlreadySet_ShouldReturnSizeAlreadySetError()
    {
        var asset = BuildAsset();
        _clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        asset.UpdateSize(_clock, Size.Create(100).Value);
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));
        _contentStorage.UploadAsync(Arg.Any<AssetPath>(), Arg.Any<Stream>())
            .Returns(Task.FromResult(Result.Success()));

        var useCase = new UploadAssetContentUseCase(_clock, _assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(
            asset.Id.Value.ToString(), Stream.Null, 100,
            OwnerId.ToString(), "test.txt", "text/plain");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.SizeAlreadySet);
    }

    [Test]
    public async Task ExecuteAsync_WhenStorageUploadFails_ShouldReturnStorageError()
    {
        var asset = BuildAsset();
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));
        var storageError = Error.Create("Storage.UploadFailed", "Upload failed.");
        _contentStorage.UploadAsync(Arg.Any<AssetPath>(), Arg.Any<Stream>())
            .Returns(Task.FromResult(Result.Failure(storageError)));

        var useCase = new UploadAssetContentUseCase(_clock, _assetRepository, _contentStorage);

        var result = await useCase.ExecuteAsync(
            asset.Id.Value.ToString(), Stream.Null, 100,
            OwnerId.ToString(), "test.txt", "text/plain");

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(storageError);
    }
}