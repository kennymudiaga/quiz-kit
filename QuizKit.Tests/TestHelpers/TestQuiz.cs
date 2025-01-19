using QuizKit.Core.Entities;

namespace QuizKit.Tests.TestHelpers;

public static class TestQuiz
{
    private record TestQuizClass : Quiz
    {
        public TestQuizClass()
        {
        }

        public void SetProperties(string id, string? title, string? organizationId, string? categoryId, DateTime? createdAt)
        {
            Id = id;
            Title = title;
            OrganizationId = organizationId;
            CategoryId = categoryId;
            CreatedAt = createdAt;
        }
    }

    public static Quiz Create(string id = "test-id", string? title = null, string? organizationId = null, 
        string? categoryId = null, DateTime? createdAt = null)
    {
        var quiz = new TestQuizClass();
        quiz.SetProperties(id, title, organizationId, categoryId, createdAt ?? DateTime.UtcNow);
        return quiz;
    }
}
