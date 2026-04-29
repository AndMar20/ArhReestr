using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace WebApp.Identity;

/// <summary>
/// Провайдер состояния аутентификации, который периодически проверяет валидность пользовательской сессии.
/// </summary>
public sealed class RevalidatingIdentityAuthenticationStateProvider<TUser>
    : RevalidatingServerAuthenticationStateProvider where TUser : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IdentityOptions _options;

    public RevalidatingIdentityAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> optionsAccessor)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _options = optionsAccessor.Value;
    }

    protected override TimeSpan RevalidationInterval { get; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Проверяет, может ли пользователь продолжать работу в рамках текущей куки/токена.
    /// </summary>
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var principal = authenticationState.User;
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return false;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<TUser>>();

        var userId = principal.FindFirstValue(_options.ClaimsIdentity.UserIdClaimType);
        if (userId is null)
        {
            return false;
        }

        var storedUser = await userManager.FindByIdAsync(userId);
        if (storedUser is null)
        {
            return false;
        }

        return await signInManager.CanSignInAsync(storedUser);
    }
}
