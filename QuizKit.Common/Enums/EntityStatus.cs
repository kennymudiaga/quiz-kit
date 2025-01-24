using System.Text.Json.Serialization;

namespace QuizKit.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityStatus
{
    Unknown,
    Active,
    Inactive,
    Deleted,
}
