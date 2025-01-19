using System.Text.Json.Serialization;

namespace QuizKit.Common.Results;

public record Result
{
    public string? Message { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
    [JsonIgnore]
    public ResultStatus? Status { get; set; }
    public bool IsSuccess => Status == ResultStatus.Success;
    [JsonIgnore]
    public bool IsFailure => !IsSuccess;
   


    public static Result Success() => new() { Status = ResultStatus.Success };

    public static Result<T> Success<T>(T value) => new() { Status = ResultStatus.Success, Data = value };

    public static PagedResult<T> Page<T>(List<T> value, int page, int pageSize, int totalCount)
        => new()
        {
            Status = ResultStatus.Success,
            Data = value,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

    public static Failure Failure(string error) => new(error);

    public static Failure BadRequest(string error) => new(error, ResultStatus.BadRequest);

    public static Failure BadRequest(Dictionary<string, List<string>> errors, string? message = null)
        => new(message ?? "One or more validation errors occured.", ResultStatus.BadRequest)
        {
            Message = message ?? "One or more validation errors occured.",
            Errors = errors,
        };

    public static Failure NotFound(string? error = null) => new(error, ResultStatus.NotFound);
    public static Failure Unauthorized(string? error = null) => new(error, ResultStatus.Unauthorized);
    public static Failure Forbidden(string? error = null) => new(error, ResultStatus.Forbidden);

    public Result<TOut> ToResult<TOut>() => new()
    {
        Errors = Errors,
        Status = Status,
        Message = Message,
    };
}

public record Result<T> : Result
{
    public T? Data { get; set; }

    public static implicit operator Result<T>(Failure failure)
        => new()
        {
            Errors = failure.Errors,
            Message = failure.Message,
            Status = failure.Status,
        };

}
