using System.Net;
using System.Text.Json.Serialization;

namespace QuizKit.Common.Results;

public record ApiResult : Result
{
    public HttpStatusCode StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }

    [JsonIgnore]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the error message that occurred during serialization or deserialization.
    /// This is meant to be used for debugging purposes only.
    /// </summary>
    [JsonIgnore]
    public string? ExceptionMessage { get; set; }
}

public record ApiResult<T> : ApiResult
{
    public T? Value { get; set; }

    public ApiResult() { }
    public ApiResult(Result<T> result)
    {
        Message = result.Message;
        Errors = result.Errors;
        Value = result.Data;
        Status = result.Status;
    }
}
