using ArchUnitNET.Domain;

namespace MadWorldEU.Byakko;

/// <summary>Loads all assemblies under test and exposes typed layer providers for architecture rules.</summary>
public abstract class BaseArchitectureTests
{
    protected static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssembly(typeof(IAdminMarker).Assembly)
            .LoadAssembly(typeof(IBlazorSharedMarker).Assembly)
            .LoadAssembly(typeof(IPortalMarker).Assembly)
            .LoadAssembly(typeof(IBuildingBlocksMarker).Assembly)
            .LoadAssembly(typeof(IDomainMarker).Assembly)
            .LoadAssembly(typeof(IApplicationMarker).Assembly)
            .LoadAssembly(typeof(IContractsMarker).Assembly)
            .LoadAssembly(typeof(IObjectStorageMarker).Assembly)
            .LoadAssembly(typeof(IPostgresqlMarker).Assembly)
            .LoadAssembly(typeof(ISecurityMarker).Assembly)
            .LoadAssembly(typeof(IApiMarker).Assembly)
            .LoadAssembly(typeof(IStatusMarker).Assembly)
            .Build();

    protected readonly IObjectProvider<IType> AdminLayer =
        Types().That().ResideInAssembly(typeof(IAdminMarker).Assembly).As("Admin Layer");

    protected readonly IObjectProvider<IType> BlazorSharedLayer =
        Types().That().ResideInAssembly(typeof(IBlazorSharedMarker).Assembly).As("BlazorShared Layer");

    protected readonly IObjectProvider<IType> PortalLayer =
        Types().That().ResideInAssembly(typeof(IPortalMarker).Assembly).As("Portal Layer");

    protected readonly IObjectProvider<IType> BuildingBlocksLayer =
        Types().That().ResideInAssembly(typeof(IBuildingBlocksMarker).Assembly).As("BuildingBlocks Layer");

    protected readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly(typeof(IDomainMarker).Assembly).As("Domain Layer");

    protected readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly(typeof(IApplicationMarker).Assembly).As("Application Layer");

    protected readonly IObjectProvider<IType> ContractsLayer =
        Types().That().ResideInAssembly(typeof(IContractsMarker).Assembly).As("Contracts Layer");

    protected readonly IObjectProvider<IType> ObjectStorageLayer =
        Types().That().ResideInAssembly(typeof(IObjectStorageMarker).Assembly).As("ObjectStorage Layer");

    protected readonly IObjectProvider<IType> PostgresqlLayer =
        Types().That().ResideInAssembly(typeof(IPostgresqlMarker).Assembly).As("Postgresql Layer");

    protected readonly IObjectProvider<IType> SecurityLayer =
        Types().That().ResideInAssembly(typeof(ISecurityMarker).Assembly).As("Security Layer");

    protected readonly IObjectProvider<IType> ApiLayer =
        Types().That().ResideInAssembly(typeof(IApiMarker).Assembly).As("Api Layer");

    protected readonly IObjectProvider<IType> StatusLayer =
        Types().That().ResideInAssembly(typeof(IStatusMarker).Assembly).As("Status Layer");
}