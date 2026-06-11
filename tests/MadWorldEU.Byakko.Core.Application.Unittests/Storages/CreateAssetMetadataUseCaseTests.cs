using MadWorldEU.Byakko.Systems;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="CreateAssetMetadataUseCase"/> error paths.</summary>
public sealed class CreateAssetMetadataUseCaseTests
{
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IGuidGenerator _guidGenerator = Substitute.For<IGuidGenerator>();
    private readonly IAssetRepository _repository = Substitute.For<IAssetRepository>();
    private readonly IOptions<AssetSettings> _settings = Options.Create(new AssetSettings { ValidityPeriodInDays = 30 });

    [Test]
    public async Task ExecuteAsync_WhenNameIsEmpty_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _settings);
        var request = new CreateAssetRequest { Name = "", ContentType = "text/plain" };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString());

        result.IsFailure.ShouldBeTrue();
    }

    [Test]
    public async Task ExecuteAsync_WhenContentTypeIsInvalid_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "not-a-valid-mime" };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString());

        result.IsFailure.ShouldBeTrue();
    }

    [Test]
    public async Task ExecuteAsync_WhenUserIdIsInvalid_ShouldReturnFailure()
    {
        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain" };

        var result = await useCase.ExecuteAsync(request, "not-a-guid");

        result.IsFailure.ShouldBeTrue();
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositoryFails_ShouldReturnSaveFailedError()
    {
        _clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        _guidGenerator.New().Returns(Guid.NewGuid());
        _repository.AddAsync(Arg.Any<Asset>()).Returns(Task.FromResult(Result.Failure(AssetErrors.SaveFailed)));

        var useCase = new CreateAssetMetadataUseCase(_clock, _guidGenerator, _repository, _settings);
        var request = new CreateAssetRequest { Name = "test.txt", ContentType = "text/plain" };

        var result = await useCase.ExecuteAsync(request, Guid.NewGuid().ToString());

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.SaveFailed);
    }
}