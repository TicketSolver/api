using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Api.Controllers;

public class TenantsController(ITenantsService tenantsService) : ControllerBase
{
    [HttpGet]
    [Route("api/tenants")]
    public async Task<IActionResult> GetTenants()
    {
        var tenants = await tenantsService.GetAllAsync(HttpContext.RequestAborted);
        if (tenants.Count() != 0)
            return Ok(ApiResponse.Ok(tenants));
        return NotFound(ApiResponse.Fail("Nenhum Tenant Encontrado!"));
    }

    [HttpPost]
    [Route("api/tenants")]
    public async Task<IActionResult> CreateTenant([FromBody] Tenants tenant)
    {
        return Ok(ApiResponse.Ok(await tenantsService.AddTenantAsync(tenant, HttpContext.RequestAborted) ??
                                 throw new InvalidOperationException()));
    }

    [HttpGet]
    [Route("api/tenants/{key}")]
    public async Task<IActionResult> GetTenantByKey(Guid key)
    {
        var tenant = await tenantsService.GetTenantByKeyAsync(key, HttpContext.RequestAborted);
        return tenant is not null
            ? Ok(ApiResponse.Ok(tenant))
            : NotFound(ApiResponse.Fail("Nenhum Tenant Encontrado!"));
    }
}