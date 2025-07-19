using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileSolver.Api.Models;
using TicketSolver.Application.Services.admin.Interfaces;

namespace MobileSolver.Api.Controllers;

[Authorize(Roles = "1")]
[ApiController]
public class AdminController(IAdminStatsService svc) : ShellController
{
    
    [HttpGet("overview/{tenantid:int}/stats")]
    public async Task<IActionResult> GetOverviewStats(
        [FromRoute] int tenantid, CancellationToken ct)
    {
        var dto = await svc.GetOverviewStatsAsync(
            tenantid,ct);
        if (dto is null)
            return NotFound(ApiResponse.Fail("Nenhum dado encontrado para o Tenant especificado."));
        return Ok(ApiResponse.Ok(dto));
    }

    [HttpGet("/api/tenants/{tenantid:int}/tickets/recent")]
    public async Task<IActionResult> GetRecentTickets(
        [FromRoute] int tenantid,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken ct = default)
    {
        var pageData = await svc
            .GetRecentTicketsAsync(
                tenantid, page, pageSize, ct);
        return Ok(ApiResponse.Ok(pageData));
    }
    
    [HttpGet("api/tenants/{tenantid:int}/tickets/")]
    public async Task<IActionResult> GetTickets(
        [FromRoute] int tenantid,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var pageData = await svc
            .GetTicketsAsync(
                tenantid, page, pageSize, ct);
        if (pageData is null || pageData.Items.Count == 0)
            return NotFound(ApiResponse.Fail("Nenhum dado encontrado para o Tenant especificado."));
        return Ok(ApiResponse.Ok(pageData));
    }
}