using FluentAssertions;
using ProjectHub.Application.Auth.Commands.Login;
using Xunit;

namespace ProjectHub.UnitTests.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("usuario-invalido", "Password123!")]
    [InlineData("test@domain.com", "")]
    public void Validate_WhenFieldsAreInvalid_ShouldFail(string email, string password)
    {
        var command = new LoginCommand(email, password);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenFieldsAreValid_ShouldPass()
    {
        var command = new LoginCommand("admin@domain.com", "SecurePass1!");
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}