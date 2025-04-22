using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Services.Tenant.Interfaces;
namespace TicketSolver.Api.Controllers;

public class TenantsController(ITenantsService service) : ControllerBase
{
    private readonly ITenantsService _service = service;

    [HttpGet]
    [Route("api/tenants")]
    public async Task<IActionResult> GetTenants()
    {   
        var tenants = await _service.GetAllAsync(HttpContext.RequestAborted);
        return !tenants.Any() ? Ok(ApiResponse.Ok(tenants)) : NotFound();
    }

    [HttpPost]
    [Route("api/tenants")]
    public async Task<IActionResult> CreateTenant([FromBody] Tenants tenant)
    {

        return Ok(ApiResponse.Ok(await _service.AddTenantAsync(tenant as Tenants, HttpContext.RequestAborted) ?? throw new InvalidOperationException()));
    }
    
    [HttpGet]
    [Route("api/tenants/{key}")]
    public async Task<IActionResult> GetTenantByKey(string key)
    {
        var tenant = await _service.GetTenantByKeyAsync(key, HttpContext.RequestAborted);
        return tenant is not null ? Ok(ApiResponse.Ok(tenant)) : NotFound();
    }
}