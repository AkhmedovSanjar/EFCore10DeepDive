namespace EFCore10DeepDive.Interfaces;

/// <summary>
/// Strategy Pattern: Each EF Core 10 feature has its own demo strategy
/// </summary>
public interface IDemoStrategy
{
    string FeatureName { get; }
    string Description { get; }
    Task ExecuteAsync();
}
