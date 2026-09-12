using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Auth.Common;
using ProjectHub.Application.Common.Exceptions;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Entities;
using ValidationException = ProjectHub.Application.Common.Exceptions.ValidationException;

namespace ProjectHub.Application.Auth.Commands.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Guid CompanyId,
    Guid RoleId
) : IRequest<AuthResponseDto>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no debe exceder 50 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(50).WithMessage("El apellido no debe exceder 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[\!\?\*\.]").WithMessage("La contraseña debe contener al menos un carácter especial (!?*.).");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El identificador de la empresa es obligatorio.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("El rol asignado es obligatorio.");
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == emailNormalized, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Email", "El correo electrónico ya se encuentra registrado.")
            });
        }

        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

        if (company == null)
            throw new NotFoundException(nameof(Company), request.CompanyId);

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            throw new NotFoundException(nameof(Role), request.RoleId);

            var user = new User(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                emailNormalized,
                _passwordHasher.HashPassword(request.Password),
                request.CompanyId,
                request.RoleId
            );

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Asignar el rol al objeto para la emisión del token
        user.Role = role;

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email,
            role.Name,
            user.CompanyId,
            accessToken,
            refreshToken.Token
        );
    }
}