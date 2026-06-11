using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Storages;

public sealed class Asset : Entity<Id>
{
    public const string DefaultPath = "assets";
    
    public Name Name { get; private set; } = null!;
    public ContentType ContentType { get; private set; } = null!;
    public UserId CreatedBy { get; private init; } = null!;
    public Size Size { get; private set; } = null!;
    public Instant CreatedAt { get; private init; }
    public Instant UpdatedAt { get; private set; }
    public Instant ExpiresAt { get; private set; }
    public Instant? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;
    
    /// <summary>
    /// Required for EF Core
    /// </summary>
    [UsedImplicitly]
    private Asset() {}

    private Asset(Id id, Name name, ContentType contentType, UserId createdBy, ValidityPeriod validityPeriod, Instant createdAt)
    {
        Id = id;
        Name = name;
        ContentType = contentType;
        CreatedBy = createdBy;
        Size = Size.Create(0).Value;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ExpiresAt = createdAt + Duration.FromDays(validityPeriod.Days);
        DeletedAt = null;
    }

    public static Result<Asset> Create(
        IClock clock,
        IGuidGenerator guidGenerator,
        Name name,
        ContentType contentType,
        UserId createdBy,
        ValidityPeriod validityPeriod)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new Asset(id, name, contentType, createdBy, validityPeriod, now);
    }

    public Result Delete(IClock clock)
    {
        if (IsDeleted)
        {
            return Result.Failure(AssetErrors.AlreadyDeleted);
        }

        var now = clock.GetCurrentInstant();
        DeletedAt = now;
        UpdatedAt = now;

        if (ExpiresAt > now)
        {
            ExpiresAt = now;
        }

        return Result.Success();
    }

    public Result UpdateSize(IClock clock, Size size)
    {
        if (Size.Value > 0)
        {
            return Result.Failure(AssetErrors.SizeAlreadySet);
        }

        var now = clock.GetCurrentInstant();
        Size = size;
        UpdatedAt = now;
        return Result.Success();
    }
    
    public bool IsExpired(IClock clock) => clock.GetCurrentInstant() > ExpiresAt;

    public AssetPath GetPath() => AssetPath.Create(DefaultPath, Id.Value.ToString()).Value;
}