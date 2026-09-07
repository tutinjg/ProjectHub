using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}