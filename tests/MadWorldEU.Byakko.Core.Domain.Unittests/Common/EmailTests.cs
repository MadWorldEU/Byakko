namespace MadWorldEU.Byakko.Common;

/// <summary>Unit tests for <see cref="Email"/> value object validation.</summary>
public sealed class EmailTests
{
    [Test]
    public void Create_WhenEmailIsNull_ShouldReturnEmptyError()
    {
        var result = Email.Create(null);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailErrors.Empty);
    }

    [Test]
    public void Create_WhenEmailIsEmpty_ShouldReturnEmptyError()
    {
        var result = Email.Create(string.Empty);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailErrors.Empty);
    }

    [Test]
    [Arguments("notanemail")]
    [Arguments("user@")]
    [Arguments("user@domain")]
    [Arguments("@example.com")]
    [Arguments("user@@example.com")]
    public void Create_WhenEmailIsInvalid_ShouldReturnInvalidError(string email)
    {
        var result = Email.Create(email);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EmailErrors.Invalid);
    }

    [Test]
    [Arguments("user@example.com")]
    [Arguments("user@mail.example.com")]
    [Arguments("user+alias@example.com")]
    public void Create_WhenEmailIsValid_ShouldReturnSuccess(string email)
    {
        var result = Email.Create(email);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(email);
    }
}