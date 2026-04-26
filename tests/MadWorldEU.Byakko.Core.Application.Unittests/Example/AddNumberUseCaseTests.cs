using Shouldly;

namespace MadWorldEU.Byakko.Example;

public class AddNumberUseCaseTests
{
    [Test]
    public void Add_TwoNumbers_ReturnSum()
    {
        // Arrange
        const int firstNumber = 1;
        const int secondNumber = 2;
        var useCase = new AddNumberUseCase();
        
        // Act
        var sum = useCase.Add(firstNumber, secondNumber);

        // Assert
        sum.ShouldBe(3);
    }
}