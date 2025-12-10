using EFCore10DeepDive.Data;
using EFCore10DeepDive.Interfaces;

namespace EFCore10DeepDive.DemoStrategies;

/// <summary>
/// Base class for demo strategies to eliminate code duplication
/// Handles common setup: console clearing, header, context initialization, query counter
/// </summary>
public abstract class DemoBase : IDemoStrategy
{
    public abstract string FeatureName { get; }
    public abstract string Description { get; }

    public async Task ExecuteAsync()
    {
        PrintHeader();

        await using var context = new AppDbContext();
        await InitializeDatabaseAsync(context);
        
        ConfigureQueryCounter();

        await ExecuteDemoAsync(context);
    }

    /// <summary>
    /// Override this method to implement the actual demo logic
    /// </summary>
    protected abstract Task ExecuteDemoAsync(AppDbContext context);

    /// <summary>
    /// Print demo header with consistent formatting
    /// </summary>
    protected virtual void PrintHeader()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"DEMO: {FeatureName}");
        Console.WriteLine(new string('=', 70));
        Console.WriteLine();
    }

    /// <summary>
    /// Initialize database - can be overridden if needed
    /// </summary>
    protected virtual async Task InitializeDatabaseAsync(AppDbContext context)
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Configure query counter - can be overridden to disable SQL logging
    /// </summary>
    protected virtual void ConfigureQueryCounter()
    {
        AppDbContext.QueryCounter.Reset();
        AppDbContext.QueryCounter.ShowSqlInConsole = true;
    }

    /// <summary>
    /// Print query counter summary - call this at the end of your demo
    /// </summary>
    protected void PrintQuerySummary()
    {
        Console.WriteLine();
        AppDbContext.QueryCounter.PrintSummary();
    }
}
