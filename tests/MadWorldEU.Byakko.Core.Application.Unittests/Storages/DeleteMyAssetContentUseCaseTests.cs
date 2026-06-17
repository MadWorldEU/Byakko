using System.Net;
using Microsoft.Extensions.Logging;
using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="DeleteMyAssetContentUseCase"/> error paths.</summary>
public sealed class DeleteMyAssetContentUseCaseTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IAssetRepository _assetRepository = Substitute.For<IAssetRepository>();
    private readonly IContentStorage _contentStorage = Substitute.For<IContentStorage>();
    private readonly IDomainEventsDispatcher _domainEventsDispatcher = Substitute.For<IDomainEventsDispatcher>();
    private readonly ILogger<DeleteMyAssetContentUseCase> _logger = Substitute.For<ILogger<DeleteMyAssetContentUseCase>>();

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
            UserId.Create(OwnerId.ToString()).Value,
            ValidityPeriod.Create(30).Value
        ).Value;
    }

    [Test]
    public async Task ExecuteAsync_WhenUserIsNotOwner_ShouldReturnForbiddenError()
    {
        var asset = BuildAsset();
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));

        var useCase = new DeleteMyAssetContentUseCase(_clock, _assetRepository, _contentStorage, _domainEventsDispatcher, _logger);

        var ipAddress = new IPAddress([127, 0, 0, 1]);

        var result = await useCase.ExecuteAsync(asset.Id.Value.ToString(), OtherUserId.ToString(), ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.Forbidden);
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetNotFound_ShouldReturnNotFoundError()
    {
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Failure<Asset>(AssetErrors.NotFound)));

        var useCase = new DeleteMyAssetContentUseCase(_clock, _assetRepository, _contentStorage, _domainEventsDispatcher, _logger);

        var ipAddress = new IPAddress([127, 0, 0, 1]);

        var result = await useCase.ExecuteAsync(Guid.NewGuid().ToString(), OwnerId.ToString(), ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.NotFound);
    }

    [Test]
    public async Task ExecuteAsync_WhenAssetAlreadyDeleted_ShouldReturnAlreadyDeletedError()
    {
        var asset = BuildAsset();
        _clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        asset.Delete(_clock);
        _assetRepository.FindAsync(Arg.Any<Id>()).Returns(Task.FromResult(Result.Success(asset)));
        _contentStorage.DeleteAsync(Arg.Any<AssetPath>()).Returns(Task.FromResult(Result.Success()));

        var useCase = new DeleteMyAssetContentUseCase(_clock, _assetRepository, _contentStorage, _domainEventsDispatcher, _logger);

        var ipAddress = new IPAddress([127, 0, 0, 1]);

        var result = await useCase.ExecuteAsync(asset.Id.Value.ToString(), OwnerId.ToString(), ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.AlreadyDeleted);
    }
}