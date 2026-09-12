using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Common;
using ProjectHub.Domain.Enums;

namespace ProjectHub.Domain.Entities;

public class User : BaseEntity, IMustHaveTenant
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Se guarda el hash en texto plano
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    // Relación Multi-Tenant (FK)
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Constructor vacío para EF
    protected User() { }
    public User(string firstName, string lastName, string email, string passwordHash, Guid companyId, Guid roleId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        CompanyId = companyId;
        RoleId = roleId;
        IsActive = true;
    }

    // Refresh tokens para la sesion activa
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();


    // Comportamientos de Dominio (Rich Domain Model)
    public string GetFullName() => $"{FirstName} {LastName}";

    public void Deactivate() => IsActive = false;
}