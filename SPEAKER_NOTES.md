# Presentation Speaker Notes
## EF Core 10 Deep Dive - Delivery Guide

---

## ?? Presentation Overview

**Duration**: 45-60 minutes  
**Format**: Technical deep dive with live coding  
**Audience**: .NET developers familiar with EF Core basics  
**Prerequisites**: Understanding of Entity Framework fundamentals

---

## ?? Preparation Checklist

### Before the Presentation
- [ ] Clone repo and test all demos locally
- [ ] Ensure SQL Server 2025 (or compatible version) is running
- [ ] Configure Azure OpenAI credentials (or use demo mode)
- [ ] Prepare IDE with demos pre-loaded
- [ ] Test projector/screen sharing
- [ ] Have backup slides ready (PDF export)
- [ ] Clear database before starting

### Equipment Needed
- [ ] Laptop with .NET 10 SDK installed
- [ ] Visual Studio 2022 or VS Code
- [ ] SQL Server 2025 (or LocalDB)
- [ ] Projector/Screen sharing setup
- [ ] Backup demo recording (optional)

---

## ?? Delivery Tips by Section

### Slide 1-2: Introduction (3 min)
**Key Points**:
- Start with enthusiasm - EF Core 10 is a major release
- Mention .NET 10 is brand new (November 2024)
- Set expectations: "We'll see 6 game-changing features"

**Speaker Notes**:
> "Welcome everyone! Today we're diving into EF Core 10, which shipped with .NET 10 in November 2024. This isn't just an incremental update—Microsoft has added features that change how we think about data access, especially AI integration. By the end, you'll see how to make your queries 100x faster, integrate AI search, and write 80% less code."

**Engagement**:
- Quick poll: "Who's currently using EF Core?" (hands up)
- "Who's tried EF Core 7 or 8?" (context check)

---

### Slide 3-7: Complex Types (8 min)

**Demo Strategy**:
1. Show the old way (many columns cluttering Customer table)
2. Run ComplexTypesDemo - show the menu selection
3. Highlight SQL output showing column names vs JSON
4. Open SSMS and show actual table structure

**Key Talking Points**:
- "Table splitting is not new, but combining it with JSON is powerful"
- "Look at this query—EF knows to access columns vs JSON_VALUE()"
- "Use table splitting for WHERE clauses, JSON for flexibility"

**Common Questions**:
- Q: "What about EF Core migrations?"  
  A: "Fully supported! Migrations create proper columns and JSON columns"
  
- Q: "Can I query inside JSON?"  
  A: "Yes, with LINQ! `customer.BillingAddress.City == 'Seattle'` works"

**Demo Script**:
```
1. Run demo: dotnet run ? Select option 1
2. Point out console output showing both storage types
3. Show SQL generated for each query type
4. Quick SSMS check of table structure (if time permits)
```

**Timing**: 5 min demo + 3 min explanation

---

### Slide 8-10: ExecuteUpdate (6 min)

**Demo Strategy**:
1. Show OLD way first (load ? modify ? save)
2. Show query counter with hundreds of UPDATEs
3. Run ExecuteUpdateDemo
4. Highlight single UPDATE statement

**Key Talking Points**:
- "This is the #1 performance improvement in EF Core history"
- "Before: Load 1000 orders into memory. After: Zero loading"
- "Single SQL statement means one network roundtrip"

**Demo Script**:
```
1. Open ExecuteUpdateJsonDemo.cs
2. Show the old commented-out code (if you added it)
3. Run demo: dotnet run ? Select option 2
4. Highlight: "[SQL]: UPDATE [Orders] SET..."
5. Point out: "Total queries: 1 UPDATE"
```

**Red Flags to Mention**:
?? "ExecuteUpdate bypasses change tracking—use for bulk ops, not single entities"  
?? "Doesn't trigger interceptors or events"  
? "Perfect for: status updates, price changes, batch processing"

**Timing**: 3 min demo + 3 min explanation

---

### Slide 11-16: Vector Search (12 min)

**Demo Strategy**:
1. Explain embeddings briefly (don't go too deep into AI)
2. Show VectorGenerationInterceptor - automatic embedding
3. Run VectorSearchDemo with AI mode (if available)
4. Compare different distance metrics

**Key Talking Points**:
- "Vector search is how ChatGPT finds relevant context"
- "Text ? Numbers ? Similarity matching"
- "This is RAG (Retrieval-Augmented Generation) at the database level"

**Demo Script**:
```
1. Show Product model - point out [Column(TypeName = "vector(1536)")]
2. Show VectorGenerationInterceptor - automatic embedding generation
3. Run demo: dotnet run ? Select option 3
4. Search: "gaming equipment with RGB lights"
5. Show results: Wireless Gaming Mouse (98.5% similar)
6. Highlight: No "gaming" or "RGB" in search text, but found semantically!
7. Compare cosine vs euclidean results
```

**Explain Distance Metrics**:
- **Cosine**: "Angle between vectors - best for text"
  - Demo: Point in same direction = similar meaning
- **Euclidean**: "Straight-line distance - like GPS"
  - Demo: Physical distance
- **Dot Product**: "Raw similarity score"
  - Demo: Faster but less normalized

**Real-World Examples**:
> "Imagine Amazon: User searches 'laptop bag'. Vector finds 'notebook carrying case', 'computer sleeve', 'portable workstation carrier' - all semantically similar but different words"

**Common Questions**:
- Q: "Do I need OpenAI API?"  
  A: "For production, yes. Demo mode works for testing with hash-based embeddings"
  
- Q: "What about cost?"  
  A: "text-embedding-3-small is $0.02 per 1M tokens - very cheap"
  
- Q: "Can I use other embedding models?"  
  A: "Yes! Just implement the interface—works with any provider"

**Timing**: 5 min demo + 4 min explanation + 3 min Q&A

---

### Slide 17-20: Named Query Filters (7 min)

**Demo Strategy**:
1. Show the problem: Manual filtering everywhere
2. Show configuration once in OnModelCreating
3. Run NamedQueryFiltersDemo with tenant switching
4. Show IgnoreQueryFilters in action

**Key Talking Points**:
- "This prevents the #1 security bug in SaaS apps: data leakage"
- "Set it once, works everywhere - no exceptions"
- "Named filters let you selectively disable - perfect for admin views"

**Demo Script**:
```
1. Show Account model - TenantId and IsDeleted
2. Show AppDbContext - HasQueryFilter("SoftDelete", ...) configuration
3. Run demo: dotnet run ? Select option 4
4. Watch tenant switching: Tenant 1 sees 3 accounts, Tenant 2 sees 2
5. Show soft delete hiding records automatically
6. Show admin view with IgnoreQueryFilters(["SoftDelete"])
```

**Real-World Scenario**:
> "You're building a SaaS invoicing app. Tenant 1 is Acme Corp, Tenant 2 is Global Inc. With named filters, it's IMPOSSIBLE for Acme to accidentally see Global's invoices—EF adds the WHERE clause automatically to every query."

**Security Emphasis**:
- "This is defense in depth - even if developer forgets WHERE clause"
- "Better than row-level security because it's in application code"
- "Auditable - you can see which filters are active"

**Timing**: 4 min demo + 3 min explanation

---

### Slide 21-23: LeftJoin/RightJoin (5 min)

**Demo Strategy**:
1. Show old GroupJoin + SelectMany syntax (confusing)
2. Show new LeftJoin syntax (clean)
3. Run LeftJoinDemo showing students with/without departments

**Key Talking Points**:
- "This is pure developer experience improvement"
- "If you know SQL, you already know this syntax"
- "80% less code - easier to review, easier to maintain"

**Demo Script**:
```
1. Show OLD syntax in slide (comment in code)
2. Show NEW syntax - point out similarity to SQL
3. Run demo: dotnet run ? Select option 5
4. Highlight: Charlie has no department but still appears (NULL)
5. Compare with Inner Join where Charlie is excluded
```

**Before/After Comparison**:
```csharp
// BEFORE (EF Core 9)
var old = context.Students
    .GroupJoin(context.Departments,
        s => s.DepartmentId, d => d.Id,
        (s, d) => new { s, d })
    .SelectMany(x => x.d.DefaultIfEmpty(),
        (x, dept) => new { x.s, dept });
// ?? What does this do?

// AFTER (EF Core 10)
var query = context.Students
    .LeftJoin(context.Departments,
        s => s.DepartmentId,
        d => d.Id,
        (s, d) => new { s, d });
// ?? Crystal clear!
```

**Timing**: 3 min demo + 2 min explanation

---

### Slide 24-25: Parameterized Collections (4 min)

**Demo Strategy**:
1. Explain query plan cache problem
2. Show how padding works
3. Run ParameterizedCollectionsDemo
4. Compare cache stats before/after

**Key Talking Points**:
- "This is invisible optimization - EF does it automatically"
- "Powers of 2 padding: 1-4, 5-8, 9-16, etc."
- "Huge benefit for apps with varied list sizes"

**Demo Script**:
```
1. Show the problem: Different list sizes = different plans
2. Run demo: dotnet run ? Select option 6
3. Watch SQL padding: 3 items ? 4 params, 5 items ? 8 params
4. Highlight cache reuse stats
```

**Visual Aid**:
Draw on whiteboard (if available):
```
List Size ? Padded To
1, 2, 3  ? 4
4        ? 4 (no padding)
5, 6, 7, 8 ? 8
9-16     ? 16
```

**Timing**: 2 min explanation + 2 min demo

---

### Slide 26: Performance Summary (2 min)

**Presentation Tips**:
- Let the numbers speak for themselves
- Pause after "100x faster" - let it sink in
- Emphasize cumulative benefits

**Speaker Notes**:
> "Let's recap the performance wins. ExecuteUpdate: 100x faster for bulk ops. Vector Search: unlocks AI capabilities that weren't possible before. Query plan cache: 93% reuse instead of 100% misses. These aren't marginal improvements—they're game changers."

---

### Slide 27-29: Architecture & Tech Stack (3 min)

**Presentation Tips**:
- This is a breather slide - let audience relax
- Highlight clean architecture
- Mention Strategy Pattern for demo organization

**Speaker Notes**:
> "The demo project itself shows best practices. Each feature has its own model—no confusion. Strategy Pattern makes it easy to run demos individually. Interceptors handle cross-cutting concerns like logging and vector generation."

---

### Slide 30-32: Key Takeaways & Use Cases (5 min)

**Presentation Tips**:
- Make it actionable
- Tie to real projects they might work on
- Encourage experimentation

**Engagement**:
- Ask: "Who works on e-commerce?" ? Complex Types + Vector Search
- Ask: "Who builds SaaS?" ? Named Filters
- Ask: "Who does reporting?" ? LeftJoin

---

### Slide 33-34: Resources & Demo (5 min)

**Live Demo Finale**:
1. Open terminal
2. `dotnet run`
3. Select "Run ALL Demos"
4. Let it flow through all features
5. Highlight SQL output scrolling by

**Speaker Notes**:
> "Everything we've seen today is in this GitHub repo. The README has comprehensive docs, each demo is isolated, and it's MIT licensed—use it in your projects. Let's run all demos together now."

---

### Slide 35: Q&A (10+ min)

**Anticipated Questions & Answers**:

**Q: When should I upgrade to EF Core 10?**  
A: "If you're on .NET 10, immediately. If on .NET 8, evaluate based on these features—especially vector search and bulk operations."

**Q: Is vector search production-ready?**  
A: "Yes, but SQL Server 2025 is required. If you're on older SQL Server, plan your upgrade path."

**Q: Can I mix table splitting and JSON in same entity?**  
A: "Absolutely! That's the demo—ShippingAddress as columns, BillingAddress as JSON."

**Q: Does ExecuteUpdate work with transactions?**  
A: "Yes, full transaction support. Use `context.Database.BeginTransaction()`."

**Q: What's the learning curve?**  
A: "Minimal! These are EF Core enhancements, not replacements. Your existing code still works."

**Q: Performance impact of named filters?**  
A: "Negligible—it's just a WHERE clause. SQL Server optimizes it well."

**Q: Can I use this in Blazor/ASP.NET Core?**  
A: "Yes! These are DbContext features—framework-agnostic."

---

## ?? Demo Troubleshooting

### Common Issues

**Issue**: Vector search demo fails with 404  
**Fix**: Check AIConfiguration.cs - endpoint URL must be `https://{resource}.openai.azure.com/openai/v1`

**Issue**: SQL Server doesn't support vector type  
**Fix**: Ensure SQL Server 2025 or compatibility level 170. Falls back to `nvarchar(max)` on older versions.

**Issue**: Demo runs but no SQL output  
**Fix**: Check `QueryCounterInterceptor.ShowSqlInConsole = true` in DemoBase

**Issue**: Named filters don't work  
**Fix**: Ensure `TenantService.CurrentTenantId` is set before queries

---

## ?? Timing Guide

| Section | Time | Cumulative |
|---------|------|------------|
| Introduction | 3 min | 3 min |
| Complex Types | 8 min | 11 min |
| ExecuteUpdate | 6 min | 17 min |
| Vector Search | 12 min | 29 min |
| Named Filters | 7 min | 36 min |
| LeftJoin | 5 min | 41 min |
| Parameterized Collections | 4 min | 45 min |
| Summary & Architecture | 5 min | 50 min |
| Live Demo Finale | 5 min | 55 min |
| Q&A | 10 min | 65 min |

**Total**: 60-65 minutes with Q&A

---

## ?? Key Messages to Emphasize

### Top 3 Takeaways for Audience

1. **EF Core 10 enables AI-powered data access** (Vector Search)
2. **100x performance improvements are real** (ExecuteUpdate)
3. **Less code, better security** (LeftJoin, Named Filters)

### Closing Statement

> "EF Core 10 isn't just an ORM update—it's Microsoft recognizing where data access is heading: AI integration, performance at scale, and developer productivity. The repo is yours to explore. Star it, fork it, use it in production. Thank you!"

---

## ?? Post-Presentation Checklist

- [ ] Share slide deck link
- [ ] Share GitHub repo link
- [ ] Provide contact info for follow-up
- [ ] Collect feedback
- [ ] Answer lingering questions
- [ ] Share recording link (if recorded)

---

## ?? Quick Links for Audience

**Share these at the end**:

- **Repo**: https://github.com/AkhmedovSanjar/EFCore10DeepDive
- **Microsoft Docs**: https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/whatsnew
- **Vector Search Guide**: https://learn.microsoft.com/ef/core/providers/sql-server/vector-search
- **.NET 10 Download**: https://dotnet.microsoft.com/download/dotnet/10.0

---

**Good luck with your presentation! ??**
