using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;

namespace Backend.Caching;

/// <summary>
/// Кеширование публичных справочников и списков каталога (бренды, категории, типы, товары).
///
/// Стандартная политика OutputCache не кеширует запросы с заголовком Authorization,
/// а фронтенд отправляет токен всегда, когда пользователь вошёл. Ответы каталога
/// от пользователя не зависят, поэтому здесь кеширование разрешено и для них.
/// Админские изменения сбрасывают кеш атрибутом <see cref="EvictCatalogCacheAttribute"/>.
/// </summary>
public static class CatalogCache
{
    public const string PolicyName = "catalog";
    public const string Tag = "catalog";
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Запрещает сохранять текущий ответ в кеш. Нужен, когда ответ зависит от роли
    /// (например, снятый с продажи товар виден только администратору).
    /// </summary>
    public static void SkipStorage(HttpContext httpContext)
    {
        var feature = httpContext.Features.Get<IOutputCacheFeature>();
        if (feature != null)
            feature.Context.AllowCacheStorage = false;
    }
}

public sealed class CatalogCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var method = context.HttpContext.Request.Method;
        var cacheable = HttpMethods.IsGet(method) || HttpMethods.IsHead(method);

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = cacheable;
        context.AllowCacheStorage = cacheable;
        // Без «склеивания» одинаковых запросов. При AllowLocking = true параллельные запросы
        // к одному адресу ждали результат первого. Если первый запрос отменялся клиентом
        // (переход на другую страницу, AbortController, двойной монтаж React.StrictMode),
        // его OperationCanceledException получали все ожидающие — и отвечали 500.
        context.AllowLocking = false;
        context.CacheVaryByRules.QueryKeys = "*";
        context.ResponseExpirationTimeSpan = CatalogCache.Duration;
        context.Tags.Add(CatalogCache.Tag);

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;

        // Кешируем только успешные ответы без cookie
        if (response.StatusCode != StatusCodes.Status200OK ||
            !StringValues.IsNullOrEmpty(response.Headers.SetCookie))
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// После успешного выполнения действия сбрасывает кеш каталога.
/// Ставится на изменяющие каталог/остатки эндпоинты.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class EvictCatalogCacheAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (executed.Exception != null && !executed.ExceptionHandled)
            return;

        var store = context.HttpContext.RequestServices.GetService<IOutputCacheStore>();
        if (store != null)
            await store.EvictByTagAsync(CatalogCache.Tag, CancellationToken.None);
    }
}