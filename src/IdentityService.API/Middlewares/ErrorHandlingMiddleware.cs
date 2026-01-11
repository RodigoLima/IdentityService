using System.Net;
using System.Text.Json;

namespace IdentityService.API.Middlewares;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument exception: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = JsonSerializer.Serialize(new
            {
                error = "Erro de argumento.",
                details = ex.Message
            });

            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = _environment.IsDevelopment()
                ? JsonSerializer.Serialize(new
                {
                    error = "Ocorreu um erro inesperado.",
                    details = ex.Message
                })
                : JsonSerializer.Serialize(new
                {
                    error = "Ocorreu um erro inesperado.",
                    details = ex.Message
                });

            await context.Response.WriteAsync(result);
        }
    }
}
