using System.Text.Json;
using Microsoft.JSInterop;
using SpanishWordLearner.Models;

namespace SpanishWordLearner.Services;

public class ProgressService
{
    private const string StorageKey = "spanish_word_learner_progress";
    private readonly IJSRuntime? _js;
    private UserProgress? _cachedProgress;
    private bool _isInitialized;

    public ProgressService(IJSRuntime? js = null)
    {
        _js = js;
    }

    public async Task<UserProgress> GetProgressAsync()
    {
        if (_cachedProgress != null && _isInitialized)
            return _cachedProgress;

        if (_js != null)
        {
            try
            {
                var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cachedProgress = JsonSerializer.Deserialize<UserProgress>(json);
                }
            }
            catch
            {
                // Fallback gracefully if localStorage is unavailable (e.g. disabled, SSR, or testing)
            }
        }

        _cachedProgress ??= new UserProgress();
        _isInitialized = true;
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
                await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            }
            catch
            {
                // In-memory progress preserved even if storage fails
            }
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
                await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            }
            catch
            {
                // Fallback gracefully
            }
        }
    }
}
