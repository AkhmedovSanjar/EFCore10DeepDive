using EFCore10DeepDive.DemoStrategies;
using EFCore10DeepDive.Interfaces;
using EFCore10DeepDive.Services;

// EF Core 10 feature demos
var strategies = new List<IDemoStrategy>
{
    new ComplexTypesDemo(),             // 1. Complex Types
    new ExecuteUpdateJsonDemo(),        // 2. ExecuteUpdate - Bulk operations
    new VectorSearchDemo(),             // 3. Vector Search (SQL Server 2025)
    new NamedQueryFiltersDemo(),        // 4. Named query filters
    new LeftJoinDemo(),                 // 5. LeftJoin/RightJoin operators (.NET 10)
    new ParameterizedCollectionsDemo(), // 6. Improved parameterized collections
};

var runner = new DemoRunner(strategies);

while (true)
{
    runner.ShowMenu();
    Console.Write("Select option: ");
    
    if (!int.TryParse(Console.ReadLine(), out int choice))
    {
        Console.WriteLine("Invalid input. Press any key to try again...");
        Console.ReadKey();
        continue;
    }

    if (choice == 0)
    {
        Console.WriteLine("\nThank you for exploring EF Core 10 NEW features!");
        break;
    }

    if (choice == strategies.Count + 1)
    {
        await runner.RunAllAsync();
        Console.WriteLine("\n\nPress any key to return to menu...");
        Console.ReadKey();
    }
    else if (choice >= 1 && choice <= strategies.Count)
    {
        await runner.RunSpecificAsync(choice);
        Console.WriteLine("\n\nPress any key to return to menu...");
        Console.ReadKey();
    }
    else
    {
        Console.WriteLine("Invalid choice. Press any key to try again...");
        Console.ReadKey();
    }
}