namespace EFCore10DeepDive.Models;

/// <summary>
/// Demonstrates ExecuteUpdateAsync - Bulk operations without loading entities
/// Benefits: 100x performance improvement, single database roundtrip, no change tracking
/// </summary>
public class Order
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; } // Pending, Processing, Shipped, Delivered, Cancelled
    public int ItemCount { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
}
