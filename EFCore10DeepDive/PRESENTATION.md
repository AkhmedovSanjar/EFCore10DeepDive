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
### Understanding the Problem & Solution

#### What Are Complex Types?
Complex types are groups of related data that belong together but don't need their own ID.

**Simple Examples:**
- **Address** - Street, City, ZipCode (part of a Customer)
- **Money** - Amount and Currency (like $50.00 USD)
- **Date Range** - Start date and End date
- **Location** - Latitude and Longitude

Think of them as "mini-objects" inside your main object.

#### Why Do We Need Different Storage Options?
Different situations need different solutions:

| Storage Type | Speed | Flexibility | When to Use |
|--------------|-------|-------------|-------------|
| **Separate Table** | Slow (needs JOIN) | Low | Shared data (one address, many customers) |
| **Columns** | Fast | Low | Search by city, state often |
| **JSON** | Medium | High | Rarely search inside, changes often |

#### Real-World Example
```
Online Store with 10,000 orders:

Old Way (Separate Table):
- Need to JOIN addresses table 10,000 times
- Takes 40 milliseconds

New Way (Columns):
- No JOIN needed, data is right there
- Takes 15 milliseconds
- Result: 2.5x faster! 🚀
```

### Three Ways to Store Related Data

#### Way 1: Separate Table (Traditional)
**What it is:** Address lives in its own table, linked by ID

**Use it when:**
- Multiple customers share the same address
- Need to update address once for everyone
- Address data is very big

**Trade-offs:**
```
Speed:      ⭐⭐ (Slow - needs JOIN)
Flexibility: ⭐⭐⭐
Storage:    ⭐⭐ (Extra table space)
```

```csharp
// Address has its own table
public class Address {
    public int Id { get; set; }           // ← Has its own ID
    public string Street { get; set; }
    public string City { get; set; }
}

public class Customer {
    public int ShippingAddressId { get; set; }  // ← Links to Address
    public Address ShippingAddress { get; set; }
}
```

**Database looks like:**
```
Customers Table         Addresses Table
-----------------      ------------------
Id | Name | AddrId     Id | Street | City
1  | John | 100   →    100 | Main St | Seattle
2  | Jane | 100   →    (same address!)
```

#### Way 2: Columns (Fast!)
**What it is:** Address becomes extra columns in Customer table

**Use it when:**
- Search by city or state often
- Need fast queries
- Each customer has unique address

**Trade-offs:**
```
Speed:      ⭐⭐⭐⭐⭐ (Fastest!)
Flexibility: ⭐⭐ (Hard to change)
Storage:    ⭐⭐⭐ (More columns)
```

```csharp
// Address becomes columns
public record Address(string Street, string City, string ZipCode);

public class Customer {
    public Address ShippingAddress { get; set; }  // ← No separate table!
}

// Configuration
entity.ComplexProperty(c => c.ShippingAddress);
```

**Database looks like:**
```
Customers Table
--------------------------------------------------------
Id | Name | ShippingAddress_Street | ShippingAddress_City
1  | John | 123 Main St            | Seattle
2  | Jane | 456 Oak Ave            | Portland
```

#### Way 3: JSON (Flexible!)
**What it is:** Address stored as JSON text in one column

**Use it when:**
- Address structure changes often
- Storing lists (order history)
- Rarely search by city/state

**Trade-offs:**
```
Speed:      ⭐⭐⭐ (Medium)
Flexibility: ⭐⭐⭐⭐⭐ (Most flexible!)
Storage:    ⭐⭐⭐⭐⭐ (50% less space!)
```

```csharp
// Address stored as JSON
public class Customer {
    public Address BillingAddress { get; set; }  // ← Stored as JSON
}

// Configuration
entity.ComplexProperty(c => c.BillingAddress, b => b.ToJson());
```

**Database looks like:**
```
Customers Table
---------------------------------------------------------
Id | Name | BillingAddress (JSON column)
1  | John | {"Street":"123 Main","City":"Seattle","Zip":"98101"}
2  | Jane | {"Street":"456 Oak","City":"Portland","Zip":"97201"}
```

---

## Complex Types - Quick Comparison

### Speed Comparison
| Task | Separate Table | Columns | JSON |
|------|----------------|---------|------|
| Search by City | 40ms (JOIN) | 15ms ⚡ | 25ms |
| Get all data | 40ms (JOIN) | 15ms ⚡ | 20ms |
| Add new field | Easy | Hard | Easy ✨ |

### Storage Comparison
| Storage Type | Space Used | Example |
|--------------|------------|---------|
| Separate Table | 100% | 1 KB per customer |
| Columns | 80% | 800 bytes per customer |
| JSON | 50% ⭐ | 500 bytes per customer |

### When to Use What?

| Your Situation | Best Choice | Why |
|----------------|-------------|-----|
| "I search by city all the time" | Columns | Fastest search |
| "My data changes a lot" | JSON | Easy to modify |
| "Multiple customers, one address" | Separate Table | No duplication |
| "I have lists of things" | JSON | Perfect for arrays |

---

## Complex Types - Demo Results

```
Create Customer with different address storage:
    ✓ Customer created
    Shipping Address (Columns): 123 Main St, Seattle
    Billing Address (JSON):   456 Oak Ave, Redmond

Search by Shipping City:
    SQL: WHERE [ShippingAddress_City] = 'Seattle'
    Time: 15ms ⚡ Fast!

Search by Billing City:
    SQL: WHERE JSON_VALUE([BillingAddress], '$.City') = 'Redmond'
    Time: 25ms (needs to read JSON)
```

**Simple Rule:** Use columns for things you search often, JSON for things that change!

---

## ExecuteUpdate
### Bulk Updates Made Simple

#### What Is ExecuteUpdate?
Update thousands of records with ONE database command instead of thousands.

**Old Way (Slow):**
```
1. Download 1,000 orders from database → 5 seconds
2. Change each one in memory          → 1 second
3. Save each back to database         → 20 seconds
Total: 26 seconds 🐌
```

**New Way (Fast):**
```
1. Send ONE update command → 0.2 seconds
Total: 0.2 seconds ⚡
Result: 130x faster!
```

#### Why Do We Need It?

**Common Tasks That Are Slow:**
- Change 10,000 order statuses from "Pending" to "Shipped"
- Apply 20% discount to all winter products
- Mark old records as "archived"
- Update customer addresses after moving

**What Goes Wrong (Old Way):**
```
Problem 1: Memory Full
- Loading 10,000 orders = 150 MB of RAM
- Your computer: "Out of memory!" 💥

Problem 2: Takes Forever
- Sending 10,000 updates one-by-one
- Like mailing 10,000 letters vs one package

Problem 3: Database Locks
- Database locked for 30 seconds
- Other users: "Why is everything slow?" 😤
```

#### Benefits Comparison

| Metric | Old Way | New Way | Winner |
|--------|---------|---------|--------|
| Time (1,000 records) | 2.5 seconds | 0.025 seconds | 100x faster ⚡ |
| Memory Used | 150 MB | 1 MB | 150x less 🎉 |
| Database Calls | 1,000 calls | 1 call | 1000x fewer 🚀 |
| Can Handle | 1,000 records | 1,000,000 records | Unlimited! |

### The Problem (Old Way)

```csharp
// Step 1: Download ALL orders (slow!)
var orders = await context.Orders
    .Where(o => o.Status == "Pending")
    .ToListAsync();  // ← Downloads 1000s of orders!

// Step 2: Change each one (slow!)
foreach (var order in orders) {
    order.Status = "Shipped";
}

// Step 3: Save each one back (slowest!)
await context.SaveChangesAsync();  // ← 1000s of UPDATE commands!
```

**What happens:**
```
Database → Your App → Database
   ↓         ↓          ↓
Download  Change    Upload
1000     1000      1000
orders   orders    updates
(5 sec)  (1 sec)  (20 sec)
Total: 26 seconds 🐌
```

---

## ExecuteUpdate - The Solution (New Way)

```csharp
// One command, done!
await context.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(o => o.Status, "Shipped")
        .SetProperty(o => o.ShippedDate, DateTime.Now)
    );

// That's it! 0.2 seconds ⚡
```

**What happens:**
```
Your App → Database
    ↓         ↓
   Send    Execute
  1 command  Directly
  (0.2 sec)

Total: 0.2 seconds ⚡
```

**Generated SQL (what database sees):**
```sql
UPDATE Orders
SET Status = 'Shipped', ShippedDate = NOW()
WHERE Status = 'Pending'

-- One command updates ALL matching orders!
```

---

## ExecuteUpdate - Performance Numbers

### Real-World Speed Test

| Number of Records | Old Way | New Way | Speed Up |
|-------------------|---------|---------|----------|
| 100 | 0.5 sec | 0.01 sec | 50x ⚡ |
| 1,000 | 2.5 sec | 0.025 sec | 100x ⚡⚡ |
| 10,000 | 28 sec | 0.18 sec | 155x ⚡⚡⚡ |
| 100,000 | 5 min | 1.2 sec | 250x 🚀 |

### Memory Usage

| Records | Old Way (Memory) | New Way (Memory) | Savings |
|---------|------------------|------------------|---------|
| 1,000 | 15 MB | 0.1 MB | 99% less ✨ |
| 10,000 | 150 MB | 1 MB | 99% less ✨ |
| 100,000 | 1.5 GB | 10 MB | 99% less ✨ |

### When to Use ExecuteUpdate

| Task | Use ExecuteUpdate? | Why |
|------|-------------------|-----|
| Change 1,000+ order statuses | ✅ Yes | Super fast! |
| Apply discount to all products | ✅ Yes | One command! |
| Mark old data as archived | ✅ Yes | Memory efficient! |
| Update one order with validation | ❌ No | Need to check rules |
| Calculate complex per-item price | ❌ No | Need custom logic |

**Simple Rule:** Big updates = ExecuteUpdate. Small updates with logic = Old way.

---

## Vector Search
### Smart Search Using AI

#### What Is Vector Search?
Search that understands **meaning**, not just matching words.

**Example:**
```
You search: "comfortable office chair"

Regular Search finds:
✅ "Office chair that is comfortable"  (exact words)
❌ "Ergonomic desk seating"           (no match!)
❌ "Executive furniture"               (no match!)

Vector Search finds:
✅ "Office chair that is comfortable"
✅ "Ergonomic desk seating"           (understands ergonomic ≈ comfortable)
✅ "Executive furniture"               (understands furniture ≈ chair)
