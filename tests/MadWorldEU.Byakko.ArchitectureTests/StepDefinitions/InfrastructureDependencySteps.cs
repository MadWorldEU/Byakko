using ArchUnitNET.Domain;

namespace MadWorldEU.Byakko.StepDefinitions;

/// <summary>Step definitions for infrastructure dependency architecture rules.</summary>
[Binding]
[Scope(Feature = "Infrastructure Dependency Rules")]
public sealed class InfrastructureDependencySteps : BaseArchitectureTests
{
    private readonly Dictionary<string, IObjectProvider<IType>> _layers;

    /// <summary>Initialises the layer lookup used by the step definitions.</summary>
    public InfrastructureDependencySteps()
    {
        _layers = new Dictionary<string, IObjectProvider<IType>>
        {
            ["BuildingBlocks"] = BuildingBlocksLayer,
            ["Domain"] = DomainLayer,
            ["Application"] = ApplicationLayer,
            ["Contracts"] = ContractsLayer,
            ["ObjectStorage"] = ObjectStorageLayer,
            ["Postgresql"] = PostgresqlLayer,
            ["Security"] = SecurityLayer,
            ["Api"] = ApiLayer
        };
    }

    [Given("the architecture is loaded")]
    public static void GivenTheArchitectureIsLoaded()
    {
        // Architecture is loaded statically in BaseArchitectureTests.
    }

    [Then("the {} layer should not depend on the {} layer")]
    public void ThenTheLayerShouldNotDependOnTheLayer(string sourceLayerName, string targetLayerName)
    {
        var sourceLayer = _layers[sourceLayerName];
        var targetLayer = _layers[targetLayerName];

        IArchRule rule = Types().That().Are(sourceLayer).Should()
            .NotDependOnAny(targetLayer);

        rule.HasNoViolations(Architecture).ShouldBeTrue(rule.Description);
    }
}