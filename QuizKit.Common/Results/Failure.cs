namespace QuizKit.Common.Results;

public record Failure : Result
{
    public const string DefaultValidationMessage = "One or more validation errors occured.";

    public Failure(ResultStatus resultCode)
    {
        Status = resultCode;
    }

    public Failure(string? message, ResultStatus? resultCode = null)
    {
        Message = message;
        Status = resultCode ?? ResultStatus.BadRequest;
    }

    public Failure(Dictionary<string, List<string>> failures, string? message = null)
    {
        Errors = failures;
        Message = message ?? DefaultValidationMessage;
        Status = ResultStatus.BadRequest;
    }
}
