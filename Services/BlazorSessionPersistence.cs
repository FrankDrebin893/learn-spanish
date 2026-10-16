using System.Text.Json;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace SpanishWordLearner.Services;

public class BlazorSessionPersistence : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "supabase_auth_session";
    private readonly IJSRuntime? _js;
    private Session? _cachedSession;

    public BlazorSessionPersistence(IJSRuntime? js = null)
    {
        _js = js;
    }

    public void SaveSession(Session session)
    {
        _cachedSession = session;
    }

    public Session? LoadSession()
    {
        return _cachedSession;
    }

    public void DestroySession()
    {
        _cachedSession = null;
    }

    public async Task SaveSessionAsync(Session session, CancellationToken cancellationToken = default)
    {
        _cachedSession = session;
        if (_js != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(session);
                await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            }
            catch
            {
                // Graceful fallback if localStorage is disabled
            }
        }
    }

    public async Task<Session?> LoadSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedSession != null)
            return _cachedSession;

        if (_js != null)
        {
            try
            {
                var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cachedSession = JsonSerializer.Deserialize<Session>(json);
                    return _cachedSession;
                }
            }
            catch
            {
                // Graceful fallback
            }
        }

        return null;
    }

    public async Task DestroySessionAsync(CancellationToken cancellationToken = default)
    {
        _cachedSession = null;
        if (_js != null)
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            }
            catch
            {
                // Graceful fallback
            }
        }
    }
}
