using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

// Domain
using Backend.Domain.Shared;

// Infrastructure (DI + DbContext + Repos + Services + MediatR)
using Backend.Infrastructure;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Services;
using Backend.Domain.Users.Repositories;
using Backend.Caching;
using Backend.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Backend (DDD) API",
        Version = "v1",
        Description = "Internet Hardware Store — DDD + CQRS + MediatR"
    });

    // JWT Authorize
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Введите токен (без слова 'Bearer').",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    throw new InvalidOperationException(
        "Строка подключения ConnectionStrings:DefaultConnection не задана. " +
        "Задайте её через переменную окружения ConnectionStrings__DefaultConnection или user-secrets.");
}

// Секрет НЕ имеет значения по умолчанию: без него приложение не должно стартовать,
// иначе токены можно было бы подделать, зная ключ из исходников.
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SecretKey не задан или короче 32 символов. " +
        "Задайте его через переменную окружения Jwt__SecretKey или user-secrets " +
        "(пример: openssl rand -base64 48).");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "InternetHardwareStore";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "InternetHardwareStoreClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        // Токен должен соответствовать текущей метке безопасности пользователя:
        // после смены пароля/роли или удаления пользователя старые токены отклоняются.
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            var userIdClaim = principal?.FindFirst("userId")?.Value
                              ?? principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var stamp = principal?.FindFirst(JwtService.SecurityStampClaim)?.Value;

            var validator = context.HttpContext.RequestServices.GetRequiredService<IUserSessionValidator>();

            if (!int.TryParse(userIdClaim, out var userId) ||
                !await validator.IsValidAsync(userId, stamp, context.HttpContext.RequestAborted))
            {
                context.Fail("Сессия недействительна: пароль или роль изменены, либо пользователь удалён");
            }
        },
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[]
    {
        "http://localhost:3000",
        "http://127.0.0.1:3000"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromDays(1));
    });
});

// Ограничение частоты запросов к /api/Auth/* (защита от подбора паролей).
// Политика "auth" подключается атрибутом [EnableRateLimiting("auth")] на AuthController.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Общий лимит на IP для всего API — защита от перебора и простого DoS
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        httpContext.Request.Path.StartsWithSegments("/health")
            ? RateLimitPartition.GetNoLimiter("health")
            : RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = builder.Configuration.GetValue("RateLimiting:GlobalPermitLimit", 600),
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 10),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Кеш публичных списков каталога (сбрасывается при изменениях, см. EvictCatalogCacheAttribute)
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(CatalogCache.PolicyName, new CatalogCachePolicy());
});

// /health/live — процесс жив; /health/ready — есть соединение с БД (для оркестратора/балансировщика)
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Применение миграций...");
        await db.Database.MigrateAsync();

        // Тестовые данные каталога — только в Development или если явно включено (Seed:Enabled=true)
        if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Seed:Enabled"))
        {
            logger.LogInformation("Заполнение БД тестовыми данными...");
            DbInitializer.Seed(db);
        }

        // Первый администратор создаётся из конфигурации (Admin:UserName / Admin:Password)
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await AdminSeeder.EnsureAdminAsync(
            userRepository, passwordHasher, app.Configuration, logger);

        // Fail-fast: проверяем ключ шифрования при старте, а не при первом запросе
        scope.ServiceProvider.GetRequiredService<IEncryptionService>();

        logger.LogInformation("Миграции и инициализация завершены.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при миграции/seed БД");
        throw;
    }
}

app.UseMiddleware<Backend.Middleware.SecurityHeadersMiddleware>();
app.UseMiddleware<Backend.Middleware.DomainExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Изображения товаров: браузер может кешировать их на неделю
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers.CacheControl = "public,max-age=604800"
});

app.UseCors("AllowFrontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.MapGet("/api/test", () => Results.Ok(new
{
    message = "Backend (DDD) работает!",
    time = DateTime.Now,
    architecture = "Domain-Driven Design + CQRS + MediatR"
}));

try
{
    Log.Information("Backend (DDD) запущен на http://localhost:5214");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение неожиданно завершилось");
    throw;
}
finally
{
    Log.CloseAndFlush();
}