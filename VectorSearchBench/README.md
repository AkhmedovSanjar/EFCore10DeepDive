# Vector Search vs WHERE Clause Benchmark

## What This Benchmarks

Compares three query approaches for finding "gaming RGB accessories":

1. **WHERE Clause (Category + Keywords)** - Traditional SQL filtering
2. **Vector Search (Semantic Similarity)** - AI-powered semantic search
3. **Hybrid (WHERE + Vector)** - Best of both worlds
4. **WHERE Clause (Category Only)** - Baseline comparison

## Benchmark Details

- **Dataset**: 10,000 products across 8 categories
- **Search Query**: "gaming RGB accessories with lights"
- **Result Set**: Top 100 matches
- **Database**: SQL Server 2025 with vector support

## The Approaches

### 1. WHERE Clause (Category + Keywords)
```csharp
// Traditional filtering
context.Products
    .Where(p => p.Category == "Gaming Accessories" &&
               (p.Name.Contains("RGB") || 
                p.Description.Contains("RGB") ||
                p.Name.Contains("gaming") ||
                p.Description.Contains("gaming")))
    .Take(100)
```

**Generated SQL:**
```sql
WHERE [Category] = 'Gaming Accessories' 
  AND ([Name] LIKE '%RGB%' OR [Description] LIKE '%RGB%' 
    OR [Name] LIKE '%gaming%' OR [Description] LIKE '%gaming%')
```

**Pros:**
- Fast with proper indexes
- Exact keyword matching
- Predictable results

**Cons:**
- Misses synonyms ("illuminated" vs "RGB")
- No semantic understanding
- Requires exact keyword match

### 2. Vector Search (Semantic Similarity)
```csharp
// AI-powered semantic search
context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, queryVector))
    .Take(100)
```

**Generated SQL:**
```sql
SELECT TOP(100) *
FROM [Products]
ORDER BY VECTOR_DISTANCE('cosine', [SearchVector], @queryVector)
```

**Pros:**
- Finds semantically similar items
- Works with synonyms
- Understands context and meaning
- No need for exact keywords

**Cons:**
- Slower without vector index (full table scan)
- Requires vector embeddings
- Approximate results

### 3. Hybrid (WHERE + Vector)
```csharp
// Combine filtering + semantic ranking
context.Products
    .Where(p => p.Category == "Gaming Accessories")
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, queryVector))
    .Take(100)
```

**Generated SQL:**
```sql
SELECT TOP(100) *
FROM [Products]
WHERE [Category] = 'Gaming Accessories'
ORDER BY VECTOR_DISTANCE('cosine', [SearchVector], @queryVector)
```

**Pros:**
- Fast pre-filtering reduces search space
- Semantic ranking within filtered set
- Best accuracy and performance balance

**Cons:**
- Still requires vector calculations
- More complex query

## Expected Results

### Without Vector Index

| Method | Mean | Allocated | Result Quality |
|--------|------|-----------|----------------|
| WHERE Clause (Category Only) | ~15 ms | ~50 KB | All gaming products |
| WHERE (Category + Keywords) | ~25 ms | ~55 KB | Keyword matches only |
| Vector Search (Semantic) | ~150 ms | ~200 KB | Semantically similar |
| Hybrid (WHERE + Vector) | ~80 ms | ~120 KB | Best of both |

### With Vector Index (SQL Server 2025)

```sql
-- Create vector index for faster ANN search
CREATE VECTOR INDEX IX_Products_SearchVector 
ON Products(SearchVector)
WITH (METRIC = 'cosine');
```

| Method | Mean | Allocated | Improvement |
|--------|------|-----------|-------------|
| WHERE Clause (Category Only) | ~15 ms | ~50 KB | Baseline |
| WHERE (Category + Keywords) | ~25 ms | ~55 KB | No change |
| Vector Search (Semantic) | ~30 ms | ~200 KB | **5x faster** |
| Hybrid (WHERE + Vector) | ~20 ms | ~120 KB | **4x faster** |

## When to Use Each Approach

### Use WHERE Clause When:
- Exact keyword matching is required
- Simple categorical filtering
- Performance is critical
- Results must be deterministic
- No AI infrastructure available

**Examples:**
- Filter by price range
- Filter by exact category
- Status-based queries

### Use Vector Search When:
- Semantic understanding matters
- User queries are natural language
- Need to find "similar" items
- Keyword variations are common
- Building recommendation systems

**Examples:**
- "Find products like this one"
- "Comfortable office chairs" ? "Ergonomic seating"
- Multi-language search
- Content recommendations

### Use Hybrid Approach When:
- Need both filtering AND ranking
- Large datasets require pre-filtering
- Want best of both worlds
- Performance AND accuracy matter

**Examples:**
- E-commerce: Filter by category, rank by relevance
- Documentation: Filter by product, rank by similarity
- Job search: Filter by location, rank by fit

## Running the Benchmark

```bash
cd VectorSearchBench
dotnet run -c Release
```

**Important:** Must run in Release mode for accurate results!

## Sample Output

```
Seeding database with 10,000 products...
Inserted 10000 products...
Setup complete: 10000 products inserted
Search query: 'gaming RGB accessories with lights'

| Method                           | Mean      | Rank | Allocated |
|--------------------------------- |----------:|-----:|----------:|
| WHERE Clause (Category Only)     |  14.82 ms |    1 |  51.2 KB  |
| WHERE (Category + Keywords)      |  23.45 ms |    2 |  54.8 KB  |
| Hybrid (WHERE + Vector)          |  78.33 ms |    3 | 118.5 KB  |
| Vector Search (Semantic)         | 142.67 ms |    4 | 195.2 KB  |
```

## Key Insights

### 1. Performance Trade-offs
- **WHERE Clause**: Fastest, but limited to exact matches
- **Vector Search**: Slowest (without index), but finds semantic matches
- **Hybrid**: Good balance for production use

### 2. Result Quality Comparison

#### Search Query: "gaming RGB accessories"

**WHERE Clause Results:**
```
- "Gaming Mouse RGB" ? (exact match)
- "RGB Gaming Keyboard" ? (exact match)
- "Gaming Headset" ? (missing - no RGB in name)
```

**Vector Search Results:**
```
- "Gaming Mouse RGB" ? (semantically similar)
- "RGB Gaming Keyboard" ? (semantically similar)
- "Gaming Headset with LED" ? (found - understands LED = lighting)
- "Illuminated Gaming Controller" ? (found - understands illuminated ? RGB)
```

### 3. Production Recommendations

**For E-Commerce:**
```csharp
// Use hybrid: Category filter + vector ranking
var results = context.Products
    .Where(p => p.Category == userSelectedCategory && p.Price <= maxPrice)
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, searchVector))
    .Take(20);
```

**For Search Engines:**
```csharp
// Use pure vector search for best semantic matching
var results = context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.SearchVector, searchVector))
    .Take(50);
```

**For Filters:**
```csharp
// Use WHERE clause for exact matching
var results = context.Products
    .Where(p => p.Category == category && p.InStock && p.Price < 100)
    .Take(100);
```

## Optimization Tips

### 1. Create Vector Index (SQL Server 2025)
```sql
-- Enable preview features
ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;

-- Create DiskANN vector index
CREATE VECTOR INDEX IX_Products_SearchVector 
ON Products(SearchVector)
WITH (METRIC = 'cosine');
```

### 2. Add Column Indexes
```csharp
modelBuilder.Entity<Product>()
    .HasIndex(p => p.Category);

modelBuilder.Entity<Product>()
    .HasIndex(p => p.Price);
```

### 3. Cache Query Vectors
```csharp
// Don't regenerate vector on every query
private static readonly Dictionary<string, SqlVector<float>> _vectorCache = new();

public SqlVector<float> GetOrCreateVector(string query)
{
    if (!_vectorCache.TryGetValue(query, out var vector))
    {
        vector = _embeddingService.GenerateEmbedding(query);
        _vectorCache[query] = vector;
    }
    return vector;
}
```

## Conclusion

- **WHERE Clause**: Best for exact matches, categorical filtering
- **Vector Search**: Best for semantic understanding, recommendations
- **Hybrid**: Best for production - combines speed and relevance
- **Vector Index**: Essential for production vector search performance

Choose based on your use case, dataset size, and accuracy requirements!

## Related Resources

- [EF Core Vector Search Docs](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [SQL Server Vector Index](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql)
- [Main Project](../README.md)
