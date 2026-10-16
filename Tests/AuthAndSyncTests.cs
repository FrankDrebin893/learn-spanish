using Microsoft.Extensions.Configuration;
using SpanishWordLearner.Models;
using SpanishWordLearner.Services;
using Supabase.Gotrue;
using Xunit;

namespace SpanishWordLearner.Tests;

public class AuthAndSyncTests
{
    [Fact]
    public async Task TestAuthServiceUnconfiguredState()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Supabase:Url", "" },
            { "Supabase:AnonKey", "" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var authService = new AuthService(configuration);
        await authService.InitializeAsync();

        Assert.False(authService.IsConfigured);
        Assert.False(authService.IsAuthenticated);
        Assert.Null(authService.CurrentUserEmail);
        Assert.Null(authService.CurrentUserId);

        var signInResult = await authService.SignInWithPasswordAsync("test@example.com", "password");
        Assert.False(signInResult.Success);
        Assert.NotNull(signInResult.ErrorMessage);

        var signUpResult = await authService.SignUpWithPasswordAsync("test@example.com", "password");
        Assert.False(signUpResult.Success);

        var magicResult = await authService.SendMagicLinkAsync("test@example.com");
        Assert.False(magicResult.Success);
    }

    [Fact]
    public void TestMergeProgressDisjointWords()
    {
        var local = new UserProgress();
        local.WordStats[1] = new WordProgress
        {
            WordId = 1,
            TimesReviewed = 5,
            TimesCorrect = 4,
            TimesIncorrect = 1,
            FirstSeen = new DateTime(2026, 1, 1),
            LastReviewed = new DateTime(2026, 1, 5)
        };

        var cloud = new UserProgress();
        cloud.WordStats[2] = new WordProgress
        {
            WordId = 2,
            TimesReviewed = 3,
            TimesCorrect = 3,
            TimesIncorrect = 0,
            FirstSeen = new DateTime(2026, 2, 1),
            LastReviewed = new DateTime(2026, 2, 2)
        };

        var merged = ProgressService.MergeProgress(local, cloud);

        Assert.Equal(2, merged.WordStats.Count);
        Assert.True(merged.WordStats.ContainsKey(1));
        Assert.True(merged.WordStats.ContainsKey(2));
        Assert.Equal(4, merged.WordStats[1].TimesCorrect);
        Assert.Equal(3, merged.WordStats[2].TimesCorrect);
    }

    [Fact]
    public void TestMergeProgressOverlappingWords()
    {
        var local = new UserProgress();
        local.WordStats[10] = new WordProgress
        {
            WordId = 10,
            TimesReviewed = 3,
            TimesCorrect = 2,
            TimesIncorrect = 1,
            FirstSeen = new DateTime(2026, 1, 10),
            LastReviewed = new DateTime(2026, 1, 12)
        };

        var cloud = new UserProgress();
        cloud.WordStats[10] = new WordProgress
        {
            WordId = 10,
            TimesReviewed = 7,
            TimesCorrect = 6,
            TimesIncorrect = 1,
            FirstSeen = new DateTime(2026, 1, 8),
            LastReviewed = new DateTime(2026, 1, 15)
        };

        var merged = ProgressService.MergeProgress(local, cloud);

        Assert.Single(merged.WordStats);
        var word = merged.WordStats[10];
        Assert.Equal(7, word.TimesReviewed);
        Assert.Equal(6, word.TimesCorrect);
        Assert.Equal(1, word.TimesIncorrect);
        Assert.Equal(new DateTime(2026, 1, 8), word.FirstSeen);
        Assert.Equal(new DateTime(2026, 1, 15), word.LastReviewed);
    }

    [Fact]
    public void TestMergeProgressQuizHistoryDeduplication()
    {
        var timestamp = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);

        var local = new UserProgress
        {
            QuizHistory = new List<QuizSession>
            {
                new() { Date = timestamp, TotalQuestions = 10, CorrectAnswers = 8 }
            }
        };

        var cloud = new UserProgress
        {
            QuizHistory = new List<QuizSession>
            {
                new() { Date = timestamp, TotalQuestions = 10, CorrectAnswers = 8 }, // Duplicate
                new() { Date = timestamp.AddHours(2), TotalQuestions = 5, CorrectAnswers = 5 } // New
            }
        };

        var merged = ProgressService.MergeProgress(local, cloud);

        Assert.Equal(2, merged.QuizHistory.Count);
        Assert.Equal(timestamp, merged.QuizHistory[0].Date);
        Assert.Equal(timestamp.AddHours(2), merged.QuizHistory[1].Date);
    }

    [Fact]
    public async Task TestBlazorSessionPersistence()
    {
        var persistence = new BlazorSessionPersistence();
        var session = new Session
        {
            AccessToken = "mock-token",
            RefreshToken = "mock-refresh"
        };

        await persistence.SaveSessionAsync(session);
        var loaded = await persistence.LoadSessionAsync();
        Assert.NotNull(loaded);
        Assert.Equal("mock-token", loaded.AccessToken);

        await persistence.DestroySessionAsync();
        var afterDestroy = await persistence.LoadSessionAsync();
        Assert.Null(afterDestroy);
    }
}
