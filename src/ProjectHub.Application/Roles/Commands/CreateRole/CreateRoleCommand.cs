using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Roles.Commands.CreateRole;

public record CreateRoleCommand(string Name, string? Description, Guid CompanyId) : IRequest<Guid>;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no debe superar los 50 caracteres.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El CompanyId es obligatorio.");
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            CompanyId = request.CompanyId,
            IsSystemRole = false
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}