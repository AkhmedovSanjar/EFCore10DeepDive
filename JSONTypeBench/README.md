# Complex Type Storage Benchmark

## What This Benchmarks

Compares query performance between two storage strategies for complex types:
1. **Column Storage** - ShippingAddress stored as individual columns (`ShippingAddress_City`, `ShippingAddress_Street`, etc.)
2. **JSON Storage** - BillingAddress stored as JSON column (SQL Server 2025 native JSON type)

Both benchmarks use the **same WHERE clause**: `WHERE City == "Seattle"`

## Benchmark Details

- **Dataset**: 10,000 customers
- **Query**: Filter by city name (`City == "Seattle"`), return 100 results
- **Column Storage**: Direct column access (`ShippingAddress_City`)
- **JSON Storage**: Uses `JSON_VALUE()` function to extract city from JSON

## What We're Testing

```csharp
// SAME WHERE CLAUSE for both:
.Where(c => c.Address.City == "Seattle")

// Method 1: Complex Type as Columns - Direct column query
context.Customers
    .Where(c => c.ShippingAddress.City == SearchCity)
    .Take(100)
    .ToListAsync();

// Method 2: Complex Type as JSON - JSON extraction query
context.Customers
    .Where(c => c.BillingAddress.City == SearchCity)
    .Take(100)
    .ToListAsync();
```

## Running the Benchmark

```bash
cd JSONTypeBench
dotnet run -c Release
```

**Important:** Must run in Release mode (`-c Release`) for accurate results!

## Expected Results

### Without Indexes (Raw Performance)

| Method                                          | Mean     | Allocated |
|-------------------------------------------------|----------|-----------|
| Complex Type (Columns): Query ShippingAddress  | ~15 ms   | ~50 KB    |
| Complex Type (JSON): Query BillingAddress      | ~25 ms   | ~50 KB    |

**Column Storage is ~40-60% faster** because:
- Direct column access (no parsing)
- No JSON extraction overhead
- Can use standard B-tree indexes
- Query optimizer has better statistics

## SQL Generated

### Complex Type as Columns
```sql
-- EF Core translates to direct column access
SELECT TOP(100) [c].[Id], [c].[Name], [c].[Email],
       [c].[ShippingAddress_Street], [c].[ShippingAddress_City], 
       [c].[ShippingAddress_State], [c].[ShippingAddress_ZipCode]
FROM [Customers] AS [c]
WHERE [c].[ShippingAddress_City] = N'Seattle'
```

### Complex Type as JSON (SQL Server 2025)
```sql
-- EF Core uses JSON_VALUE() to extract from JSON
SELECT TOP(100) [c].[Id], [c].[Name], [c].[Email],
       [c].[BillingAddress]
FROM [Customers] AS [c]
WHERE JSON_VALUE([c].[BillingAddress], '$.City') = N'Seattle'
```

## Performance Comparison

### Column Storage (ShippingAddress)
- **Faster queries** - Direct column access
- **Better for WHERE clauses** - No parsing overhead
- **Easier to index** - Standard column indexes
- **Query optimizer friendly** - Better statistics
- **More columns** - Wider table schema
- **Less flexible** - Schema changes require migrations

### JSON Storage (BillingAddress)
- **Flexible schema** - Easy to add properties
- **Compact storage** - 50% less space (SQL Server 2025)
- **Good for collections** - Order histories, preferences
- **Atomic updates** - Update entire object at once
- **Slower queries** - JSON parsing overhead
- **Limited indexing** - Requires computed columns

## When to Use Each

### Use Column Storage When:
- Properties are **frequently queried** in WHERE clauses
- Need **fast filtering** and sorting
- Want to create **indexes** on individual properties
- Performance is **critical**
- Schema is **stable**

**Examples:**
- User addresses (frequently searched by city/state)
- Product details (searched by category/price)
- Order information (filtered by status/date)

### Use JSON Storage When:
- Properties are **rarely queried** individually
- Schema **changes frequently**
- Working with **collections** or **nested objects**
- Want **compact storage**
- Data is mostly **read as a whole**

**Examples:**
- User preferences/settings
- Order histories
- Audit logs
- Metadata/tags
- Dynamic attributes

## Optimization Tips

### For Column Storage
```csharp
// Create index on frequently queried columns
modelBuilder.Entity<Customer>()
    .HasIndex(c => c.ShippingAddress.City);
```

### For JSON Storage (SQL Server 2025)
```csharp
// Create computed column + index for frequent queries
context.Database.ExecuteSqlRaw(@"
    ALTER TABLE Customers 
    ADD BillingCity AS JSON_VALUE(BillingAddress, '$.City') PERSISTED;
    
    CREATE INDEX IX_BillingCity ON Customers(BillingCity);
");
```

## Best Practice: Use Both Together

```csharp
public class Customer
{
    // Column Storage - for frequently queried properties
    public Address ShippingAddress { get; set; }  // Columns
    
    // JSON Storage - for flexible/less queried data
    public Address BillingAddress { get; set; }   // JSON
    public List<OrderHistory> OrderHistories { get; set; }  // JSON Array
    public CustomerPreferences Preferences { get; set; }    // JSON
}
```

## Key Takeaways

1. **Column Storage is 40-60% faster** for individual property queries
2. **JSON is more flexible** for evolving schemas
3. **Both can be indexed** - JSON requires computed columns
4. **SQL Server 2025** JSON improvements narrow the gap
5. **Use both together** - Columns for hot paths, JSON for flexibility

## Related

- [EF Core Complex Types Docs](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types)
- [SQL Server JSON Support](https://learn.microsoft.com/en-us/sql/relational-databases/json/json-data-sql-server)
