using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Application.Projects.Common;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? TargetEndDate
) : IRequest<ProjectDto>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del proyecto es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no debe superar los 150 caracteres.");

        RuleFor(x => x.TargetEndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.TargetEndDate.HasValue)
            .WithMessage("La fecha estimada de fin debe ser posterior o igual a la fecha de inicio.");
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CreateProjectCommandHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var companyId = _tenantService.CompanyId
            ?? throw new UnauthorizedAccessException("No se pudo identificar el Tenant del usuario actual.");

        var project = new Project(
            request.Name.Trim(),
            request.Description?.Trim(),
            companyId,
            request.StartDate,
            request.TargetEndDate
        );

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.TargetEndDate,
            0,
            project.CreatedBy,
            project.CreatedAt
        );
    }
}