using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Application.Auth.Common;

public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string RoleName,
    Guid CompanyId
);