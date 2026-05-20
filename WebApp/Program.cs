using System.Data;
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

// РўРѕС‡РєР° РІС…РѕРґР° Blazor Server: РЅР°СЃС‚СЂР°РёРІР°РµРј DI, Р°СѓС‚РµРЅС‚РёС„РёРєР°С†РёСЋ Рё РјРёРЅРёРјР°Р»СЊРЅС‹Рµ API.
var crashLogPath = Path.Combine(AppContext.BaseDirectory, "webapp-crash.log");
void WriteCrashLog(string source, Exception exception)
{
    try
    {
        var message = $"{DateTimeOffset.Now:O} [{source}]{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
        File.AppendAllText(crashLogPath, message);
    }
    catch
    {
        // Best-effort crash logging only.
    }
}

AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    if (args.ExceptionObject is Exception exception)
    {
        WriteCrashLog("UnhandledException", exception);
    }
};

TaskScheduler.UnobservedTaskException += (_, args) =>
{
    WriteCrashLog("UnobservedTaskException", args.Exception);
    args.SetObserved();
};

var builder = WebApplication.CreateBuilder(args);

// РџРѕР»СѓС‡Р°РµРј СЃС‚СЂРѕРєСѓ РїРѕРґРєР»СЋС‡РµРЅРёСЏ Рє MySQL РёР· РєРѕРЅС„РёРіСѓСЂР°С†РёРё; РµСЃР»Рё РµС‘ РЅРµС‚, РёСЃРїРѕР»СЊР·СѓРµРј InMemory, С‡С‚РѕР±С‹ РїСЂРёР»РѕР¶РµРЅРёРµ РјРѕРіР»Рѕ СЃС‚Р°СЂС‚РѕРІР°С‚СЊ Р±РµР· Р‘Р”.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemory = string.IsNullOrWhiteSpace(connectionString);

// Р РµРіРёСЃС‚СЂРёСЂСѓРµРј РєРѕРЅС‚РµРєСЃС‚ EF Core СЃ РїСЂРѕРІР°Р№РґРµСЂРѕРј MySQL РёР»Рё InMemory.
// РСЃРїРѕР»СЊР·СѓРµРј С„Р°Р±СЂРёРєСѓ, С‡С‚РѕР±С‹ РєР°Р¶РґС‹Р№ Р·Р°РїСЂРѕСЃ РїРѕР»СѓС‡Р°Р» СЃРІРѕР№ СЌРєР·РµРјРїР»СЏСЂ РєРѕРЅС‚РµРєСЃС‚Р°.
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
builder.Services.AddSingleton<DatabaseHealthState>();
builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddScoped<IUserStore<ApplicationUser>, ArhUserStore>();
builder.Services.AddScoped<IRoleStore<ApplicationRole>, ArhRoleStore>();

// РќР°СЃС‚СЂР°РёРІР°РµРј Identity СЃ РјРёРЅРёРјР°Р»СЊРЅС‹РјРё С‚СЂРµР±РѕРІР°РЅРёСЏРјРё Рє РїР°СЂРѕР»СЋ Рё СѓРЅРёРєР°Р»СЊРЅРѕСЃС‚СЊСЋ РїРѕС‡С‚С‹.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = true;
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

// РџРѕР»РёС‚РёРєРё Р°РІС‚РѕСЂРёР·Р°С†РёРё: СЂРѕР»СЊ Р°РіРµРЅС‚Р° Рё Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР°.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAgent", policy => policy.RequireRole("agent", "admin"))
    .AddPolicy("RequireClient", policy => policy.RequireRole("client", "admin"))
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

builder.Services.AddScoped<RealEstateService>();
builder.Services.AddScoped<InteractionService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<AdminReferenceService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<ViewingCalendarService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AddressSuggestionService>();
builder.Services.AddHttpClient<GeocodingService>();



builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    });

var app = builder.Build();

// РџСЂРё РЅР°Р»РёС‡РёРё СЂРµР°Р»СЊРЅРѕР№ СЃС‚СЂРѕРєРё РїРѕРґРєР»СЋС‡РµРЅРёСЏ РїСЂРѕРІРµСЂСЏРµРј РґРѕСЃС‚СѓРїРЅРѕСЃС‚СЊ Р‘Р”, РЅРѕ РЅРµ РїР°РґР°РµРј РїСЂРё РёСЃРїРѕР»СЊР·РѕРІР°РЅРёРё InMemory.
if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<ArhReestrContext>();
    var dbHealthState = scope.ServiceProvider.GetRequiredService<DatabaseHealthState>();

    try
    {
        const int maxAttempts = 12;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (db.Database.CanConnect())
            {
                await EnsureNotificationLinkColumnAsync(db);
                dbHealthState.MarkAvailable();
                break;
            }

            if (attempt == maxAttempts)
            {
                throw new InvalidOperationException(DatabaseErrorMessages.ConnectionFailed);
            }

            logger.LogWarning("Р‘Р°Р·Р° РґР°РЅРЅС‹С… РїРѕРєР° РЅРµРґРѕСЃС‚СѓРїРЅР°, РїРѕРІС‚РѕСЂРЅР°СЏ РїСЂРѕРІРµСЂРєР° {Attempt}/{MaxAttempts}", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
    catch (DbException ex)
    {
        var message = DatabaseErrorMessages.Resolve(ex);
        logger.LogError(ex, message);
        dbHealthState.MarkUnavailable(message);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, ex.Message);
        dbHealthState.MarkUnavailable(ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, DatabaseErrorMessages.UnexpectedError);
        dbHealthState.MarkUnavailable(DatabaseErrorMessages.UnexpectedError);
    }
}

static async Task EnsureNotificationLinkColumnAsync(ArhReestrContext db)
{
    if (db.Database.IsInMemory())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;

    if (shouldClose)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'Notifications'
              AND COLUMN_NAME = 'linkUrl';
            """;

        var existingColumns = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
        if (existingColumns > 0)
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `Notifications` ADD COLUMN `linkUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL AFTER `message`;");
    }
    catch
    {
        // Optional compatibility column; real DB problems are handled by the startup health check.
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
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
        logger.LogWarning(ex, "РђРЅС‚РёС„СЂРѕРґ С‚РѕРєРµРЅ РЅРµ РїСЂРѕС€С‘Р» РїСЂРѕРІРµСЂРєСѓ РґР»СЏ {Path}", context.Request.Path);

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

// Р Р°Р·Р»РѕРіРёРЅРёРІР°РµРј РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ С‡РµСЂРµР· РѕС‚РґРµР»СЊРЅС‹Р№ HTTP-Р·Р°РїСЂРѕСЃ, С‡С‚РѕР±С‹ РёР·Р±РµР¶Р°С‚СЊ РїРѕРґРІРёСЃР°РЅРёР№ РІ SignalR-СЃРѕРµРґРёРЅРµРЅРёРё Blazor.
app.MapPost("/logout", async ([FromQuery] string? returnUrl,
        SignInManager<ApplicationUser> signInManager) =>
    {
        await signInManager.SignOutAsync();

        var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        return Results.Redirect(redirectUrl);
    })
    .RequireAuthorization();

app.MapGet("/reports/admin.xlsx", async ([FromQuery] string? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to, ReportService service, TimeProvider timeProvider, CancellationToken token) =>
    {
        var bytes = await service.BuildExcelAsync(type, from, to, token);
        return Results.File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"arhreestr-{(string.IsNullOrWhiteSpace(type) ? "full" : type)}-{timeProvider.GetUtcNow():yyyyMMddHHmmss}.xlsx");
    })
    .RequireAuthorization("RequireAdmin");

app.Run();

