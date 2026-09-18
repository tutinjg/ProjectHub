using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Common;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Domain.Entities;

public class Project : BaseEntity, IMustHaveTenant
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public DateTime? StartDate { get; set; }
    public DateTime? TargetEndDate { get; set; }

    // Multi-tenant
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Colección de tareas
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public Project() { }

    public Project(string name, string? description, Guid companyId, DateTime? startDate = null, DateTime? targetEndDate = null)
    {
        Name = name;
        Description = description;
        CompanyId = companyId;
        Status = ProjectStatus.Planning;
        StartDate = startDate;
        TargetEndDate = targetEndDate;
    }
}