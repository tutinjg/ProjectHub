using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Common;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Domain.Entities;

public class TaskItem : BaseEntity, IMustHaveTenant
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public int Order { get; set; }

    // Relaciones
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public TaskItem() { }

    public TaskItem(string title, Guid projectId, Guid companyId, TaskPriority priority = TaskPriority.Medium)
    {
        Title = title;
        ProjectId = projectId;
        CompanyId = companyId;
        Status = TaskItemStatus.Todo;
        Priority = priority;
    }
}