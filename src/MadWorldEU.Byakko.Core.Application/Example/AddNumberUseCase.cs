using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Example;

public sealed class AddNumberUseCase(ILogger<AddNumberUseCase> logger)
{
    public int Add(int a, int b)
    {
        logger.LogInformation("Adding {a} and {b}", a, b);
        
        return a + b;
    }
}