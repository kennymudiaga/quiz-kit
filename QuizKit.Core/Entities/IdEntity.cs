using QuizKit.Core.Utils;

namespace QuizKit.Core.Entities;

public abstract record IdEntity
{
    protected IdEntity()
    {
        Id = IdGenerator.GenerateId(12);
    }

    public string Id { get; set; }
}
