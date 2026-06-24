namespace MadWorldEU.Byakko.Configurations;

public sealed class BlazorError
{
    public string Code { get; init; }
    public string Description { get; init; }  
    
    private BlazorError(string code, string description)
    {
        Code = code;
        Description = description;
    }
    
    public static BlazorError Create(string code, string description) => new(code, description);
}