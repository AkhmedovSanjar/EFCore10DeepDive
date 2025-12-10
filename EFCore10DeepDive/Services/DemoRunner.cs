using EFCore10DeepDive.Interfaces;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Facade Pattern: Simplifies running multiple demo strategies
/// </summary>
public class DemoRunner
{
    private readonly List<IDemoStrategy> _strategies;

    public DemoRunner(IEnumerable<IDemoStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public async Task RunAllAsync()
    {
        for (int i = 0; i < _strategies.Count; i++)
        {
            var strategy = _strategies[i];
            try
            {
                await strategy.ExecuteAsync();
                
                if (i < _strategies.Count - 1)
                {
                    Console.WriteLine("\n\nPress any key to continue to next demo...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] Error in {strategy.FeatureName}: {ex.Message}");
                Console.WriteLine($"   {ex.StackTrace}");
            }
        }
    }

    public async Task RunSpecificAsync(int demoNumber)
    {
        if (demoNumber < 1 || demoNumber > _strategies.Count)
        {
            Console.WriteLine($"Invalid demo number. Please choose 1-{_strategies.Count}");
            return;
        }

        var strategy = _strategies[demoNumber - 1];
        await strategy.ExecuteAsync();
    }

    public void ShowMenu()
    {
        Console.Clear();

        for (int i = 0; i < _strategies.Count; i++)
        {
            var strategy = _strategies[i];
            Console.WriteLine($"{i + 1}. {strategy.FeatureName}");
            Console.WriteLine($"   >> {strategy.Description}");
            Console.WriteLine();
        }

        Console.WriteLine($"{_strategies.Count + 1}. Run ALL Demos");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
    }
}
