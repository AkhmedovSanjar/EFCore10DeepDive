using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10DeepDive.DemoStrategies;

/// <summary>
/// ExecuteUpdateAsync - Operations
/// Benefits: performance, single roundtrip, no tracking overhead
/// </summary>
public class ExecuteUpdateJsonDemo : DemoBase
{
    public override string FeatureName => "ExecuteUpdateAsync - Bulk Operations";
    public override string Description => "Bulk update without loading entities into memory";

    protected override async Task ExecuteDemoAsync(AppDbContext context)
    {
        Console.WriteLine("Create sample orders");
        var orders = new List<Order>
        {
            new()
            {
                OrderNumber = "ORD-2024-001",
                CustomerName = "Alice Johnson",
                OrderDate = DateTime.UtcNow.AddDays(-10),
                TotalAmount = 149.99m,
                Status = "Pending",
                ItemCount = 3
            },
            new()
            {
                OrderNumber = "ORD-2024-002",
                CustomerName = "Bob Smith",
                OrderDate = DateTime.UtcNow.AddDays(-8),
                TotalAmount = 299.50m,
                Status = "Pending",
                ItemCount = 5
            },
            new()
            {
                OrderNumber = "ORD-2024-003",
                CustomerName = "Carol White",
                OrderDate = DateTime.UtcNow.AddDays(-6),
                TotalAmount = 89.99m,
                Status = "Pending",
                ItemCount = 2
            },
            new()
            {
                OrderNumber = "ORD-2024-004",
                CustomerName = "David Brown",
                OrderDate = DateTime.UtcNow.AddDays(-5),
                TotalAmount = 499.99m,
                Status = "Pending",
                ItemCount = 8
            },
            new()
            {
                OrderNumber = "ORD-2024-005",
                CustomerName = "Emma Davis",
                OrderDate = DateTime.UtcNow.AddDays(-3),
                TotalAmount = 199.99m,
                Status = "Pending",
                ItemCount = 4
            }
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
        Console.WriteLine($"    Created {orders.Count} orders");

        Console.WriteLine("\nBulk update: Change status from 'Pending' to 'Processing'");
        Console.WriteLine("    Using ExecuteUpdateAsync - NO entity tracking, single SQL UPDATE");

        var rowsAffected = await context.Orders
            .Where(o => o.Status == "Pending")
            .ExecuteUpdateAsync(s =>
                s.SetProperty(o => o.Status, "Processing"));

        Console.WriteLine($"    + Updated {rowsAffected} orders in single database roundtrip");

        var processingOrders = await context.Orders.AsNoTracking().ToListAsync();
        foreach (var order in processingOrders)
        {
            Console.WriteLine($"    - {order.OrderNumber} - Status: {order.Status}");
        }

        Console.WriteLine("\nConditional bulk update: Ship high-value orders (>= $200)");
        rowsAffected = await context.Orders
            .Where(o => o.TotalAmount >= 200 && o.Status == "Processing")
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, "Shipped")
                .SetProperty(o => o.ShippedDate, DateTime.UtcNow));

        Console.WriteLine($"    + Shipped {rowsAffected} high-value orders");

        var allOrders = await context.Orders.AsNoTracking().OrderBy(o => o.OrderNumber).ToListAsync();
        foreach (var order in allOrders)
        {
            var shipped = order.ShippedDate.HasValue ? $"(Shipped: {order.ShippedDate:g})" : "";
            Console.WriteLine($"    - {order.OrderNumber} - ${order.TotalAmount:F2} - {order.Status} {shipped}");
        }

        Console.WriteLine("\nMultiple property updates in single query");
        Console.WriteLine("    Applying 10% discount and incrementing item count");

        rowsAffected = await context.Orders
            .Where(o => o.Status == "Processing")
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.TotalAmount, o => o.TotalAmount * 0.9m)
                .SetProperty(o => o.ItemCount, o => o.ItemCount + 1));

        Console.WriteLine($"    + Updated {rowsAffected} orders with discount");

        var discountedOrders = await context.Orders
            .AsNoTracking()
            .Where(o => o.Status == "Processing")
            .ToListAsync();

        foreach (var order in discountedOrders)
        {
            Console.WriteLine($"    - {order.OrderNumber} - ${order.TotalAmount:F2} ({order.ItemCount} items) - 10% discount applied");
        }

        // ✅ NEW PATTERN: Conditional property chain inside ExecuteUpdateAsync
        Console.WriteLine("\nConditional update example: Apply promotion based on business rules");
        bool isWeekendPromotion = DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday ||
                                   DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday;

        rowsAffected = await context.Orders
            .ExecuteUpdateAsync(s =>
            {
                s.SetProperty(o => o.Status, "Promoted");
                if (isWeekendPromotion)
                {
                    s.SetProperty(o => o.TotalAmount, o => o.TotalAmount * 0.85m); // 15% discount
                    s.SetProperty(o => o.CustomerName, o => o.CustomerName + " [WEEKEND-PROMO]");
                }
                else
                {
                    s.SetProperty(o => o.TotalAmount, o => o.TotalAmount * 0.95m); // 5% discount
                    s.SetProperty(o => o.CustomerName, o => o.CustomerName + " [WEEKDAY-LOYALTY]");
                }
            });

        Console.WriteLine($"    + Applied weekend promotion to {rowsAffected} orders");

        Console.WriteLine("\nFinal order status summary");
        var finalOrders = await context.Orders
            .AsNoTracking()
            .OrderBy(o => o.OrderNumber)
            .ToListAsync();

        foreach (var order in finalOrders)
        {
            var timeline = order.DeliveredDate.HasValue
                ? $"Delivered: {order.DeliveredDate:g}"
                : order.ShippedDate.HasValue
                    ? $"Shipped: {order.ShippedDate:g}"
                    : $"Ordered: {order.OrderDate:g}";

            Console.WriteLine($"    - {order.OrderNumber}");
            Console.WriteLine($"       Customer: {order.CustomerName} | Amount: ${order.TotalAmount:F2}");
            Console.WriteLine($"       Status: {order.Status} | {timeline}");
        }
    }
}

