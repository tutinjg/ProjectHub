using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Common.Interfaces;
using ProjectHub.Domain.Common;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService,
        ICurrentUserService currentUserService) : base(options)
    {
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones IEntityTypeConfiguration del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Relación User -> Role
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1. Filtro global para Soft Delete
        modelBuilder.ApplyGlobalFilters<ISoftDelete>(e => !e.IsDeleted);

        // 2. Filtro global multi-tenant usando CompanyId
        modelBuilder.ApplyGlobalFilters<IMustHaveTenant>(e => e.CompanyId == _tenantService.CompanyId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentCompanyId = _tenantService.CompanyId; 
        var currentUserId = _currentUserService.UserId ?? "Anonymous";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IMustHaveTenant tenantEntity && entry.State == EntityState.Added)
            {
                if (tenantEntity.CompanyId == Guid.Empty && currentCompanyId.HasValue)
                {
                    tenantEntity.CompanyId = currentCompanyId.Value;
                }
            }

            // Manejo de Auditoría
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedAt = DateTime.UtcNow;
                        auditableEntity.CreatedBy = currentUserId;
                        break;

                    case EntityState.Modified:
                        auditableEntity.LastModifiedAt = DateTime.UtcNow;
                        auditableEntity.LastModifiedBy = currentUserId;
                        break;
                }
            }

            // Manejo de Soft Delete
            if (entry.Entity is ISoftDelete softDeleteEntity && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = DateTime.UtcNow;
                softDeleteEntity.DeletedBy = currentUserId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}