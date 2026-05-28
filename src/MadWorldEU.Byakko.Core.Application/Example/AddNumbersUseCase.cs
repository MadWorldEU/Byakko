using Microsoft.Extensions.Logging;

namespace MadWorldEU.Byakko.Example;

public sealed class AddNumbersUseCase(ILogger<AddNumbersUseCase> logger)
{
    public int Add(int a, int b)
    {
        logger.LogInformation("Adding {FirstNumber} and {SecondNumber}", a, b);
        
        return a + b;
    }
}