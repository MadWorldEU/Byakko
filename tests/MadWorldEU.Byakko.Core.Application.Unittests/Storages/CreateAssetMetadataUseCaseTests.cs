using System.Net;
using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="CreateAssetMetadataUseCase"/> error paths.</summary>
public sealed class CreateAssetMetadataUseCaseTests
{
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IGuidGenerator _guidGenerator = Substitute.For<IGuidGenerator>();
    private readonly IAssetRepository _repository = Substitute.For<IAssetRepository>();
    private readonly IDomainEventsDispatcher _domainEventsDispatcher = Substitute.For<IDomainEventsDispatcher>();
    private readonly IOptions<AssetSettings> _settings = Options.Create(new AssetSettings { ValidityPeriodInDays = 30, MaxFilesEachUser = 10, MaxUploadSizeInBytes = 1073741824 });
    private readonly IPAddress _ipAddress = new IPAddress([127, 0, 0, 1]);

    [Test]
    public async Task ExecuteAsync_WhenNameIsEmpty_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "", ContentType = "text/plain", ExpiresInDays = 30 };
        
        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(NameErrors.Empty);
    }

    [Test]
    public async Task ExecuteAsync_WhenContentTypeIsInvalid_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "not-a-valid-mime", ExpiresInDays = 30 };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ContentTypeErrors.Invalid);
    }

    [Test]
    public async Task ExecuteAsync_WhenUserIdIsInvalid_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain", ExpiresInDays = 30 };

        var result = await useCase.ExecuteAsync(request, "not-a-guid", _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserIdErrors.Invalid);
    }

    [Test]
    public async Task ExecuteAsync_WhenSizeIsNegative_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain", ExpiresInDays = 30, Size = -1 };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SizeErrors.Negative);
    }

    [Test]
    public async Task ExecuteAsync_WhenSizeExceedsMaximum_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain", ExpiresInDays = 30, Size = 1073741825 };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.FileTooLarge);
    }

    [Test]
    public async Task ExecuteAsync_WhenExpiresInDaysIsZero_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain", ExpiresInDays = 0 };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ValidityPeriodErrors.MustBePositive);
    }

    [Test]
    public async Task ExecuteAsync_WhenExpiresInDaysExceedsMaximum_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain", ExpiresInDays = 31 };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ValidityPeriodErrors.ExceedsMaximum);
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositoryFails_ShouldReturnSaveFailedError()
    {
        _clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        _guidGenerator.New().Returns(Guid.NewGuid());
        _repository.GetCountOfActiveAssetsAsync(Arg.Any<UserId>()).Returns(Task.FromResult(Result.Success(0)));
        _repository.AddAsync(Arg.Any<Asset>()).Returns(Task.FromResult(Result.Failure(AssetErrors.SaveFailed)));

        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _domainEventsDispatcher, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain", ExpiresInDays = 30 };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString(), _ipAddress);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.SaveFailed);
    }
}