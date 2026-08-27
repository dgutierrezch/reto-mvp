using EventService.Domain.Exceptions;
using FluentValidation;

namespace EventService.Api.Middleware;

/// <summary>
/// Traduce excepciones a respuestas HTTP consistentes sin filtrar stack traces
/// ni detalles internos (requisito de manejo de errores seguro).
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Error de validación",
                errors = ex.Errors.Select(e => e.ErrorMessage)
            });
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await context.Response.WriteAsJsonAsync(new { title = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { title = "Ocurrió un error interno. Intenta nuevamente." });
        }
    }
}
