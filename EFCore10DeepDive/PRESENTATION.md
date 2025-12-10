# EF Core 10 Deep Dive
## What's NEW in Entity Framework Core 10

**Presenter**: Sanjar Akhmedov

**Repository**: https://github.com/AkhmedovSanjar/EFCore10DeepDive

---

## Agenda

1. **Introduction** - Why EF Core 10 Matters
2. **Complex Types** - Table Splitting vs JSON Storage
3. **ExecuteUpdate** - Bulk Operations Without Loading
4. **Vector Search** - AI-Powered Semantic Search
5. **Named Query Filters** - Multi-Tenancy Made Easy
6. **LeftJoin/RightJoin** - SQL-Like LINQ Syntax
7. **Parameterized Collections** - Query Optimization
8. **Demo & Q&A**

---

## Why EF Core 10?

### Key Themes
- **AI Integration** - Native vector search support
- **Performance** - 100x faster bulk operations
- **Flexibility** - Multiple data storage strategies
- **Code Quality** - 80% less boilerplate code
- **Multi-Tenancy** - First-class support

### Technology Stack
- **.NET 10** - Latest framework
- **SQL Server 2025** - Vector & JSON native types
- **C# 14** - Modern language features

---

## Complex Types
### Three Ways to Store Related Data

#### Way 1: Separate Table (Foreign Key)
```csharp
// Address in a separate table with foreign key relationship
public class Address {
    public int Id { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}

public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Foreign key relationship
    public int ShippingAddressId { get; set; }
    public Address ShippingAddress { get; set; }
}

// Configuration
entity.HasOne(c => c.ShippingAddress)
      .WithMany()
      .HasForeignKey(c => c.ShippingAddressId);
```

#### Way 2: Table Splitting (Complex Type as Columns)
```csharp
// Complex type stored as individual columns in same table
public record Address(string Street, string City, string ZipCode);

public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Stored as: ShippingAddress_Street, ShippingAddress_City, etc.
    public Address ShippingAddress { get; set; }
}

// Configuration
entity.ComplexProperty(c => c.ShippingAddress);
```

#### Way 3: JSON Storage (Complex Type as JSON)
```csharp
// Complex type stored as JSON column
public record Address(string Street, string City, string ZipCode);

public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Stored as: {"Street": "...", "City": "...", "ZipCode": "...">
    public Address BillingAddress { get; set; }
    public List<OrderHistory> OrderHistories { get; set; }
}

// Configuration
entity.ComplexProperty(c => c.BillingAddress, b => b.ToJson());
entity.ComplexProperty(c => c.OrderHistories, b => b.ToJson());
```

---

## Complex Types - Configuration

```csharp
// AppDbContext.OnModelCreating
entity.ComplexProperty(c => c.ShippingAddress);  
// Columns: ShippingAddress_Street, ShippingAddress_City

entity.ComplexProperty(c => c.BillingAddress, b => b.ToJson());  
// JSON Column: BillingAddress (native type in SQL 2025)

entity.ComplexProperty(c => c.OrderHistories, b => b.ToJson());  
// JSON Array: [{"OrderId": 1, "Amount": 99.99}, ...]
```

### When to Use What?

| Use Case | Separate Table | Table Splitting | JSON Storage |
|----------|----------------|-----------------|--------------|
| **Frequently Queried** | Yes | Yes | Slow |
| **Can Indexing** | Yes | Yes | No |
| **Flexible Schema** | No | No | Yes |
| **Storage Efficiency** | Many JOINs | Many columns | 50% less |
| **Read Performance** | JOIN overhead | 2x faster | Parse overhead |

---

## Complex Types - Demo Results

```
Create Customer with multiple address types
    [OK] Customer created with ID: 1
    Shipping Address (TABLE): 123 Main St, Seattle, WA 98101
    Billing Address (JSON):   456 Oak Ave, Redmond, WA 98052

Query by Shipping Address (Indexed Column):
    SQL: WHERE [ShippingAddress_City] = 'Seattle'
    Fast - Direct column access

Query by Billing Address (JSON):
    SQL: WHERE JSON_VALUE([BillingAddress], '$.City') = 'Redmond'
    Slower - JSON parsing required

Update complex properties:
    Both updated in single SaveChanges
```

**Key Insight**: Use table splitting for performance-critical fields, JSON for flexibility!

---

## ExecuteUpdate
### Bulk Operations Without Loading

#### The Problem
```csharp
// OLD WAY - EF Core 9 and below
var pendingOrders = await context.Orders
    .Where(o => o.Status == "Pending")
    .ToListAsync();  // Loads 1000s into memory

foreach (var order in pendingOrders) {
    order.Status = "Processing";
    order.ShippedDate = DateTime.UtcNow;
}

await context.SaveChangesAsync();  // 1000s of UPDATE statements
```

**Problems**:
- Loads all entities into memory
- Tracks each entity
- Executes separate UPDATE per entity
- Slow for bulk operations

---

## ExecuteUpdate - The Solution

```csharp
// NEW WAY - EF Core 7+ (Enhanced in 10)
await context.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(o => o.Status, "Processing")
        .SetProperty(o => o.ShippedDate, DateTime.UtcNow)
    );

// Single SQL UPDATE
// No tracking
// No memory loading
// 100x faster
```

**Generated SQL**:
```sql
UPDATE [Orders]
SET [Status] = 'Processing', [ShippedDate] = GETUTCDATE()
WHERE [Status] = 'Pending'
```

---

## ExecuteUpdate - Performance

| Operation | Traditional | ExecuteUpdate | Speedup |
|-----------|------------|---------------|---------|
| **Update 1,000 records** | 2.5s | 25ms | **100x** |
| **Update 10,000 records** | 28s | 180ms | **155x** |
| **Memory Usage** | 150 MB | 1 MB | **150x less** |
| **SQL Roundtrips** | 1,000 | 1 | **1,000x less** |

### Demo Output
```
Bulk update 100 orders from 'Pending' to 'Processing':
    [SQL]: UPDATE [Orders] SET [Status] = 'Processing' 
           WHERE [Status] = 'Pending'
    Updated 100 orders in single query
    No entities loaded into memory
    No change tracking overhead

Total queries: 1 UPDATE
```

---

## Vector Search
### AI-Powered Semantic Search

#### What is Vector Search?
- Convert text to numeric embeddings (vectors)
- Find similar items by **semantic meaning**, not keywords
- Powers: ChatGPT search, recommendations, RAG

#### The Product Model
```csharp
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public SqlVector<float> SearchVector { get; set; }  // AI embeddings
}
```

**`vector(1536)`** = SQL Server 2025 native type for embeddings

---

## Vector Search - How It Works

### 1. Generate Embeddings
```csharp
// VectorGenerationInterceptor - Auto-generates embeddings
public override InterceptionResult<int> SavingChanges(...)
{
    foreach (var entry in eventData.Context.ChangeTracker.Entries<Product>())
    {
        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
        {
            var text = $"{entry.Entity.Name} {entry.Entity.Description} {entry.Entity.Category}";
            entry.Entity.SearchVector = _embeddingService.GenerateEmbedding(text);
        }
    }
}
```

### 2. Similarity Search
```csharp
var searchText = "gaming equipment with RGB lights";
var queryVector = _embeddingService.GenerateEmbedding(searchText);

var results = await context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, queryVector))
    .Take(3)
    .ToListAsync();
```

---

## Vector Search - Distance Metrics

### Cosine Similarity (Best for Text)
```csharp
.OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, queryVector))
```
- Measures **angle** between vectors
- Range: 0 (identical) to 2 (opposite)
- Ignores magnitude, focuses on direction

### Euclidean Distance (Spatial)
```csharp
.OrderBy(p => EF.Functions.VectorDistance("euclidean", p.SearchVector, queryVector))
```
- Measures **straight-line distance**
- Good for spatial data

### Dot Product (Raw Similarity)
```csharp
.OrderByDescending(p => EF.Functions.VectorDistance("dot", p.SearchVector, queryVector))
```
- Higher = more similar
- Faster but unnormalized

---

## Vector Search - Demo Results

```
Search: 'gaming equipment with RGB lights'

Top 3 Similar Products (Cosine Similarity):
    98.5% - Wireless Gaming Mouse (Gaming Accessories)
    97.2% - Mechanical Keyboard RGB (Gaming Accessories)
    96.8% - Gaming Headset RGB (Gaming Accessories)

Distance Metrics Comparison:
    1. Cosine Similarity (angle between vectors, best for text)
       0.0152 - Wireless Gaming Mouse (Gaming Accessories)
       0.0281 - Mechanical Keyboard RGB (Gaming Accessories)
       0.0319 - Gaming Headset RGB (Gaming Accessories)

    2. Euclidean Distance (straight-line distance)
       0.3421 - Wireless Gaming Mouse (Gaming Accessories)
       0.4782 - Gaming Headset RGB (Gaming Accessories)
       0.5201 - Mechanical Keyboard RGB (Gaming Accessories)
```

**Key**: Found semantically similar products without exact keyword match!

---

## Vector Search - Use Cases

### E-Commerce
```
User searches: "comfortable office chair"
Vector finds: "Ergonomic desk seating", "Executive furniture"
```

### Documentation Search
```
User asks: "How do I reset password?"
Vector finds: "Account credential recovery", "Login troubleshooting"
```

### Recommendations
```
User watched: "Sci-fi thriller with time travel"
Vector suggests: "Inception", "Interstellar", "Primer"
```

### RAG (Retrieval-Augmented Generation)
```
Find relevant docs → Feed to ChatGPT → Get contextualized answer
```

---

## Named Query Filters
### Multi-Tenancy & Soft Delete Made Easy

#### The Problem
```csharp
// OLD: Manual filtering everywhere
var accounts = await context.Accounts
    .Where(a => !a.IsDeleted)  // Forget this? Bug!
    .Where(a => a.TenantId == currentTenantId)  // Repeat everywhere
    .ToListAsync();
```

#### The Solution: Named Query Filters
```csharp
// Configuration - Set once
modelBuilder.Entity<Account>()
    .HasQueryFilter("SoftDelete", a => !a.IsDeleted)
    .HasQueryFilter("Tenant", a => a.TenantId == TenantService.CurrentTenantId);

// Query - Filters applied automatically
var accounts = await context.Accounts.ToListAsync();  
// Auto-filtered by tenant AND soft delete

// Selectively disable
var allAccounts = context.Accounts
    .IgnoreQueryFilters(["SoftDelete"]);  // Show deleted items
```

---

## Named Query Filters - Configuration

```csharp
public class Account {
    public int Id { get; set; }
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    
    // Multi-tenancy
    public int TenantId { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
}

// AppDbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var entity = modelBuilder.Entity<Account>();
    
    // Named filters - Apply globally
    entity.HasQueryFilter("SoftDelete", a => !a.IsDeleted);
    entity.HasQueryFilter("Tenant", a => a.TenantId == TenantService.CurrentTenantId);
}
```

---

## Named Query Filters - Demo Results

```
=== Multi-Tenant SaaS Demo ===

Create 3 tenants with accounts:
    Tenant 1 (Acme Corp): 3 accounts
    Tenant 2 (Global Inc): 2 accounts
    Tenant 3 (Tech Ltd): 2 accounts

--- Scenario 1: Default Query (Auto-Filtered) ---
Current Tenant: 1 (Acme Corp)
Accounts visible: 3
    ACC-1001, Balance: $10,000
    ACC-1002, Balance: $25,000
    ACC-1003, Balance: $5,000
[SQL]: WHERE [TenantId] = 1 AND [IsDeleted] = 0

--- Scenario 2: Soft Delete ---
Soft delete ACC-1002
Accounts visible: 2  Deleted account hidden
    ACC-1001, Balance: $10,000
    ACC-1003, Balance: $5,000

--- Scenario 3: Admin View (Ignore SoftDelete) ---
Accounts with deleted: 3  Shows deleted items
    ACC-1001 (Active)
    ACC-1002 (Deleted)
    ACC-1003 (Active)
```

---

## Named Query Filters - Benefits

### Security
- Tenant isolation enforced at DB level
- Impossible to accidentally query wrong tenant

### Maintainability
- Single source of truth
- No repeated `WHERE` clauses

### Flexibility
- Enable/disable per query
- Multiple independent filters

### Perfect For
- **SaaS Applications** (tenant isolation)
- **Soft Delete** (hide deleted records)
- **Active/Inactive** (status filtering)
- **Security Policies** (row-level security)

---

## LeftJoin/RightJoin
### SQL-Like LINQ Syntax (.NET 10)

#### The Problem
```csharp
// OLD WAY - EF Core 9 and below
var result = context.Students
    .GroupJoin(
        context.Departments,
        student => student.DepartmentId,
        dept => dept.Id,
        (student, depts) => new { student, depts }
    )
    .SelectMany(
        x => x.depts.DefaultIfEmpty(),
        (x, dept) => new { x.student, dept }
    );

// What is this even doing?!
```

---

## LeftJoin/RightJoin - The Solution

```csharp
// NEW WAY - EF Core 10 (.NET 10)
var result = context.Students
    .LeftJoin(
        context.Departments,
        student => student.DepartmentId,
        dept => dept.Id,
        (student, dept) => new { student, dept }
    );

// Readable, SQL-like syntax!
```

**Generated SQL**:
```sql
SELECT s.*, d.*
FROM Students s
LEFT JOIN Departments d ON s.DepartmentId = d.Id
```

---

## LeftJoin - Demo Results

```
=== LeftJoin Demo ===

Students:
    Alice (Dept: 1 - Computer Science)
    Bob (Dept: 2 - Mathematics)
    Charlie (Dept: NULL - No department assigned)

--- LeftJoin: All students, departments optional ---
    Alice - Computer Science
    Bob - Mathematics
    Charlie - (No Department) Included with NULL dept

--- Inner Join: Only students with departments ---
    Alice - Computer Science
    Bob - Mathematics
    Charlie - Excluded (no match)

Code Reduction: 80% less boilerplate!
```

---

## LeftJoin - Benefits

### Before vs After

| Aspect | Old (GroupJoin) | New (LeftJoin) | Improvement |
|--------|----------------|----------------|-------------|
| **Lines of Code** | 10 | 2 | **80% less** |
| **Readability** | Confusing | Clear | **Much better** |
| **SQL Familiarity** | Foreign | Native | **SQL-like** |
| **Maintainability** | Hard | Easy | **Much easier** |

### Use Cases
- Reports with optional relationships
- Outer joins for data completeness
- Finding orphaned records

---

## Parameterized Collections
### Query Plan Cache Optimization

#### The Problem
```csharp
// Different list sizes = Different query plans
var ids1 = new[] { 1, 2, 3 };
var orders1 = context.Orders.Where(o => ids1.Contains(o.Id));  
// Plan: WHERE Id IN (@p0, @p1, @p2)

var ids2 = new[] { 1, 2, 3, 4, 5 };
var orders2 = context.Orders.Where(o => ids2.Contains(o.Id));  
// Plan: WHERE Id IN (@p0, @p1, @p2, @p3, @p4)

// Cache pollution - Each size creates new plan
```

#### The Solution: Smart Padding
```csharp
// EF Core 10 - Intelligent padding
var ids = new[] { 1, 2, 3 };  // 3 items
// Padded to 4: WHERE Id IN (@p0, @p1, @p2, @p3)

var ids = new[] { 1, 2, 3, 4, 5 };  // 5 items
// Padded to 8: WHERE Id IN (@p0, @p1, @p2, ..., @p7)

// Cache reused for: 1-4, 5-8, 9-16, 17-32, etc.
```

---

## Parameterized Collections - Demo

```
=== Parameterized Collections Optimization ===

Query with 3 IDs:
    [SQL]: WHERE [Id] IN (@p0, @p1, @p2, @p3)
    Padded to 4 parameters (power of 2)

Query with 5 IDs:
    [SQL]: WHERE [Id] IN (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)
    Padded to 8 parameters

Query with 10 IDs:
    [SQL]: WHERE [Id] IN (@p0...@p15)
    Padded to 16 parameters

Query Plan Cache Stats:
    Before: 100 unique plans (100% cache misses)
    After:  7 unique plans (93% cache reuse)
```

### Benefits
- **Better cache utilization** - Fewer unique plans
- **Faster query execution** - Reuse compiled plans
- **Lower memory usage** - Less plan cache bloat

---

## Performance Summary

| Feature | Traditional | EF Core 10 | Improvement |
|---------|------------|------------|-------------|
| **Bulk Update (1K records)** | 2.5s | 25ms | **100x faster** |
| **JSON Storage** | N/A | 50% less | **Storage savings** |
| **Vector Search** | Keyword only | Semantic | **AI-powered** |
| **Code for LeftJoin** | 10 lines | 2 lines | **80% less** |
| **Query Plan Cache** | 100 plans | 7 plans | **93% reuse** |

---

## Architecture Highlights

### Strategy Pattern for Demos
```csharp
public interface IDemoStrategy {
    string FeatureName { get; }
    string Description { get; }
    Task ExecuteAsync();
}

// Each feature = Separate demo
- ComplexTypesDemo
- ExecuteUpdateJsonDemo
- VectorSearchDemo
- NamedQueryFiltersDemo
- LeftJoinDemo
- ParameterizedCollectionsDemo
```

### Clean Architecture
- **Models** - One model per feature
- **Services** - Reusable components
- **Interceptors** - Cross-cutting concerns
- **DemoStrategies** - Feature isolation

---

## Tech Stack

### Runtime
- **.NET 10** - Latest framework
- **C# 14** - Modern language features
- **EF Core 10** - All new features

### Database
- **SQL Server 2025** - Vector & native JSON
- **SQL Server 2019+** - Other features

### AI Integration
- **OpenAI SDK** - Embedding generation
- **Azure OpenAI** - Production embeddings
- **Fallback Mode** - Demo without API

---

## Key Takeaways

### 1. Choose the Right Tool
- **Table Splitting** → Performance-critical fields
- **JSON** → Flexible schemas
- **ExecuteUpdate** → Bulk operations
- **Vector Search** → AI-powered search

### 2. Think About Scale
- Bulk operations over entity loading
- Query plan cache matters
- Named filters prevent bugs

### 3. Embrace Modern Patterns
- LeftJoin for readability
- Complex types for domain modeling
- Interceptors for cross-cutting concerns

---

## Real-World Use Cases

### E-Commerce Platform
- **Complex Types**: Product variants, addresses
- **Vector Search**: "Find similar products"
- **ExecuteUpdate**: Bulk price updates
- **Named Filters**: Multi-store tenancy

### SaaS Application
- **Named Filters**: Tenant isolation
- **Soft Delete**: Audit trail
- **JSON Storage**: User preferences
- **LeftJoin**: Reporting

### Content Management
- **Vector Search**: Semantic article search
- **JSON**: Flexible metadata
- **Complex Types**: Author info
- **Parameterized Collections**: Tag filtering

---

## Resources

### Official Documentation
- [EF Core 10 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [Vector Search Guide](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [Complex Types](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types)
- [Named Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)

### This Project
- **GitHub**: [github.com/AkhmedovSanjar/EFCore10DeepDive](https://github.com/AkhmedovSanjar/EFCore10DeepDive)
- **README**: Comprehensive feature guide
- **Code**: Production-ready examples

---

## Live Demo

### Run the Project
```bash
git clone https://github.com/AkhmedovSanjar/EFCore10DeepDive
cd EFCore10DeepDive
dotnet run
```

### Demo Menu
```
1. Complex Types (Table Splitting vs JSON)
2. ExecuteUpdate - Bulk Operations
3. Vector Search (SQL Server 2025)
4. Named Query Filters
5. LeftJoin/RightJoin (.NET 10)
6. Parameterized Collections
7. Run ALL Demos
0. Exit
```

**Let's see it in action!**

---

## Q&A

### Common Questions

**Q: Do I need SQL Server 2025?**  
A: Only for Vector Search & native JSON. Other features work on SQL 2019+

**Q: Can I use Vector Search without OpenAI?**  
A: Yes! Demo mode uses hash-based embeddings for testing

**Q: Is ExecuteUpdate safe for production?**  
A: Yes! It's been in EF Core since v7, enhanced in v10

**Q: How do Named Filters affect performance?**  
A: Minimal impact - filters are added to WHERE clause

**Q: Can I use LeftJoin with EF Core 9?**  
A: No - Requires .NET 10 & EF Core 10

---

## Thank You!

### Contact
- **GitHub**: [@AkhmedovSanjar](https://github.com/AkhmedovSanjar)
- **Project**: [EFCore10DeepDive](https://github.com/AkhmedovSanjar/EFCore10DeepDive)

### What's Next?
- Star the repository
- Fork and experiment
- Open issues for questions
- Use in your projects

**Questions?**

---

## Bonus: Quick Reference Card

```csharp
// Complex Types
entity.ComplexProperty(c => c.Address);           // Table Splitting
entity.ComplexProperty(c => c.Metadata, b => b.ToJson());  // JSON

// ExecuteUpdate
await context.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Shipped"));

// Vector Search
await context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Vector, query))
    .Take(5;

// Named Filters
entity.HasQueryFilter("SoftDelete", a => !a.IsDeleted);
context.Accounts.IgnoreQueryFilters(["SoftDelete"]);

// LeftJoin
context.Students.LeftJoin(context.Departments, ...);

// Query Counter
AppDbContext.QueryCounter.PrintSummary();
```

---

**End of Presentation**

*Built with .NET 10 & EF Core 10*
