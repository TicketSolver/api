using MobileSolver.Api.Models;
using TicketSolver.Api.Exceptions;

namespace MobileSolver.Api.Middlewares;

public class HttpExceptionHandler(RequestDelegate next, ILogger<HttpExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (HttpException ex)
        {
            context.Response.StatusCode = (int)ex.HttpStatusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(ex.Message));
        }
    }
}