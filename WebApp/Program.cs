using System.Data.Common;
using DataLayer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Components;
using WebApp.Infrastructure;
using WebApp.Identity;
using WebApp.Services;
using WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Antiforgery;

// Точка входа Blazor Server: настраиваем DI, аутентификацию и минимальные API.
var builder = WebApplication.CreateBuilder(args);

// Получаем строку подключения к MySQL из конфигурации; если её нет, используем InMemory, чтобы приложение могло стартовать без БД.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemory = string.IsNullOrWhiteSpace(connectionString);

// Регистрируем контекст EF Core с провайдером MySQL или InMemory.
// Используем фабрику, чтобы каждый запрос получал свой экземпляр контекста.
builder.Services.AddDbContextFactory<ArhReestrContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("ArhReestrFallback");
    }
    else
    {
        options.UseMySQL(connectionString!);
    }
});
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<ArhReestrContext>>().CreateDbContext());

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddScoped<IUserStore<ApplicationUser>, ArhUserStore>();
builder.Services.AddScoped<IRoleStore<ApplicationRole>, ArhRoleStore>();

// Настраиваем Identity с минимальными требованиями к паролю и уникальностью почты.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddErrorDescriber<RussianIdentityErrorDescriber>()
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
});

authenticationBuilder.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
    options.SlidingExpiration = true;
});

// Политики авторизации: роль агента и администратора.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAgent", policy => policy.RequireRole("agent", "admin"))
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

builder.Services.AddScoped<RealEstateService>();
builder.Services.AddScoped<InteractionService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<AdminUserService>();



builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// При наличии реальной строки подключения проверяем доступность БД, но не падаем при использовании InMemory.
if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<ArhReestrContext>();

    try
    {
        if (!db.Database.CanConnect())
        {
            throw new InvalidOperationException(DatabaseErrorMessages.ConnectionFailed);
        }
    }
    catch (DbException ex)
    {
        var message = DatabaseErrorMessages.Resolve(ex);
        logger.LogError(ex, message);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, DatabaseErrorMessages.UnexpectedError);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (AntiforgeryValidationException ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Antiforgery");
        logger.LogWarning(ex, "Антифрод токен не прошёл проверку для {Path}", context.Request.Path);

        if (!context.Response.HasStarted)
        {
            var returnUrl = context.Request.Path.HasValue
                ? $"{context.Request.Path}{context.Request.QueryString}"
                : "/";

            var redirectUrl = QueryHelpers.AddQueryString("/login", new Dictionary<string, string?>
            {
                ["error"] = "antiforgery",
                ["returnUrl"] = returnUrl
            });

            context.Response.Redirect(redirectUrl);
        }
    }
});
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/login", async ([FromForm] LoginInputModel request,
        [FromQuery] string? returnUrl,
        SignInManager<ApplicationUser> signInManager) =>
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            var redirectUrl = QueryHelpers.AddQueryString("/login", new Dictionary<string, string?>
            {
                ["error"] = "missing",
                ["returnUrl"] = returnUrl
            });

            return Results.Redirect(redirectUrl);
        }

        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        var loginFailedUrl = QueryHelpers.AddQueryString("/login", new Dictionary<string, string?>
        {
            ["error"] = "invalid",
            ["returnUrl"] = returnUrl
        });

        return Results.Redirect(loginFailedUrl);
    })
    .AllowAnonymous();

// Разлогиниваем пользователя через отдельный HTTP-запрос, чтобы избежать подвисаний в SignalR-соединении Blazor.
app.MapPost("/logout", async ([FromQuery] string? returnUrl,
        SignInManager<ApplicationUser> signInManager) =>
    {
        await signInManager.SignOutAsync();

        var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        return Results.Redirect(redirectUrl);
    })
    .RequireAuthorization();

app.MapGet("/reports/admin.xlsx", async (ReportService service, TimeProvider timeProvider, CancellationToken token) =>
    {
        var bytes = await service.BuildExcelAsync(token);
        return Results.File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"arhreestr-report-{timeProvider.GetUtcNow():yyyyMMddHHmmss}.xlsx");
    })
    .RequireAuthorization("RequireAdmin");

app.Run();
