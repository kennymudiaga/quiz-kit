using System.Text.Json.Serialization;

namespace QuizKit.Common.Results;

public record Result
{
    public string? Message { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
    [JsonIgnore]
    public ResultStatus? Status { get; set; }
    public bool IsSuccess => Status == ResultStatus.OK;
    [JsonIgnore]
    public bool IsFailure => !IsSuccess;
   


    public static Result Success() => new() { Status = ResultStatus.OK };

    public static Result<T> Success<T>(T value) => new() { Status = ResultStatus.OK, Data = value };

    public static Result<T> Created<T>(T value) => new() { Status = ResultStatus.Created, Data = value };

    public static PagedResult<T> Page<T>(List<T> value, int page, int pageSize, int totalCount)
        => new()
        {
            Status = ResultStatus.OK,
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

    public static Failure NotFound() => new(ResultStatus.NotFound);
    public static Failure Unauthorized() => new(ResultStatus.Unauthorized);
    public static Failure Forbidden() => new(ResultStatus.Forbidden);

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
