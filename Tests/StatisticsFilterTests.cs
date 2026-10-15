using SpanishWordLearner.Models;

namespace SpanishWordLearner.Tests;

public class StatisticsFilterTests
{
    [Fact]
    public void WordProgress_IsLearned_ReturnsTrueWhen3CorrectAnd70PercentAccuracy()
    {
        var progress = new WordProgress
        {
            WordId = 1,
            TimesCorrect = 3,
            TimesIncorrect = 1,
            TimesReviewed = 4
        };

        // Accuracy = 3/4 = 0.75 (75%)
        Assert.True(progress.IsLearned);
        Assert.Equal(0.75, progress.Accuracy);
    }

    [Fact]
    public void WordProgress_IsLearned_ReturnsFalseWhenLessThan3Correct()
    {
        var progress = new WordProgress
        {
            WordId = 1,
            TimesCorrect = 2,
            TimesIncorrect = 0,
            TimesReviewed = 2
        };

        // Accuracy = 100% but only 2 correct
        Assert.False(progress.IsLearned);
    }

    [Fact]
    public void WordProgress_IsLearned_ReturnsFalseWhenAccuracyBelow70Percent()
    {
        var progress = new WordProgress
        {
            WordId = 1,
            TimesCorrect = 3,
            TimesIncorrect = 3,
            TimesReviewed = 6
        };

        // Accuracy = 3/6 = 0.5 (50%)
        Assert.False(progress.IsLearned);
        Assert.Equal(0.5, progress.Accuracy);
    }

    [Fact]
    public void WordProgress_Accuracy_ReturnsZeroWhenNoReviews()
    {
        var progress = new WordProgress
        {
            WordId = 1,
            TimesCorrect = 0,
            TimesIncorrect = 0,
            TimesReviewed = 0
        };

        Assert.Equal(0, progress.Accuracy);
    }

    [Fact]
    public void FilterLearned_ReturnsOnlyLearnedWords()
    {
        var words = CreateTestWords();
        var progressList = CreateTestProgress();

        var wordProgressList = CreateWordProgressList(words, progressList);

        var learnedWords = wordProgressList.Where(x => x.Progress.IsLearned).ToList();

        Assert.Single(learnedWords);
        Assert.Equal("learned_word", learnedWords[0].Word.Spanish);
    }

    [Fact]
    public void FilterStruggling_ReturnsOnlyStrugglingWords()
    {
        var words = CreateTestWords();
        var progressList = CreateTestProgress();

        var wordProgressList = CreateWordProgressList(words, progressList);

        // Struggling: Accuracy < 0.5 AND TimesReviewed >= 3
        var strugglingWords = wordProgressList
            .Where(x => x.Progress.Accuracy < 0.5 && x.Progress.TimesReviewed >= 3)
            .ToList();

        Assert.Single(strugglingWords);
        Assert.Equal("struggling_word", strugglingWords[0].Word.Spanish);
    }

    [Fact]
    public void FilterAll_ReturnsAllWords()
    {
        var words = CreateTestWords();
        var progressList = CreateTestProgress();

        var wordProgressList = CreateWordProgressList(words, progressList);

        Assert.Equal(3, wordProgressList.Count);
    }

    [Fact]
    public void WordProgressList_MatchesWordsById()
    {
        var words = new List<SpanishWord>
        {
            new() { Id = 1, Spanish = "hola", English = "hello", PartOfSpeech = "interjection" },
            new() { Id = 2, Spanish = "adios", English = "goodbye", PartOfSpeech = "interjection" },
        };

        var progress = new Dictionary<int, WordProgress>
        {
            { 1, new WordProgress { WordId = 1, TimesCorrect = 5, TimesReviewed = 5 } },
            { 999, new WordProgress { WordId = 999, TimesCorrect = 1, TimesReviewed = 1 } }, // No matching word
        };

        var wordProgressList = new List<(SpanishWord Word, WordProgress Progress)>();
        foreach (var kvp in progress)
        {
            var word = words.FirstOrDefault(w => w.Id == kvp.Key);
            if (word != null)
            {
                wordProgressList.Add((word, kvp.Value));
            }
        }

        // Should only include word ID 1, not 999
        Assert.Single(wordProgressList);
        Assert.Equal(1, wordProgressList[0].Word.Id);
    }

    private List<SpanishWord> CreateTestWords()
    {
        return new List<SpanishWord>
        {
            new() { Id = 1, Spanish = "learned_word", English = "learned", PartOfSpeech = "noun", FrequencyRank = 1 },
            new() { Id = 2, Spanish = "struggling_word", English = "struggling", PartOfSpeech = "noun", FrequencyRank = 2 },
            new() { Id = 3, Spanish = "learning_word", English = "learning", PartOfSpeech = "noun", FrequencyRank = 3 },
        };
    }

    private Dictionary<int, WordProgress> CreateTestProgress()
    {
        return new Dictionary<int, WordProgress>
        {
            // Learned: 4 correct out of 5 = 80% accuracy, >= 3 correct
            { 1, new WordProgress { WordId = 1, TimesCorrect = 4, TimesIncorrect = 1, TimesReviewed = 5 } },

            // Struggling: 1 correct out of 4 = 25% accuracy, reviewed >= 3 times
            { 2, new WordProgress { WordId = 2, TimesCorrect = 1, TimesIncorrect = 3, TimesReviewed = 4 } },

            // Learning: 1 correct out of 2 = 50% accuracy, reviewed < 3 times (not struggling yet)
            { 3, new WordProgress { WordId = 3, TimesCorrect = 1, TimesIncorrect = 1, TimesReviewed = 2 } },
        };
    }

    private List<(SpanishWord Word, WordProgress Progress)> CreateWordProgressList(
        List<SpanishWord> words,
        Dictionary<int, WordProgress> progressDict)
    {
        var wordProgressList = new List<(SpanishWord Word, WordProgress Progress)>();
        foreach (var kvp in progressDict)
        {
            var word = words.FirstOrDefault(w => w.Id == kvp.Key);
            if (word != null)
            {
                wordProgressList.Add((word, kvp.Value));
            }
        }
        return wordProgressList;
    }
}
