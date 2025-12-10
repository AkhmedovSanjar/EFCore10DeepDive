using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;

var config = DefaultConfig.Instance
    .AddJob(Job.Default.WithRuntime(BenchmarkDotNet.Environments.CoreRuntime.Core10_0)
        .WithId("EF Core 10.0 (.NET 10)"))
    .AddJob(Job.Default.WithRuntime(BenchmarkDotNet.Environments.CoreRuntime.Core90)
        .WithId("EF Core 9.0 (.NET 9)"));

BenchmarkRunner.Run<ParameterizedCollectionsBenchmark>(config);

/// <summary>
/// Benchmark: EF Core 9 vs EF Core 10 - Parameterized Collections Performance
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class ParameterizedCollectionsBenchmark
{
    private List<string> _stringCollection = ["Completed", "Pending", "Cancelled", "Shipped", "Processing",
                                    "On Hold", "Refunded", "Failed", "Delivered", "In Transit", "Returned",
                                    "Awaiting Payment", "Partially Shipped", "Backordered", "Pre-Order" ];
    private List<string> _smallStringCollection = ["Completed", "Pending", "Cancelled", "Shipped", "Processing"];

    private List<int> _smallIntCollection = Enumerable.Range(1, 5).ToList();
    private List<int> _intCollection = Enumerable.Range(1, 10).ToList();

    private BenchDbContext _context = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _context = new BenchDbContext();
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        Console.WriteLine("==================================================================");
        Console.WriteLine("  Parameterized Collections Benchmark Setup");
        Console.WriteLine("==================================================================");

        var version = typeof(DbContext).Assembly.GetName().Version;
        Console.WriteLine($"  EF Core Version: {version?.Major}.{version?.Minor}.{version?.Build}");
        Console.WriteLine($"  Runtime: .NET {Environment.Version}");
        Console.WriteLine();
        Console.WriteLine("  Seeding database with 10,000 orders...");

        var orders = Enumerable.Range(1, 10000).Select(i => new Order
        {
            OrderNumber = $"ORD-{i:0000}",
            ProductName = $"Product {i % 100}",
            Quantity = i % 50 + 1,
            UnitPrice = (i % 100) * 10.99m,
            TotalAmount = ((i % 50) + 1) * ((i % 100) * 10.99m),
            OrderDate = DateTime.UtcNow.AddDays(-i % 365),
            Status = i % 3 == 0 ? "Completed" : "Pending"
        }).ToList();

        const int batchSize = 100;
        for (int i = 0; i < orders.Count; i += batchSize)
        {
            var batch = orders.Skip(i).Take(batchSize).ToList();
            _context.Orders.AddRange(batch);
            await _context.SaveChangesAsync();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    /// <summary>
    /// Small collection (20 items)
    /// </summary>
    [Benchmark(Description = "20 items")]
    public async Task<int> QueryWith20Items()
    {
        return await _context.Orders
            .Where(o => _smallIntCollection.Contains(o.Id))
            .CountAsync();
    }

    /// <summary>
    /// Large collection (100 items)
    /// </summary>
    [Benchmark(Description = "100 items")]
    public async Task<int> QueryWith100Items()
    {
        return await _context.Orders
            .Where(o => _intCollection.Contains(o.Id))
            .CountAsync();
    }

    /// <summary>
    /// String collection (not just integers!)
    /// </summary>
    [Benchmark(Description = "String collection (5 items)")]
    public async Task<int> QueryWith5StringCollection()
    {
        return await _context.Orders
            .Where(o => _smallStringCollection.Contains(o.Status))
            .CountAsync();
    }

    /// <summary>
    /// String collection (not just integers!)
    /// </summary>
    [Benchmark(Description = "String collection (15 items)")]
    public async Task<int> QueryWith15StringCollection()
    {
        return await _context.Orders
            .Where(o => _stringCollection.Contains(o.Status))
            .CountAsync();
    }
}

/// <summary>
/// Minimal DbContext for benchmarking
/// </summary>
public class BenchDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=ParameterizedCollectionsBench;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}

/// <summary>
/// Simple Order entity for benchmarking
/// </summary>
public class Order
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public required string Status { get; set; }
}
