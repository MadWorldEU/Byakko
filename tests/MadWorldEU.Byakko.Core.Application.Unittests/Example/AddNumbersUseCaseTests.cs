using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MadWorldEU.Byakko.Example;

public sealed class AddNumbersUseCaseTests
{
    [Test]
    public void Add_TwoNumbers_ReturnSum()
    {
        // Arrange
        const int firstNumber = 1;
        const int secondNumber = 2;
        
        var logger = Substitute.For<ILogger<AddNumbersUseCase>>();
        var useCase = new AddNumbersUseCase(logger);
        
        // Act
        var sum = useCase.Add(firstNumber, secondNumber);

        // Assert
        sum.ShouldBe(3);
    }
}