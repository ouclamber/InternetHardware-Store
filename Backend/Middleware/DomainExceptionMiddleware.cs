using Backend.Domain.Shared;

namespace Backend.Middleware;

/// <summary>
/// Единая обработка исключений → JSON-ответ { error, type, status }.
///
///   NotFoundException      → 404
///   ForbiddenException     → 403
///   ConflictException      → 409 (конкурентное изменение, нарушение уникальности)
///   DomainException        → 400 (нарушение бизнес-правила)
///   прочие                 → 500 (детали — только в Development)
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public DomainExceptionMiddleware(
        RequestDelegate next,
        ILogger<DomainExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Клиент закрыл соединение — это не ошибка сервера
            _logger.LogDebug("Запрос {Path} отменён клиентом", context.Request.Path);
        }
        catch (DomainException ex)
        {
            var (status, type) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "AccessDenied"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status400BadRequest, "DomainError")
            };

            _logger.LogWarning("Domain error {Type} ({Status}) on {Path}: {Message}",
                type, status, context.Request.Path, ex.Message);

            await WriteAsync(context, status, new { error = ex.Message, type, status });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied: {Message}", ex.Message);

            await WriteAsync(context, StatusCodes.Status403Forbidden,
                new { error = ex.Message, type = "AccessDenied", status = StatusCodes.Status403Forbidden });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var response = _env.IsDevelopment()
                ? new
                {
                    error = ex.Message,
                    type = "InternalError",
                    status = StatusCodes.Status500InternalServerError,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                }
                : (object)new
                {
                    error = "Внутренняя ошибка сервера",
                    type = "InternalError",
                    status = StatusCodes.Status500InternalServerError
                };

            await WriteAsync(context, StatusCodes.Status500InternalServerError, response);
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, object body)
    {
        // Если ответ уже начал отправляться, заголовки менять нельзя
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(body);
    }
}
