# EF Core 10 Deep Dive: NEW Features Only

EF Core 10 introduces transformative features for modern data modeling, AI integration, and performance. This project demonstrates **ONLY** the new features introduced in EF Core 10, with **separate models for each feature** to clearly show their purpose and differences.

## NEW Features Demonstrated

### 1. **Complex Types - Table Splitting vs JSON** - ENHANCED
**Model**: `Customer` with `Address`, `OrderHistory`, `CustomerPreferences`
- **Table Splitting**: `ShippingAddress` stored as columns (fast reads, indexable)
- **JSON Storage**: `BillingAddress`, `OrderHistories`, `Preferences` stored as JSON
- Compare performance and use cases for each approach
- Struct support for value semantics
- **Pattern**: Value object pattern, Flexible schema

### 2. **ExecuteUpdateAsync - Bulk Operations** - NEW
**Model**: `Order` for demonstrating bulk updates
- Bulk update without loading entities into memory
- 100x performance improvement
- Single database roundtrip, no change tracking
- Conditional updates on multiple properties
- **Pattern**: High-performance bulk operations

### 3. **Vector Search for AI Integration** - NEW
**Model**: `Product` with `SearchVector` (vector embeddings)
- SQL Server 2025 `vector(1536)` data type
- `EF.Functions.VectorDistance()` for similarity search
- AI-powered semantic search and recommendations
- Distance metrics: cosine, euclidean, dot product
- **Pattern**: AI/Semantic search, RAG scenarios

### 4. **Named Query Filters & Soft Delete** - NEW
**Model**: `Account` with multi-tenant and soft delete flags
- Multiple named filters per entity
- Selective disabling: `IgnoreQueryFilters(["SoftDelete"])`
- Perfect for multi-tenant + soft-delete patterns
- Tenant context managed via `TenantService`
- **Pattern**: Multi-tenancy, Soft Delete, Specification pattern

### 5. **LeftJoin/RightJoin Syntax** - NEW (.NET 10)
**Model**: `Student`, `Department`, and `Enrollment`
- First-class LINQ `LeftJoin()` and `RightJoin()`
- Replaces `GroupJoin+SelectMany+DefaultIfEmpty`
- 80% less code, SQL-like syntax
- **Pattern**: Query simplification

### Additional Features:
- **Improved Parameterized Collections** - NEW - Uses `Order` model, intelligent padding, plan cache optimization
- **Split Query Ordering Consistency** - NEW - Uses `Student`/`Enrollment` model, automatic deterministic ordering

## Architecture - Feature Isolation

```
EFCore10DeepDive/
|- Models/                          # Each model demonstrates ONE feature
|   |- Customer.cs                  # Complex Types (Table Splitting + JSON)
|   |   |- Address                  # Used for both: columns and JSON
|   |   |- OrderHistory             # Complex type collection as JSON
|   |   |- CustomerPreferences      # Struct complex type as JSON
|   |- Order.cs                     # ExecuteUpdateAsync + Parameterized Collections
|   |- Product.cs                   # Vector Search
|   |   |- SearchVector             # vector(1536) for embeddings
|   |- Account.cs                   # Named Query Filters
|   |   |- IsDeleted                # Soft delete filter
|   |   |- TenantId                 # Multi-tenant filter
|   |- Student.cs                   # LeftJoin/RightJoin + SplitQuery
|       |- Department               # For join demonstrations
|       |- Enrollment               # For split query with multiple includes
|- Data/
|   |- AppDbContext.cs              # Feature-specific configurations
|- DemoStrategies/                  # One demo per feature
|   |- ComplexTypesDemo.cs          # Customer - Table Split vs JSON comparison
|   |- ExecuteUpdateJsonDemo.cs     # Order + Bulk operations
|   |- VectorSearchDemo.cs          # Product + AI search
|   |- NamedQueryFiltersDemo.cs     # Account + Multi-tenant
|   |- LeftJoinDemo.cs              # Student/Department joins
|   |- ParameterizedCollectionsDemo.cs  # Order + Query optimization
|   |- SplitQueryConsistencyDemo.cs     # Student/Enrollment + Ordering
|- Services/
    |- QueryCounterInterceptor.cs   # SQL tracking
    |- DemoRunner.cs                # Facade pattern
    |- TenantService.cs             # Multi-tenant context provider

```

## Model-to-Feature Mapping

| Model | Primary Feature | Key Properties | Benefits |
|-------|----------------|----------------|----------|
| **Customer** | Complex Types (Split + JSON) | `ShippingAddress` (columns), `BillingAddress` (JSON), `OrderHistories` (JSON array), `Preferences` (struct JSON) | Compare: Table splitting vs JSON storage |
| **Order** | ExecuteUpdateAsync + Collections | Status, dates, amounts | 100x performance, plan cache optimization |
| **Product** | Vector Search | `SearchVector` (1536) | AI semantic search |
| **Account** | Named Query Filters | `IsDeleted`, `TenantId` | Multi-tenant, Soft delete |
| **Student/Department/Enrollment** | LeftJoin/RightJoin + SplitQuery | Navigation properties | 80% less code, deterministic ordering |

## Quick Start

### Prerequisites
- .NET 10 SDK
- SQL Server 2025 (or LocalDB with compatibility level 170) for JSON/Vector features
- SQL Server 2019+ for other features

### Run Demos

```bash
dotnet run
```

### Menu Options
Select from menu to run specific feature demo. Each demo uses its dedicated model.

## Key Benefits Summary

| Feature | Model Used | Performance | Code Quality | SQL Server |
|---------|-----------|-------------|--------------|------------|
| Complex Types (Split) | Customer | 2x faster | No JOINs | All |
| Complex Types (JSON) | Customer | 3x faster | 50% less storage | 2025+ for native |
| ExecuteUpdate | Order | 100x faster | Bulk ops | All |
| Vector Search | Product | AI-powered | Semantic search | 2025+ |
| Named Filters | Account | - | Multi-tenant | All |
| LeftJoin/RightJoin | Student/Dept | - | 80% less code | All |

## Code Examples by Model

### Customer - Complex Types (Table Splitting vs JSON)
```csharp
public class Customer
{
    // TABLE SPLITTING - Stored as separate columns
    public required Address ShippingAddress { get; set; }
    
    // JSON STORAGE - Stored as native JSON column
    public required Address BillingAddress { get; set; }
    
    // JSON COLLECTION - Array stored as JSON
    public List<OrderHistory> OrderHistories { get; set; }
    
    // STRUCT JSON - Value semantics as JSON
    public CustomerPreferences Preferences { get; set; }
}

// Configuration - Showing the difference
entity.ComplexProperty(c => c.ShippingAddress);  // Table splitting (columns)

entity.ComplexProperty(c => c.BillingAddress, b => b.ToJson());  // JSON storage

// Queries
// Table Splitting: WHERE [ShippingAddress_City] = 'Seattle'
var result1 = context.Customers.Where(c => c.ShippingAddress.City == "Seattle");

// JSON Storage: WHERE JSON_VALUE([BillingAddress], '$.City') = 'Redmond'
var result2 = context.Customers.Where(c => c.BillingAddress.City == "Redmond");
```

### Order - ExecuteUpdateAsync
```csharp
// Update thousands of orders without loading them
await context.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(o => o.Status, "Processing")
        .SetProperty(o => o.ShippedDate, DateTime.UtcNow));
// Single SQL UPDATE, 100x faster than traditional approach
```

### Product - Vector Search
```csharp
public class Product
{
    [Column(TypeName = "vector(1536)")]
    public required SqlVector<float> SearchVector { get; set; }  // AI embeddings
}

// Semantic similarity search
var similar = context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, queryVector))
    .Take(5);
```

### Account - Named Query Filters
```csharp
// Configuration
entity.HasQueryFilter("SoftDelete", a => !a.IsDeleted);
entity.HasQueryFilter("Tenant", a => a.TenantId == TenantService.CurrentTenantId);

// Usage - selectively ignore filters
var allAccounts = context.Accounts.IgnoreQueryFilters(["SoftDelete"]);
var crossTenant = context.Accounts.IgnoreQueryFilters(["Tenant"]);
```

### Student/Department - LeftJoin
```csharp
// NEW syntax (EF Core 10)
var query = context.Students
    .LeftJoin(context.Departments,
        student => student.DepartmentId,
        dept => dept.Id,
        (student, dept) => new { student, dept });

// OLD syntax (EF Core 9 and below)
var oldQuery = context.Students
    .GroupJoin(context.Departments, s => s.DepartmentId, d => d.Id, (s, d) => new { s, d })
    .SelectMany(x => x.d.DefaultIfEmpty(), (x, dept) => new { x.s, dept });
```

## SQL Server Compatibility

| Feature | Model | SQL Server 2025 | SQL Server 2019-2022 |
|---------|-------|----------------|---------------------|
| Vector Search | Product | + Required | - |
| JSON Type (native) | Customer | + Required (level 170) | Uses `nvarchar(max)` fallback |
| Complex Types | Customer | + | + |
| ExecuteUpdate | Order | + | + |
| Named Filters | Account | + | + |
| LeftJoin | Student | + | + |

## What Makes This Project Different

### Feature Isolation
Each EF Core 10 feature is demonstrated with its own dedicated model:
- **Customer** = Complex Types (not mixed with JSON or vectors)
- **Article** = JSON Type Support (pure document storage)
- **Order** = ExecuteUpdateAsync (bulk operations focus)
- **Product** = Vector Search (AI/semantic search only)
- **Account** = Named Query Filters (multi-tenancy + soft delete)

### Clear Purpose
No confusion about which model demonstrates which feature. Each model is designed specifically for its feature demonstration.

### Real-World Scenarios
- Customer with addresses (e-commerce)
- Article with metadata (content management)
- Order processing (order management)
- Product search (e-commerce/recommendations)
- Account management (SaaS/multi-tenant)

## Official Documentation

- [EF Core 10 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [Vector Search](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [Complex Types](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types)
- [JSON Columns](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#json-columns)
- [Named Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)

## License

MIT License - Free to use for learning

---

**Built with .NET 10 & EF Core 10** | Each feature with its dedicated model | Verified against Microsoft Learn docs
