using FluentAssertions;
using ProjectHub.Application.Auth.Commands.RegisterUser;
using Xunit;

namespace ProjectHub.UnitTests.Auth.Commands.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenPasswordLacksSpecialCharacter_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "Carlos",
            "Perez",
            "carlos@example.com",
            "Password2026", // Sin caracter especial (!?*.)
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "Carlos",
            "Perez",
            "correo-invalido",
            "Admin2026!",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "Carlos",
            "Perez",
            "carlos@example.com",
            "Admin2026!",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}