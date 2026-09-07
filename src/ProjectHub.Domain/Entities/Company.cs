using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Common;

namespace ProjectHub.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Information { get; set; }

    // Propiedad para desactivar el tenant completo
    public bool IsActive { get; set; } = true;

    // Propiedades de navegación
    public ICollection<User> Users { get; private set; } = new List<User>();

    // Constructor vacío para EF
    protected Company() { }

    public Company(string name)
    {
        Name = name;
    }
}