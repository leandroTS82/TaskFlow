namespace TaskFlow.Processor.Settings;

public sealed class ProcessorSettings
{
    public const string SectionName = "Processor";
    public int IntervalSeconds { get; init; } = 5;
}