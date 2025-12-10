using Microsoft.Data.SqlTypes;

namespace EFCore10DeepDive.Models;

/// <summary>
/// Demonstrates Vector Search feature
/// </summary>
public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public SqlVector<float> SearchVector { get; set; }
}
