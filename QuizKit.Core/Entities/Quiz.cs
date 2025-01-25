using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Core.Entities;

public record Quiz : IdEntity
{
    public const string QuestionsNavigationName = nameof(_questions);

    protected Quiz()
    {
    }

    public Quiz(CreateQuizCommand command)
        : this()
    {
        Title = command.Title;
        Description = command.Description;
        OrganizationId = command.OrganizationId;
        TimeLimit = command.TimeLimit;
        RandomizeQuestions = command.RandomizeQuestions;
        ShowAnswers = command.ShowAnswers;
        CreatedAt = DateTime.UtcNow;
    }

    public string? Title { get; protected set; }
    public string? Description { get; protected set; }
    public string? CategoryId { get; protected set; }
    public string? OrganizationId { get; protected set; }
    public DateTime? CreatedAt { get; protected set; }
    public DateTime? StartDate { get; protected set; }
    public DateTime? EndDate { get; protected set; }
    public string? ImageUrl { get; protected set; }
    public int? TimeLimit { get; protected set; }
    public bool RandomizeQuestions { get; protected set; }
    public bool ShowAnswers { get; protected set; }

    protected readonly List<QuizQuestion> _questions = [];

    public IReadOnlyList<QuizQuestion> Questions => _questions;

    public virtual Category? Category { get; protected set; }
    public virtual Organization? Organization { get; protected set; }

    public void AddQuestion(QuizQuestion question)
    {
        _questions.Add(question);
    }

    public void Update(UpdateQuizCommand command)
    {
        Title = command.Title;
        Description = command.Description;
        OrganizationId = command.OrganizationId;
        TimeLimit = command.TimeLimit;
        RandomizeQuestions = command.RandomizeQuestions;
        ShowAnswers = command.ShowAnswers;
        CategoryId = command.CategoryId;
        ImageUrl = command.ImageUrl;
        StartDate = command.StartDate;
        EndDate = command.EndDate;
    }
}
