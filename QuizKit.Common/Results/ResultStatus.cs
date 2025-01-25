using System.Text.Json.Serialization;

namespace QuizKit.Common.Results;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResultStatus
{
    Unknown,
    Failure,
    BadRequest,
    NotFound,
    Unauthorized,
    Forbidden,
    Okay,
    Created,
}
