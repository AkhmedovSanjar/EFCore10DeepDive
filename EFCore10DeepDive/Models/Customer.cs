namespace EFCore10DeepDive.Models;

/// <summary>
/// Demonstrates Complex Types
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required Address ShippingAddress { get; set; }
    public required Address BillingAddress { get; set; }
    public AlternateAddress? AlternateAddress { get; set; }
    public CustomerPreferences Preferences { get; set; }
    public List<OrderHistory> OrderHistories { get; set; } = new();
}

/// <summary>
/// Complex type for table splitting (no separate table, no JOINs)
/// </summary>
public record Address
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// One-to-one association entity (stored in separate table with JOIN)
/// </summary>
public class AlternateAddress
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// Complex type for JSON collection storage
/// </summary>
public class OrderHistory
{
    public required string OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public List<string> Items { get; set; } = new();
}

/// <summary>
/// Struct complex type with value semantics
/// </summary>
public struct CustomerPreferences
{
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public required string PreferredLanguage { get; set; }
    public required string Currency { get; set; }
    public required string ThemeMode { get; set; } // "Light" or "Dark"
}