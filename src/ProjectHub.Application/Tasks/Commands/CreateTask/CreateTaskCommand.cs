using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Common.Exceptions;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Application.Tasks.Common;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Application.Tasks.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string? Description,
    Guid ProjectId,
    TaskPriority Priority = TaskPriority.Medium,
    TaskItemStatus Status = TaskItemStatus.Todo,
    DateTime? DueDate = null,
    Guid? AssignedToUserId = null
) : IRequest<TaskDto>;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título de la tarea es obligatorio.")
            .MaximumLength(200).WithMessage("El título no debe superar los 200 caracteres.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("El identificador del proyecto es obligatorio.");
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CreateTaskCommandHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var companyId = _tenantService.CompanyId
            ?? throw new UnauthorizedAccessException("Tenant no identificado.");

        // Validar que el proyecto existe y pertenece al tenant
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        // Validar usuario asignado si se provee
        User? assignedUser = null;
        if (request.AssignedToUserId.HasValue)
        {
            assignedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.AssignedToUserId.Value, cancellationToken);

            if (assignedUser == null)
                throw new NotFoundException(nameof(User), request.AssignedToUserId.Value);
        }

        // Obtener el último orden en la columna seleccionada
        var maxOrder = await _context.Tasks
            .Where(t => t.ProjectId == request.ProjectId && t.Status == request.Status)
            .Select(t => (int?)t.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var task = new TaskItem(request.Title.Trim(), request.ProjectId, companyId, request.Priority)
        {
            Description = request.Description?.Trim(),
            Status = request.Status,
            DueDate = request.DueDate,
            Order = maxOrder + 1,
            AssignedToUserId = request.AssignedToUserId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.Order,
            task.ProjectId,
            assignedUser != null ? new UserSummaryDto(assignedUser.Id, assignedUser.GetFullName(), assignedUser.Email) : null,
            task.CreatedBy,
            task.CreatedAt
        );
    }
}