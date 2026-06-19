namespace MadWorldEU.Byakko.Application;

/// <summary>Unit tests for <see cref="GeneratePasswordUseCase"/>.</summary>
public sealed class GeneratePasswordUseCaseTests
{
    [Test]
    public void Execute_WhenCalled_ShouldReturnPasswordWithLengthOfTwenty()
    {
        var password = GeneratePasswordUseCase.Execute();

        password.Length.ShouldBe(20);
    }

    [Test]
    public void Execute_WhenCalled_ShouldContainAtLeastOneUppercaseLetter()
    {
        var password = GeneratePasswordUseCase.Execute();

        password.Any(char.IsUpper).ShouldBeTrue();
    }

    [Test]
    public void Execute_WhenCalled_ShouldContainAtLeastOneLowercaseLetter()
    {
        var password = GeneratePasswordUseCase.Execute();

        password.Any(char.IsLower).ShouldBeTrue();
    }

    [Test]
    public void Execute_WhenCalled_ShouldContainAtLeastOneDigit()
    {
        var password = GeneratePasswordUseCase.Execute();

        password.Any(char.IsDigit).ShouldBeTrue();
    }

    [Test]
    public void Execute_WhenCalled_ShouldContainAtLeastOneSpecialCharacter()
    {
        const string special = "!@#$%^&*()-_=+[]{}<>?";

        var password = GeneratePasswordUseCase.Execute();

        password.Any(c => special.Contains(c)).ShouldBeTrue();
    }

    [Test]
    public void Execute_WhenCalledMultipleTimes_ShouldReturnUniquePasswords()
    {
        var passwords = Enumerable.Range(0, 10).Select(_ => GeneratePasswordUseCase.Execute()).ToList();

        passwords.Distinct().Count().ShouldBe(10);
    }
}