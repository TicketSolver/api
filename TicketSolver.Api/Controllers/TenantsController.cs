using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Api.Controllers;

public class TenantsController(ITenantsService service) : ControllerBase
{
    [HttpGet]
    [Route("api/tenants")]
    public async Task<IActionResult> GetTenants()
    {   
        var tenants = await service.GetAllAsync(HttpContext.RequestAborted);
        if (tenants.Count() != 0)
            return Ok(ApiResponse.Ok(tenants));
        return NotFound(ApiResponse.Fail("Nenhum Tenant Encontrado!"));
        
    }

    [HttpPost]
    [Route("api/tenants")]
    public async Task<IActionResult> CreateTenant([FromBody] Tenants tenant)
    {
        return Ok(ApiResponse.Ok(await service.AddTenantAsync(tenant as Tenants, HttpContext.RequestAborted) ?? throw new InvalidOperationException()));
    }
    
    [HttpGet]
    [Route("api/tenants/{key}")]
    public async Task<IActionResult> GetTenantByKey(Guid key)
    {
        var tenant = await service.GetTenantByKeyAsync(key, HttpContext.RequestAborted);
        return tenant is not null ? Ok(ApiResponse.Ok(tenant)) : NotFound(ApiResponse.Fail("Nenhum Tenant Encontrado!"));
    }
}