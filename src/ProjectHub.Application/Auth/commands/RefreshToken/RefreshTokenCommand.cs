using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Auth.Common;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Entities;
using ValidationException = ProjectHub.Application.Common.Exceptions.ValidationException;

namespace ProjectHub.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es obligatorio.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenClean = request.RefreshToken?.Trim();

        if (string.IsNullOrWhiteSpace(tokenClean))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Token", "El token de refresco no puede estar vacío.")
            });
        }

        // Búsqueda del token ignorando filtros globales de tenant
        var storedToken = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .Include(rt => rt.User)
                .ThenInclude(u => u.Company)
            .FirstOrDefaultAsync(rt => rt.Token == tokenClean, cancellationToken);

        if (storedToken == null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Token", "El token de refresco no existe en el sistema.")
            });
        }

        if (storedToken.IsRevoked)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Token", "El token de refresco ya ha sido utilizado o revocado.")
            });
        }

        if (storedToken.IsExpired)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Token", $"El token ha expirado. Expiró el: {storedToken.ExpiresAt} UTC.")
            });
        }

        // Rotación: revocar el token actual
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email,
            user.Role?.Name ?? "User",
            user.CompanyId,
            newAccessToken,
            newRefreshToken.Token
        );
    }
}