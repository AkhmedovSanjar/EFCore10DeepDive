using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10DeepDive.DemoStrategies;

/// <summary>
/// NEW in EF Core 10: Parameterized Collections with Intelligent Padding
/// </summary>
public class ParameterizedCollectionsDemo : DemoBase
{
    public override string FeatureName => "Parameterized Collections";
    public override string Description => "Intelligent padding reduces query plan cache bloat";

    protected override async Task ExecuteDemoAsync(AppDbContext context)
    {
        Console.WriteLine("[Setup] Creating 20 sample orders");
        var orders = Enumerable.Range(1, 20).Select(i => new Order
        {
            OrderNumber = $"ORD-2024-{i:000}",
            CustomerName = $"Customer {i}",
            OrderDate = DateTime.UtcNow.AddDays(-i),
            TotalAmount = i * 25.50m,
            Status = i % 3 == 0 ? "Shipped" : "Processing",
            ItemCount = i % 5 + 1
        }).ToList();

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
        Console.WriteLine($"    Created {orders.Count} orders\n");

        Console.WriteLine("Query with 3 IDs (padded to 4)");
        Console.WriteLine("    EF Core 10 uses individual parameters and pads to power of 2");
        int[] smallIds = [1, 2, 3];
        var smallResult = await context.Orders
            .Where(o => smallIds.Contains(o.Id))
            .ToListAsync();
        Console.WriteLine($"    Result: {smallResult.Count} orders");
        foreach (var order in smallResult)
        {
            Console.WriteLine($"       - {order.OrderNumber}: {order.CustomerName}");
        }

        Console.WriteLine("\nQuery with 7 IDs (padded to 8)");
        int[] mediumIds = [1, 2, 3, 4, 5, 6, 7];
        var mediumResult = await context.Orders
            .Where(o => mediumIds.Contains(o.Id))
            .ToListAsync();
        Console.WriteLine($"    Result: {mediumResult.Count} orders");
        Console.WriteLine($"    First few: {string.Join(", ", mediumResult.Take(3).Select(o => o.OrderNumber))}...");
    }
}

