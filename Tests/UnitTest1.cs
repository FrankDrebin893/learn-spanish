using SpanishWordLearner.Models;
using SpanishWordLearner.Services;

namespace SpanishWordLearner.Tests;

public class VocabularyAndProgressTests
{
    [Fact]
    public void TestLoadWordsFromDataFiles()
    {
        var freqPath = Path.Combine("..", "..", "..", "..", "spanish_data", "frequency.csv");
        var dictPath = Path.Combine("..", "..", "..", "..", "spanish_data", "es-en.data");
        if (File.Exists(freqPath) && File.Exists(dictPath))
        {
            var words = DataLoader.LoadFromFiles(freqPath, dictPath);
            Assert.NotEmpty(words);
            Assert.True(words.Count >= 1000);
        }
    }

    [Fact]
    public void TestWordServiceFallbackWords()
    {
        var wordService = new WordService();
        Assert.True(wordService.TotalWordCount >= 200);

        var top10 = wordService.GetWordsByFrequency(10);
        Assert.Equal(10, top10.Count);

        var firstWord = wordService.GetWordById(1);
        Assert.NotNull(firstWord);
        Assert.Equal("de", firstWord.Spanish);

        var random = wordService.GetRandomWords(5, new List<int> { 1 });
        Assert.Equal(5, random.Count);
        Assert.DoesNotContain(random, w => w.Id == 1);
    }

    [Fact]
    public async Task TestProgressServiceOperations()
    {
        var progressService = new ProgressService();
        var initial = await progressService.GetProgressAsync();
        Assert.NotNull(initial);
        Assert.Empty(initial.WordStats);
        Assert.Empty(initial.QuizHistory);

        // Record a correct answer
        await progressService.RecordAnswerAsync(1, true);
        var progress = await progressService.GetProgressAsync();
        Assert.True(progress.WordStats.ContainsKey(1));
        Assert.Equal(1, progress.WordStats[1].TimesReviewed);
        Assert.Equal(1, progress.WordStats[1].TimesCorrect);
        Assert.Equal(0, progress.WordStats[1].TimesIncorrect);
        Assert.Equal(1.0, progress.WordStats[1].Accuracy);

        // Record 2 more correct answers to verify mastery threshold (3 correct >= 70%)
        await progressService.RecordAnswerAsync(1, true);
        await progressService.RecordAnswerAsync(1, true);
        progress = await progressService.GetProgressAsync();
        Assert.True(progress.WordStats[1].IsLearned);
        Assert.Equal(1, progress.TotalWordsLearned);

        // Record a quiz session
        var session = new QuizSession
        {
            Date = DateTime.UtcNow,
            TotalQuestions = 10,
            CorrectAnswers = 9,
            WordIdsTested = new List<int> { 1 }
        };
        await progressService.RecordQuizSessionAsync(session);
        progress = await progressService.GetProgressAsync();
        Assert.Single(progress.QuizHistory);
        Assert.Equal(1, progress.TotalQuizzesTaken);
        Assert.Equal(0.9, progress.OverallAccuracy);

        // Reset progress
        await progressService.ResetProgressAsync();
        progress = await progressService.GetProgressAsync();
        Assert.Empty(progress.WordStats);
        Assert.Empty(progress.QuizHistory);
    }
}
