using System.Text.Json.Serialization;

namespace QuizKit.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuizStatus
{
    Created,
    Approved,
    Live,
    Closed
}