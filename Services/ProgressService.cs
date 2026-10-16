using System.Text.Json;
using Microsoft.JSInterop;
using SpanishWordLearner.Models;

namespace SpanishWordLearner.Services;

public class ProgressService
{
    private const string GuestStorageKey = "spanish_word_learner_progress";
    private readonly IJSRuntime? _js;
    private readonly AuthService? _authService;
    private UserProgress? _cachedProgress;
    private bool _isInitialized;
    private string? _currentActiveUserId;

    public event Action? ProgressUpdated;
    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    public ProgressService(IJSRuntime? js = null, AuthService? authService = null)
    {
        _js = js;
        _authService = authService;

        if (_authService != null)
        {
            _authService.AuthStateChanged += OnAuthStateChanged;
        }
    }

    private string CurrentStorageKey => !string.IsNullOrEmpty(_authService?.CurrentUserId)
        ? $"spanish_word_learner_progress_{_authService.CurrentUserId}"
        : GuestStorageKey;

    private async void OnAuthStateChanged()
    {
        var newUserId = _authService?.CurrentUserId;
        if (newUserId != _currentActiveUserId)
        {
            _currentActiveUserId = newUserId;
            _cachedProgress = null;
            _isInitialized = false;

            if (!string.IsNullOrEmpty(newUserId))
            {
                await SyncWithCloudAsync();
            }
            else
            {
                await GetProgressAsync();
                ProgressUpdated?.Invoke();
            }
        }
    }

    public async Task<UserProgress> GetProgressAsync()
    {
        if (_cachedProgress != null && _isInitialized)
            return _cachedProgress;

        if (_js != null)
        {
            try
            {
                var json = await _js.InvokeAsync<string?>("localStorage.getItem", CurrentStorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cachedProgress = JsonSerializer.Deserialize<UserProgress>(json);
                }
            }
            catch
            {
                // Fallback gracefully if localStorage is unavailable
            }
        }

        _cachedProgress ??= new UserProgress();
        _isInitialized = true;
        _currentActiveUserId = _authService?.CurrentUserId;

        // If authenticated and not yet synced in this session, trigger sync
        if (_authService != null && _authService.IsAuthenticated && LastSyncedAt == null)
        {
            _ = SyncWithCloudAsync();
        }

        return _cachedProgress;
    }

    public async Task SaveProgressAsync(UserProgress progress)
    {
        _cachedProgress = progress;

        if (_js != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(progress);
                await _js.InvokeVoidAsync("localStorage.setItem", CurrentStorageKey, json);
            }
            catch
            {
                // In-memory progress preserved even if storage fails
            }
        }

        ProgressUpdated?.Invoke();

        if (_authService != null && _authService.IsAuthenticated)
        {
            _ = PushToCloudAsync(progress);
        }
    }

    public async Task RecordAnswerAsync(int wordId, bool correct)
    {
        var progress = await GetProgressAsync();

        if (!progress.WordStats.TryGetValue(wordId, out var wordProgress))
        {
            wordProgress = new WordProgress
            {
                WordId = wordId,
                FirstSeen = DateTime.UtcNow
            };
            progress.WordStats[wordId] = wordProgress;
        }

        wordProgress.TimesReviewed++;
        if (correct)
            wordProgress.TimesCorrect++;
        else
            wordProgress.TimesIncorrect++;
        wordProgress.LastReviewed = DateTime.UtcNow;

        await SaveProgressAsync(progress);
    }

    public async Task RecordQuizSessionAsync(QuizSession session)
    {
        var progress = await GetProgressAsync();
        progress.QuizHistory.Add(session);
        await SaveProgressAsync(progress);
    }

    public async Task ResetProgressAsync()
    {
        _cachedProgress = new UserProgress();

        if (_js != null)
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", CurrentStorageKey);
            }
            catch
            {
                // Fallback gracefully
            }
        }

        ProgressUpdated?.Invoke();

        if (_authService != null && _authService.IsAuthenticated)
        {
            _ = PushToCloudAsync(_cachedProgress);
        }
    }

    public async Task SyncWithCloudAsync()
    {
        if (_authService == null || !_authService.IsAuthenticated || _authService.Client == null)
            return;

        var userId = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return;

        IsSyncing = true;
        ProgressUpdated?.Invoke();

        try
        {
            // First load local progress
            var local = await GetProgressAsync();

            // Also check if there was guest progress to merge upon first login
            if (_js != null)
            {
                try
                {
                    var guestJson = await _js.InvokeAsync<string?>("localStorage.getItem", GuestStorageKey);
                    if (!string.IsNullOrWhiteSpace(guestJson))
                    {
                        var guestProgress = JsonSerializer.Deserialize<UserProgress>(guestJson);
                        if (guestProgress != null && (guestProgress.WordStats.Count > 0 || guestProgress.QuizHistory.Count > 0))
                        {
                            local = MergeProgress(local, guestProgress);
                            // Clear guest data so it's not re-merged repeatedly
                            await _js.InvokeVoidAsync("localStorage.removeItem", GuestStorageKey);
                        }
                    }
                }
                catch
                {
                    // Ignore guest storage access errors
                }
            }

            // Fetch cloud progress from Supabase
            var response = await _authService.Client
                .From<SupabaseProgressRecord>()
                .Where(x => x.UserId == userId)
                .Get();

            var cloudRecord = response.Models.FirstOrDefault();
            UserProgress merged;

            if (cloudRecord != null && !string.IsNullOrWhiteSpace(cloudRecord.DataJson))
            {
                var cloudProgress = JsonSerializer.Deserialize<UserProgress>(cloudRecord.DataJson) ?? new UserProgress();
                merged = MergeProgress(local, cloudProgress);
            }
            else
            {
                merged = local;
            }

            _cachedProgress = merged;
            _isInitialized = true;
            LastSyncedAt = DateTime.UtcNow;

            // Save merged to local storage
            if (_js != null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(merged);
                    await _js.InvokeVoidAsync("localStorage.setItem", CurrentStorageKey, json);
                }
                catch
                {
                    // Fallback
                }
            }

            // Push merged back to Supabase
            var updateRecord = new SupabaseProgressRecord
            {
                UserId = userId,
                DataJson = JsonSerializer.Serialize(merged),
                UpdatedAt = DateTime.UtcNow
            };
            await _authService.Client.From<SupabaseProgressRecord>().Upsert(updateRecord);
        }
        catch
        {
            // If offline or network error, local progress remains intact
        }
        finally
        {
            IsSyncing = false;
            ProgressUpdated?.Invoke();
        }
    }

    private async Task PushToCloudAsync(UserProgress progress)
    {
        if (_authService == null || !_authService.IsAuthenticated || _authService.Client == null)
            return;

        var userId = _authService.CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            var updateRecord = new SupabaseProgressRecord
            {
                UserId = userId,
                DataJson = JsonSerializer.Serialize(progress),
                UpdatedAt = DateTime.UtcNow
            };
            await _authService.Client.From<SupabaseProgressRecord>().Upsert(updateRecord);
            LastSyncedAt = DateTime.UtcNow;
            ProgressUpdated?.Invoke();
        }
        catch
        {
            // Graceful fallback for offline work
        }
    }

    public static UserProgress MergeProgress(UserProgress local, UserProgress cloud)
    {
        var result = new UserProgress();

        // Merge WordStats
        var allWordIds = local.WordStats.Keys.Concat(cloud.WordStats.Keys).Distinct();
        foreach (var wordId in allWordIds)
        {
            local.WordStats.TryGetValue(wordId, out var localWord);
            cloud.WordStats.TryGetValue(wordId, out var cloudWord);

            if (localWord != null && cloudWord != null)
            {
                result.WordStats[wordId] = new WordProgress
                {
                    WordId = wordId,
                    TimesCorrect = Math.Max(localWord.TimesCorrect, cloudWord.TimesCorrect),
                    TimesIncorrect = Math.Max(localWord.TimesIncorrect, cloudWord.TimesIncorrect),
                    TimesReviewed = Math.Max(localWord.TimesReviewed, cloudWord.TimesReviewed),
                    FirstSeen = localWord.FirstSeen < cloudWord.FirstSeen ? localWord.FirstSeen : cloudWord.FirstSeen,
                    LastReviewed = localWord.LastReviewed > cloudWord.LastReviewed ? localWord.LastReviewed : cloudWord.LastReviewed
                };
            }
            else if (localWord != null)
            {
                result.WordStats[wordId] = localWord;
            }
            else if (cloudWord != null)
            {
                result.WordStats[wordId] = cloudWord;
            }
        }

        // Merge QuizHistory: deduplicate by Date timestamp within 1 second
        var history = new List<QuizSession>();
        history.AddRange(local.QuizHistory);

        foreach (var cloudSession in cloud.QuizHistory)
        {
            var exists = history.Any(h =>
                Math.Abs((h.Date - cloudSession.Date).TotalSeconds) < 2 &&
                h.TotalQuestions == cloudSession.TotalQuestions &&
                h.CorrectAnswers == cloudSession.CorrectAnswers);

            if (!exists)
            {
                history.Add(cloudSession);
            }
        }

        result.QuizHistory = history.OrderBy(h => h.Date).ToList();
        return result;
    }
}
