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
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Application.Tasks.Commands.UpdateTaskStatus;

public record UpdateTaskStatusCommand(
    Guid TaskId,
    TaskItemStatus NewStatus,
    int TargetOrder
) : IRequest<Unit>;

public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.TargetOrder).GreaterThanOrEqualTo(1);
    }
}

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateTaskStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            throw new NotFoundException(nameof(TaskItem), request.TaskId);

        var oldStatus = task.Status;
        var oldOrder = task.Order;
        var newStatus = request.NewStatus;
        var targetOrder = request.TargetOrder;

        if (oldStatus == newStatus)
        {
            // Reordenamiento dentro de la misma columna
            if (oldOrder != targetOrder)
            {
                var tasksInColumn = await _context.Tasks
                    .Where(t => t.ProjectId == task.ProjectId && t.Status == oldStatus && t.Id != task.Id)
                    .OrderBy(t => t.Order)
                    .ToListAsync(cancellationToken);

                if (targetOrder > oldOrder)
                {
                    foreach (var item in tasksInColumn.Where(t => t.Order > oldOrder && t.Order <= targetOrder))
                    {
                        item.Order--;
                    }
                }
                else
                {
                    foreach (var item in tasksInColumn.Where(t => t.Order >= targetOrder && t.Order < oldOrder))
                    {
                        item.Order++;
                    }
                }
                task.Order = targetOrder;
            }
        }
        else
        {
            // Mover a otra columna: decrementar los de la columna origen
            var tasksInOldColumn = await _context.Tasks
                .Where(t => t.ProjectId == task.ProjectId && t.Status == oldStatus && t.Order > oldOrder)
                .ToListAsync(cancellationToken);

            foreach (var item in tasksInOldColumn)
            {
                item.Order--;
            }

            // Incrementar los de la columna destino a partir de la posición target
            var tasksInNewColumn = await _context.Tasks
                .Where(t => t.ProjectId == task.ProjectId && t.Status == newStatus && t.Order >= targetOrder)
                .ToListAsync(cancellationToken);

            foreach (var item in tasksInNewColumn)
            {
                item.Order++;
            }

            task.Status = newStatus;
            task.Order = targetOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}