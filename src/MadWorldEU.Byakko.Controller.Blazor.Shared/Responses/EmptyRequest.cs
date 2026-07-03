namespace MadWorldEU.Byakko.Responses;

/// <summary>An empty request body used for POST endpoints that require no input.</summary>
public sealed class EmptyRequest
{
    private EmptyRequest()
    {
    }
    
    public static EmptyRequest Create() => new();
}