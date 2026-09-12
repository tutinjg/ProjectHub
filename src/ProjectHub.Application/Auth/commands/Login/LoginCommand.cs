using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Auth.Common;
using ProjectHub.Application.Common.Interfaces;
using ValidationException = ProjectHub.Application.Common.Exceptions.ValidationException;

namespace ProjectHub.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Company)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email.ToLower() == emailNormalized, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Credentials", "Credenciales inválidas.")
            });
        }

        if (!user.IsActive)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Account", "El usuario se encuentra desactivado. Contacte a su administrador.")
            });
        }

        if (!user.Company.IsActive)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Company", "La organización asociada a esta cuenta está inactiva.")
            });
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email,
            user.Role.Name,
            user.CompanyId,
            accessToken,
            refreshToken.Token
        );
    }
}