using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Application.Projects.Common;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? TargetEndDate,
    int TaskCount,
    string? CreatedBy,
    DateTime CreatedAt
);