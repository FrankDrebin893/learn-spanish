using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Supabase;
using Supabase.Gotrue;

namespace SpanishWordLearner.Services;

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly IJSRuntime? _js;
    private readonly ILogger<AuthService>? _logger;
    private Supabase.Client? _client;
    private bool _isInitialized;

    public event Action? AuthStateChanged;

    public bool IsConfigured { get; private set; }
    public bool IsAuthenticated => _client?.Auth.CurrentUser != null;
    public string? CurrentUserId => _client?.Auth.CurrentUser?.Id;
    public string? CurrentUserEmail => _client?.Auth.CurrentUser?.Email;
    public Supabase.Client? Client => _client;

    public AuthService(IConfiguration configuration, IJSRuntime? js = null, ILogger<AuthService>? logger = null)
    {
        _configuration = configuration;
        _js = js;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        var url = _configuration["Supabase:Url"];
        var key = _configuration["Supabase:AnonKey"];

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            _logger?.LogInformation("Supabase is not configured. Running in offline/guest mode.");
            IsConfigured = false;
            return;
        }

        try
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false,
                SessionHandler = new BlazorSessionPersistence(_js)
            };

            _client = new Supabase.Client(url, key, options);
            await _client.InitializeAsync();

            _client.Auth.AddStateChangedListener((sender, state) =>
            {
                _logger?.LogInformation("Supabase auth state changed: {State}", state);
                AuthStateChanged?.Invoke();
            });

            IsConfigured = true;
            _logger?.LogInformation("Supabase initialized successfully. Authenticated: {IsAuth}", IsAuthenticated);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize Supabase client. Running in guest mode.");
            IsConfigured = false;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> SignInWithPasswordAsync(string email, string password)
    {
        if (!IsConfigured || _client == null)
            return (false, "El servicio de autenticación no está configurado.");

        try
        {
            var session = await _client.Auth.SignInWithPassword(email.Trim(), password);
            if (session != null && session.User != null)
            {
                AuthStateChanged?.Invoke();
                return (true, null);
            }
            return (false, "No se pudo iniciar sesión. Verifica tus credenciales.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error signing in with password");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> SignUpWithPasswordAsync(string email, string password)
    {
        if (!IsConfigured || _client == null)
            return (false, "El servicio de autenticación no está configurado.");

        try
        {
            var session = await _client.Auth.SignUp(email.Trim(), password);
            if (session != null)
            {
                AuthStateChanged?.Invoke();
                return (true, null);
            }
            return (false, "No se pudo registrar la cuenta.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error registering user");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> SendMagicLinkAsync(string email)
    {
        if (!IsConfigured || _client == null)
            return (false, "El servicio de autenticación no está configurado.");

        try
        {
            await _client.Auth.SendMagicLink(email.Trim());
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending magic link");
            return (false, ex.Message);
        }
    }

    public async Task<string?> GetGoogleOAuthUrlAsync(string? redirectTo = null)
    {
        if (!IsConfigured || _client == null)
            return null;

        try
        {
            var options = new SignInOptions();
            if (!string.IsNullOrWhiteSpace(redirectTo))
            {
                options.RedirectTo = redirectTo;
            }

            var res = await _client.Auth.SignIn(Constants.Provider.Google, options);
            return res?.Uri?.ToString();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initiating Google OAuth sign-in");
            return null;
        }
    }

    public async Task CheckUrlForSessionAsync(string url)
    {
        if (!IsConfigured || _client == null || string.IsNullOrWhiteSpace(url))
            return;

        if (url.Contains("access_token=") || url.Contains("refresh_token="))
        {
            try
            {
                await _client.Auth.GetSessionFromUrl(new Uri(url));
                AuthStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error restoring session from OAuth redirect URL");
            }
        }
    }

    public async Task SignOutAsync()
    {
        if (_client != null)
        {
            try
            {
                await _client.Auth.SignOut();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error signing out");
            }
        }
        AuthStateChanged?.Invoke();
    }
}
