using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Unit tests for <see cref="Asset"/> domain entity error paths.</summary>
public sealed class AssetTests
{
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
    public void Delete_WhenAlreadyDeleted_ShouldReturnAlreadyDeletedError()
    {
        var clock = Substitute.For<IClock>();
        clock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(0));
        var asset = BuildAsset();
        asset.Delete(clock);

        var result = asset.Delete(clock);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AssetErrors.AlreadyDeleted);
    }
}