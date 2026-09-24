using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Application.Tasks.Common;

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email
);

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    int Order,
    Guid ProjectId,
    UserSummaryDto? AssignedTo,
    string? CreatedBy,
    DateTime CreatedAt
);