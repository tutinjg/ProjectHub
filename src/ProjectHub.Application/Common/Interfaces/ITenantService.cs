using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Application.Common.Interfaces;

public interface ITenantService
{
    Guid? CompanyId { get; }
    void SetTenant(Guid companyId);
}