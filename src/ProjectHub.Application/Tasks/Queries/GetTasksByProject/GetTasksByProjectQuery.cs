using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Common.Exceptions;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Application.Tasks.Common;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Tasks.Queries.GetTasksByProject;

public record GetTasksByProjectQuery(Guid ProjectId) : IRequest<List<TaskDto>>;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<TaskDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTasksByProjectQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == request.ProjectId)
            .Include(t => t.AssignedToUser)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.Order)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.Order,
                t.ProjectId,
                t.AssignedToUser != null
                    ? new UserSummaryDto(t.AssignedToUser.Id, t.AssignedToUser.GetFullName(), t.AssignedToUser.Email)
                    : null,
                t.CreatedBy,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}