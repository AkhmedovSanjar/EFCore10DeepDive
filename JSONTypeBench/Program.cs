using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using Microsoft.EntityFrameworkCore;

BenchmarkRunner.Run<ComplexTypeStorageBenchmark>();

/// <summary>
/// Benchmark: Complex Types - Column Storage vs JSON Storage
/// Compares query performance of ShippingAddress (columns) vs BillingAddress (JSON) vs AlternateAddress (one-to-one association)
/// </summary>
[MemoryDiagnoser]
public class ComplexTypeStorageBenchmark
{
    private AppDbContext _context = null!;
    private const string SearchCity = "Seattle";

    [GlobalSetup]
    public async Task Setup()
    {
        _context = new AppDbContext();
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        Console.WriteLine("Seeding database with 10,000 customers...");

        var cities = new[] { "Seattle", "Redmond", "Bellevue", "Tacoma", "Spokane", "Portland", "San Francisco", "Los Angeles" };
        var customers = new List<Customer>();

        for (int i = 1; i <= 10000; i++)
        {
            var shippingCity = cities[i % cities.Length];
            var billingCity = cities[(i + 1) % cities.Length];

            customers.Add(new Customer
            {
                Name = $"Customer {i}",
                Email = $"customer{i}@example.com",
                ShippingAddress = new Address
                {
                    Street = $"{i} Shipping St",
                    City = shippingCity,
                    State = "WA",
                    ZipCode = $"{98000 + i % 999:00000}",
                    Country = "USA"
                },
                BillingAddress = new Address
                {
                    Street = $"{i} Billing Ave",
                    City = billingCity,
                    State = "WA",
                    ZipCode = $"{98000 + i % 999:00000}",
                    Country = "USA"
                },
                AlternateAddress = i % 2 == 0 ? new AlternateAddress
                {
                    Street = $"{i} Alternate Blvd",
                    City = cities[(i + 2) % cities.Length],
                    State = "WA",
                    ZipCode = $"{98000 + i % 999:00000}",
                    Country = "USA"
                } : null,
                OrderHistories = new List<OrderHistory>
                {
                    new()
                    {
                        OrderId = $"ORD-{i:000000}",
                        OrderDate = DateTime.UtcNow.AddDays(-i % 365),
                        TotalAmount = i * 10.99m,
                        Status = i % 3 == 0 ? "Delivered" : "Shipped",
                        Items = [$"Item {i}-1", $"Item {i}-2"]
                    }
                },
                Preferences = new CustomerPreferences
                {
                    EmailNotifications = i % 2 == 0,
                    SmsNotifications = i % 3 == 0,
                    PreferredLanguage = "en-US",
                    Currency = "USD",
                    ThemeMode = i % 2 == 0 ? "Dark" : "Light"
                }
            });
        }

        // Batch insert for performance
        const int batchSize = 1000;
        for (int i = 0; i < customers.Count; i += batchSize)
        {
            var batch = customers.Skip(i).Take(batchSize).ToList();
            _context.Customers.AddRange(batch);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Inserted {Math.Min(i + batchSize, customers.Count)} customers...");
        }

        Console.WriteLine($"Setup complete: {customers.Count} customers inserted");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    /// <summary>
    /// Query by BillingAddress.City (Complex Type - stored as JSON)
    /// Uses JSON_VALUE() to extract property from JSON column
    /// </summary>
    [Benchmark(Description = "Complex Type (JSON): Query BillingAddress.City")]
    public async Task<List<Customer>> QueryByBillingAddress()
    {
        return await _context.Customers
            .Where(c => c.BillingAddress.City == SearchCity)
            .Take(100)
            .ToListAsync();
    }

    /// <summary>
    /// Query by ShippingAddress.City (Complex Type - stored as columns)
    /// Fast: Direct column access with potential index support
    /// </summary>
    [Benchmark(Description = "Complex Type (Columns): Query ShippingAddress.City")]
    public async Task<List<Customer>> QueryByShippingAddress()
    {
        return await _context.Customers
            .Where(c => c.ShippingAddress.City == SearchCity)
            .Take(100)
            .ToListAsync();
    }

    /// <summary>
    /// Query by AlternateAddress.City (One-to-one association - separate table)
    /// Requires JOIN to AlternateAddresses table
    /// </summary>
    [Benchmark(Description = "One-to-One Association: Query AlternateAddress.City")]
    public async Task<List<Customer>> QueryByAlternateAddress()
    {
        return await _context.Customers.Include(c => c.AlternateAddress)
            .Where(c => c.AlternateAddress != null && c.AlternateAddress.City == SearchCity)
            .Take(100)
            .ToListAsync();
    }
}
