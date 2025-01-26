using QuizKit.Common.Enums;
using QuizKit.Core.Entities;

namespace QuizKit.Tests.TestHelpers;

public static class TestQuiz
{
    private record TestQuizClass : Quiz
    {
        public TestQuizClass()
        {
        }

        public TestQuizClass(string id,
        string? title, 
        string? organizationId, 
        string? categoryId, 
        DateTime? createdAt, 
        string? description = null,
        int? timeLimit = null,
        bool randomizeQuestions = false,
        bool showAnswers = false,
        string? imageUrl = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        QuizStatus status = QuizStatus.Created)
        {
            Id = id;
            Title = title;
            OrganizationId = organizationId;
            CategoryId = categoryId;
            CreatedAt = createdAt;
            Description = description;
            TimeLimit = timeLimit;
            RandomizeQuestions = randomizeQuestions;
            ShowAnswers = showAnswers;
            ImageUrl = imageUrl;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
        }
    }

    public static Quiz Create(
        string id = "test-id", 
        string? title = null, 
        string? organizationId = null, 
        string? categoryId = null, 
        DateTime? createdAt = null,
        string? description = null,
        int? timeLimit = null,
        bool randomizeQuestions = false,
        bool showAnswers = false,
        string? imageUrl = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        QuizStatus status = QuizStatus.Created)
    {
        var quiz = new TestQuizClass(
            id, 
            title, 
            organizationId, 
            categoryId, 
            createdAt ?? DateTime.UtcNow, 
            description, 
            timeLimit, 
            randomizeQuestions, 
            showAnswers, 
            imageUrl, 
            startDate, 
            endDate,
            status);
        return quiz;
    }
}
