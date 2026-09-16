using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using ProjectHub.Application.Auth.Commands.Login;
using ProjectHub.Application.Common.Exceptions;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Entities;
using Xunit;

namespace ProjectHub.UnitTests.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock = new Mock<IJwtTokenGenerator>();

        _handler = new LoginCommandHandler(
            _contextMock.Object,
            _hasherMock.Object,
            _jwtMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowValidationException()
    {
        // Arrange: DbContext sin usuarios
        var emptyUsers = new List<User>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(emptyUsers.Object);

        var command = new LoginCommand("notfound@domain.com", "Secret123!");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert: Validar tipo de excepción y contenido del diccionario Errors
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Credentials");
        exception.Which.Errors["Credentials"].Should().Contain("Credenciales inválidas.");
    }

    [Fact]
    public async Task Handle_WhenPasswordDoesNotMatch_ShouldThrowValidationException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var user = new User("John", "Doe", "john@domain.com", "HashedValue", companyId, Guid.NewGuid())
        {
            Company = new Company("Acme") { Id = companyId, IsActive = true },
            Role = new Role { Id = Guid.NewGuid(), Name = "User" }
        };

        var usersList = new List<User> { user }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(usersList.Object);

        _hasherMock.Setup(h => h.VerifyPassword("WrongPassword!", "HashedValue"))
            .Returns(false);

        var command = new LoginCommand("john@domain.com", "WrongPassword!");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert: Validar tipo de excepción y contenido del diccionario Errors
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Credentials");
        exception.Which.Errors["Credentials"].Should().Contain("Credenciales inválidas.");
    }
}