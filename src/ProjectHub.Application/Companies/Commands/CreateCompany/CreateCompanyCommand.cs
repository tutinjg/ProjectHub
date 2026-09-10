using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Companies.Commands.CreateCompany;

public record CreateCompanyCommand(string Name, string? Information) : IRequest<Guid>;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
    }
}

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company(request.Name)
        {
            Information = request.Information
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        return company.Id;
    }
}