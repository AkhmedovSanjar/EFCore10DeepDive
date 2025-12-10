using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using EFCore10DeepDive.Services;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;


BenchmarkRunner.Run<VectorSearchVsWhereClauseBenchmark>();

internal static class SearchConstants
{
    private static EmbeddingService _embeddingService = new();

    public const string SearchDescription = "gaming RGB accessories with lights";
    public const string SearchCategory = "Gaming Accessories";
    public const string SearchKeyword1 = "RGB";
    public const string SearchKeyword2 = "gaming";

    public static SqlVector<float> SearchVector = _embeddingService.GenerateEmbeddingAsync(SearchDescription).GetAwaiter().GetResult();

}

/// <summary>
/// Benchmark: Vector Search vs Traditional WHERE Clause
/// Compares semantic search using vector embeddings against keyword-based filtering
/// Both methods search for "gaming RGB accessories"
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class VectorSearchVsWhereClauseBenchmark
{
    private AppDbContext _context = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _context = new AppDbContext();
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        Console.WriteLine("Seeding database with 1,000 products...");

        var categories = new[]
        {
            "Gaming Accessories",
            "Computer Accessories",
            "Audio Equipment",
            "Storage Devices",
            "Networking Hardware",
            "Office Supplies",
            "Home Automation",
            "Mobile Accessories"
        };

        var products = new List<Product>();

        for (int i = 1; i <= 1000; i++)
        {
            var category = categories[i % categories.Length];
            var hasRgb = i % 3 == 0;

            products.Add(new Product
            {
                Name = $"Product {i}{(hasRgb ? " RGB" : "")}",
                Description = GenerateDescription(i, category, hasRgb),
                Category = category,
                Price = 50m + (i % 500),
                StockQuantity = 10 + (i % 200)
            });
        }

        // Batch insert for performance
        const int batchSize = 1000;
        for (int i = 0; i < products.Count; i += batchSize)
        {
            var batch = products.Skip(i).Take(batchSize).ToList();
            _context.Products.AddRange(batch);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Inserted {Math.Min(i + batchSize, products.Count)} products...");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    /// <summary>
    /// Traditional WHERE clause: Filter by category AND keyword matches
    /// </summary>
    [Benchmark(Description = "WHERE Clause (Category + Keywords)")]
    public async Task<List<Product>> SearchWithWhereClause()
    {
        return await _context.Products
            .Where(p => p.Category == SearchConstants.SearchCategory &&
                       (p.Name.Contains(SearchConstants.SearchKeyword1) ||
                        p.Description.Contains(SearchConstants.SearchKeyword1) ||
                        p.Name.Contains(SearchConstants.SearchKeyword2) ||
                        p.Description.Contains(SearchConstants.SearchKeyword2)))
            .Take(100)
            .ToListAsync();
    }

    /// <summary>
    /// Vector Search: Semantic similarity using cosine distance
    /// </summary>
    [Benchmark(Description = "Vector Search")]
    public async Task<List<Product>> SearchWithVectorSimilarity()
    {
        return await _context.Products
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, SearchConstants.SearchVector))
            .Take(100)
            .ToListAsync();
    }

    private static string GenerateDescription(int index, string category, bool hasRgb)
    {
        var baseDesc = category switch
        {
            "Gaming Accessories" => index % 2 == 0
                ? $"High-performance gaming device{(hasRgb ? " with RGB lighting" : "")} and programmable features"
                : $"Premium gaming accessory{(hasRgb ? " with customizable RGB" : "")} and advanced options",
            "Audio Equipment" => index % 2 == 0
                ? $"Wireless audio device{(hasRgb ? " with RGB indicators" : "")} with noise cancellation"
                : $"Premium audio equipment{(hasRgb ? " featuring RGB accents" : "")} with superior sound quality",
            "Computer Accessories" => $"Essential computer accessory{(hasRgb ? " with RGB illumination" : "")} for productivity",
            "Storage Devices" => $"Fast storage solution{(hasRgb ? " with LED indicators" : "")} with high capacity",
            "Networking Hardware" => $"Network device{(hasRgb ? " with status LEDs" : "")} with high-speed connectivity",
            _ => $"Quality {category.ToLower()} product{(hasRgb ? " with lighting" : "")} with excellent features"
        };

        return baseDesc;
    }
}
