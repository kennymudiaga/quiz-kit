namespace QuizKit.Core.Options;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ConfigSectionNameAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
