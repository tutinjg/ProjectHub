using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Domain.Common;

public abstract class BaseEntity : IAuditableEntity, ISoftDelete
{
    // PK universal para todas las entidades
    public Guid Id { get; set; } = Guid.NewGuid();

    // Implementación de IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Implementación de ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}