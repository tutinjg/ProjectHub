using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ProjectHub.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CompanyId
    {
        get
        {
            if (_tenantId.HasValue) return _tenantId;

            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst("companyId")?.Value;
            if (Guid.TryParse(claimValue, out var parsedGuid))
            {
                _tenantId = parsedGuid;
            }

            return _tenantId;
        }
    }

    public void SetTenant(Guid companyId)
    {
        _tenantId = companyId;
    }
}