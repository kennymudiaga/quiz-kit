using AutoMapper;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Core.Entities;
using QuizKit.Core.Mappers;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.Mappers;

public class QuizMappingProfileTests
{
    private readonly IMapper _mapper;

    public QuizMappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<QuizMappingProfile>());
        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Map_Quiz_To_QuizPreviewModel_Maps_All_Properties_Correctly()
    {
        // Arrange
        var quiz = TestQuiz.Create(
            id: "quiz-123",
            title: "Test Quiz",
            categoryId: "cat-1",
            organizationId: "org-1",
            description: "Test Description",
            timeLimit: 30,
            randomizeQuestions: true,
            showAnswers: true,
            imageUrl: "test-image.png",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddHours(1));
        quiz.AddQuestion(new QuizQuestion());
        quiz.AddQuestion(new QuizQuestion());

        // Act
        var result = _mapper.Map<QuizPreviewModel>(quiz);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(quiz.Id, result.Id);
        Assert.Equal(quiz.Title, result.Title);
        Assert.Equal(quiz.Description, result.Description);
        Assert.Equal(quiz.StartDate, result.StartDate);
        Assert.Equal(quiz.TimeLimit, result.TimeLimit);
        Assert.Equal(2, result.QuestionsCount);
        Assert.Equal(quiz.CategoryId, result.Category);
    }

    [Fact]
    public void Map_Quiz_To_QuizPreviewModel_Handles_Null_Values()
    {
        // Arrange
        var quiz = TestQuiz.Create(
            id: "quiz-123",
            title: null,
            categoryId: "cat-1");            

        // Act
        var result = _mapper.Map<QuizPreviewModel>(quiz);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(quiz.Id, result.Id);
        Assert.Equal(string.Empty, result.Title);
        Assert.Null(result.Description);
        Assert.Equal(DateTime.MinValue, result.StartDate);
        Assert.Null(result.TimeLimit);
        Assert.Equal(0, result.QuestionsCount);
        Assert.Equal(quiz.CategoryId, result.Category);
    }
}
