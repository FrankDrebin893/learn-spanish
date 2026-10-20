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

    [Fact]
    public void TestGetRandomWordsFiltersByPartOfSpeech()
    {
        var wordService = new WordService();
        var allWords = wordService.GetAllWords();

        foreach (var word in allWords)
        {
            var options = wordService.GetRandomWords(3, new List<int> { word.Id }, word.PartOfSpeech);
            Assert.Equal(3, options.Count);
            Assert.DoesNotContain(options, o => o.Id == word.Id);
            Assert.All(options, o => Assert.Equal(word.PartOfSpeech, o.PartOfSpeech, ignoreCase: true));
        }
    }

    [Fact]
    public async Task TestGetQuizWordsExcludesLearnedWordsByDefaultAndAllowsFullProgression()
    {
        var wordService = new WordService();
        var progressService = new ProgressService();
        var totalWords = wordService.TotalWordCount;

        var quizzedAndLearnedIds = new HashSet<int>();

        // Simulate progressing through the entire vocabulary via quizzes
        while (quizzedAndLearnedIds.Count < totalWords)
        {
            var progress = await progressService.GetProgressAsync();
            var quizWords = wordService.GetQuizWords(10, 100, progress, excludeLearned: true);

            Assert.NotEmpty(quizWords);
            // None of the selected quiz words should already be learned
            Assert.All(quizWords, w => Assert.DoesNotContain(w.Id, quizzedAndLearnedIds));

            // Mark all selected quiz words as learned (3 correct answers)
            foreach (var w in quizWords)
            {
                await progressService.RecordAnswerAsync(w.Id, true);
                await progressService.RecordAnswerAsync(w.Id, true);
                await progressService.RecordAnswerAsync(w.Id, true);
                quizzedAndLearnedIds.Add(w.Id);
            }
        }

        Assert.Equal(totalWords, quizzedAndLearnedIds.Count);
    }

    [Fact]
    public async Task TestFull10000WordsDatasetCategoriesAndProgression()
    {
        var wordService = new WordService();
        await wordService.InitializeAsync();
        var allWords = wordService.GetAllWords();
        Assert.Equal(10000, allWords.Count);

        var categoryCounts = allWords
            .GroupBy(w => w.PartOfSpeech, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        // Verify distractor generation across all categories in the 10,000-word dataset
        foreach (var group in allWords.GroupBy(w => w.PartOfSpeech, StringComparer.OrdinalIgnoreCase))
        {
            var sampleWord = group.First();
            var distractors = wordService.GetRandomWords(3, new List<int> { sampleWord.Id }, sampleWord.PartOfSpeech);
            Assert.Equal(3, distractors.Count);
            Assert.DoesNotContain(distractors, d => d.Id == sampleWord.Id);

            if (categoryCounts[sampleWord.PartOfSpeech] >= 4)
            {
                Assert.All(distractors, d => Assert.Equal(sampleWord.PartOfSpeech, d.PartOfSpeech, ignoreCase: true));
            }
        }

        // Simulate progressing through all 10,000 words without ever repeating a learned word
        var progress = new UserProgress();
        var learnedIds = new HashSet<int>();

        while (learnedIds.Count < allWords.Count)
        {
            var batch = wordService.GetQuizWords(50, 10000, progress, excludeLearned: true);
            Assert.NotEmpty(batch);
            Assert.All(batch, w => Assert.DoesNotContain(w.Id, learnedIds));

            foreach (var word in batch)
            {
                progress.WordStats[word.Id] = new WordProgress
                {
                    WordId = word.Id,
                    TimesReviewed = 3,
                    TimesCorrect = 3,
                    TimesIncorrect = 0
                };
                learnedIds.Add(word.Id);
            }
        }

        Assert.Equal(10000, learnedIds.Count);
        Assert.Equal(10000, progress.TotalWordsLearned);
    }
}
