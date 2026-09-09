using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectHub.Application.Common.Interfaces;

namespace ProjectHub.Infrastructure.Services;

public class TenantService : ITenantService
{
    // Id temporal para desarrollo inicial
    public Guid? CompanyId { get; private set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public void SetTenant(Guid companyId)
    {
        CompanyId = companyId;
    }
}