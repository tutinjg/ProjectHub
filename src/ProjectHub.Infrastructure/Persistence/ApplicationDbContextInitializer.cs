using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlite())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al aplicar las migraciones a la base de datos.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error durante el sembrado inicial de datos.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Empresa por defecto (Default Tenant)
        var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == defaultCompanyId);

        if (company == null)
        {
            company = new Company("ProjectHub Default Organization")
            {
                Id = defaultCompanyId,
                Information = "Tenant raíz del sistema",
                IsActive = true
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync(default);
            _logger.LogInformation("Organización raíz creada con éxito.");
        }

        // 2. Roles del Sistema
        var adminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var memberRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        if (!await _context.Roles.IgnoreQueryFilters().AnyAsync(r => r.Id == adminRoleId))
        {
            _context.Roles.Add(new Role
            {
                Id = adminRoleId,
                Name = "Administrator",
                Description = "Control total de la organización y sus miembros",
                CompanyId = defaultCompanyId,
                IsSystemRole = true
            });
        }

        if (!await _context.Roles.IgnoreQueryFilters().AnyAsync(r => r.Id == memberRoleId))
        {
            _context.Roles.Add(new Role
            {
                Id = memberRoleId,
                Name = "Member",
                Description = "Acceso a proyectos y tareas asignadas",
                CompanyId = defaultCompanyId,
                IsSystemRole = true
            });
        }

        await _context.SaveChangesAsync(default);

        // 3. Usuario Administrador Inicial
        var defaultUserEmail = "admin@projecthub.com";
        var userExists = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == defaultUserEmail);

        if (!userExists)
        {
            var adminUser = new User(
                "Admin",
                "ProjectHub",
                defaultUserEmail,
                _passwordHasher.HashPassword("Admin2026!*"),
                defaultCompanyId,
                adminRoleId
            );

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync(default);
            _logger.LogInformation("Usuario administrador inicial sembrado: {Email}", defaultUserEmail);
        }
    }
}