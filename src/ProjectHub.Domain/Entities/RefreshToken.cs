using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Common;

namespace ProjectHub.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Usar DateTime.UtcNow
    public bool IsExpired => DateTime.UtcNow >= (ExpiresAt.Kind == DateTimeKind.Utc ? ExpiresAt : DateTime.SpecifyKind(ExpiresAt, DateTimeKind.Utc));
    public bool IsActive => !IsRevoked && !IsExpired;
}