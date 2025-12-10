namespace EFCore10DeepDive.Models;

/// <summary>
/// Demonstrates Named Query Filters - Multi-tenant + Soft Delete patterns
/// Benefits: Multiple filters per entity, selective filter ignoring
/// </summary>
public class Account
{
    public int Id { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountName { get; set; }
    public required string Email { get; set; }
    public decimal Balance { get; set; }
    public required string AccountType { get; set; } // Savings, Checking, Investment
    
    /// <summary>
    /// Soft delete flag - used by "SoftDelete" named query filter
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Multi-tenant isolation - used by "Tenant" named query filter
    /// </summary>
    public int TenantId { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}
