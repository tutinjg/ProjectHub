using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.Companies.Commands.CreateCompany;

namespace ProjectHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ISender _sender;

    public CompaniesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCompanyCommand command)
    {
        var companyId = await _sender.Send(command);
        return Ok(companyId);
    }
}