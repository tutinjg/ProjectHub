using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Domain.Common;

public interface IMustHaveTenant
{
    Guid CompanyId { get; set; }
}