using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectHub.Application.Common.Interfaces;

namespace ProjectHub.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    // Mock temporal para pruebas locales
    public string? UserId => "system-admin";
    public string? Email => "admin@projecthub.local";
    public bool IsAuthenticated => true;
}