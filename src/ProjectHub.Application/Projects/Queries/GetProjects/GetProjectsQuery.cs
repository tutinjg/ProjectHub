using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Application.Projects.Common;

namespace ProjectHub.Application.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<List<ProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Status,
                p.StartDate,
                p.TargetEndDate,
                p.Tasks.Count(t => !t.IsDeleted),
                p.CreatedBy,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}