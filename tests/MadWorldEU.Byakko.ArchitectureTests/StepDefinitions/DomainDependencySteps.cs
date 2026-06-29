using ArchUnitNET.Domain;

namespace MadWorldEU.Byakko.StepDefinitions;

/// <summary>Step definitions for domain dependency architecture rules.</summary>
[Binding]
[Scope(Feature = "Domain Dependency Rules")]
public sealed class DomainDependencySteps : BaseArchitectureTests
{
    private readonly Dictionary<string, IObjectProvider<IType>> _layers;

    /// <summary>Initialises the layer lookup used by the step definitions.</summary>
    public DomainDependencySteps()
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
            ["Mail"] = MailLayer,
            ["Api"] = ApiLayer,
            ["Status"] = StatusLayer
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

        var rule = Types().That().Are(sourceLayer).Should()
            .NotDependOnAny(targetLayer);

        rule.HasNoViolations(Architecture).ShouldBeTrue(rule.Description);
    }
}