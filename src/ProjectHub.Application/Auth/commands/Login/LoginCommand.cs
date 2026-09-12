using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public LoginCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        // Buscamos al usuario ignorando temporalmente filtros de tenant para permitir login por credencial
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Company)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email.ToLower() == emailNormalized, cancellationToken);

        // Mensaje genérico para no revelar si el correo existe o no
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

        return new AuthResponseDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email,
            user.Role.Name,
            user.CompanyId
        );
    }
}