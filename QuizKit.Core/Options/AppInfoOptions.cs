namespace QuizKit.Core.Options;

[ConfigSectionName(ConfigSection)]
public class AppInfoOptions : OptionsBase
{
    public const string ConfigSection = "AppInfo";

    public string? AppName { get; set; }
    public string? Version { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? Organization { get; set; }
}
