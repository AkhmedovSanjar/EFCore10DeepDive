using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using EFCore10DeepDive.Services;
using Microsoft.EntityFrameworkCore;

namespace EFCore10DeepDive.DemoStrategies;

/// <summary>
/// NEW in EF Core 10: Named Query Filters
/// Benefits: Multi-tenant + soft-delete support, selective filter ignoring
/// </summary>
public class NamedQueryFiltersDemo : DemoBase
{
    public override string FeatureName => "Named Query Filters";
    public override string Description => "Multiple named filters per entity with selective disabling";

    protected override async Task ExecuteDemoAsync(AppDbContext context)
    {
        Console.WriteLine("Create accounts for different tenants");
        var accounts = new List<Account>
        {
            new() 
            { 
                AccountNumber = "ACC-T1-001", 
                AccountName = "Alice Corp Savings",
                Email = "alice@tenant1.com",
                Balance = 50000m,
                AccountType = "Savings",
                TenantId = 1, 
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow.AddMonths(-6)
            },
            new() 
            { 
                AccountNumber = "ACC-T1-002", 
                AccountName = "Alice Corp Checking",
                Email = "alice@tenant1.com",
                Balance = 25000m,
                AccountType = "Checking",
                TenantId = 1, 
                IsDeleted = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-12),
                DeletedDate = DateTime.UtcNow.AddDays(-30)
            },
            new() 
            { 
                AccountNumber = "ACC-T2-001", 
                AccountName = "Bob Industries Checking",
                Email = "bob@tenant2.com",
                Balance = 75000m,
                AccountType = "Checking",
                TenantId = 2, 
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow.AddMonths(-8)
            },
            new() 
            { 
                AccountNumber = "ACC-T2-002", 
                AccountName = "Bob Industries Investment",
                Email = "bob@tenant2.com",
                Balance = 150000m,
                AccountType = "Investment",
                TenantId = 2, 
                IsDeleted = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-18),
                DeletedDate = DateTime.UtcNow.AddDays(-15)
            },
        };

        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();
        Console.WriteLine($"    Created {accounts.Count} accounts across 2 tenants");

        AppDbContext.CurrentTenantId = 1;
        
        Console.WriteLine("\nDefault query (Both filters active: Tenant=1, NotDeleted)");
        Console.WriteLine("    Current Tenant: 1");
        var defaultQuery = await context.Accounts.ToListAsync();
        
        foreach (var account in defaultQuery)
        {
            Console.WriteLine($"    - {account.AccountNumber} - {account.AccountName}");
            Console.WriteLine($"       Type: {account.AccountType} | Balance: ${account.Balance:N2} | Tenant: {account.TenantId} | Deleted: {account.IsDeleted}");
        }

        Console.WriteLine("\nIgnore 'SoftDelete' filter (show deleted accounts for Tenant 1)");
        var ignoreSoftDelete = await context.Accounts
            .IgnoreQueryFilters([AppDbContext.SoftDeleteFilterName])
            .ToListAsync();
        
        Console.WriteLine($"    Found {ignoreSoftDelete.Count} accounts (including deleted):");
        foreach (var account in ignoreSoftDelete)
        {
            var status = account.IsDeleted ? "X DELETED" : "+ Active";
            Console.WriteLine($"    - {account.AccountNumber} - {account.AccountName} [{status}]");
            if (account.IsDeleted && account.DeletedDate.HasValue)
            {
                Console.WriteLine($"       Deleted on: {account.DeletedDate:d}");
            }
        }

        Console.WriteLine("\nIgnore 'Tenant' filter (show all active accounts across all tenants)");
        var ignoreTenant = await context.Accounts
            .IgnoreQueryFilters([AppDbContext.TenantFilterName])
            .ToListAsync();
        
        Console.WriteLine($"    Found {ignoreTenant.Count} active accounts across all tenants:");
        foreach (var account in ignoreTenant)
        {
            Console.WriteLine($"    - {account.AccountNumber} - {account.AccountName}");
            Console.WriteLine($"       Tenant: {account.TenantId} | Type: {account.AccountType} | Balance: ${account.Balance:N2}");
        }

        Console.WriteLine("\nIgnore all filters (admin view - all accounts everywhere)");
        var ignoreAll = await context.Accounts
            .IgnoreQueryFilters()
            .OrderBy(a => a.TenantId)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();
        
        Console.WriteLine($"    Admin View - All {ignoreAll.Count} accounts:");
        foreach (var account in ignoreAll)
        {
            var status = account.IsDeleted ? "X DELETED" : "+ Active";
            Console.WriteLine($"    - Tenant {account.TenantId}: {account.AccountNumber} - {account.AccountName} [{status}]");
            Console.WriteLine($"       Balance: ${account.Balance:N2} | Type: {account.AccountType}");
        }

        Console.WriteLine("\nSwitch to Tenant 2 context");
        AppDbContext.CurrentTenantId = 2;
        
        var tenant2Accounts = await context.Accounts.ToListAsync();
        Console.WriteLine($"    Current Tenant: 2");
        Console.WriteLine($"    Found {tenant2Accounts.Count} active accounts for Tenant 2:");
        foreach (var account in tenant2Accounts)
        {
            Console.WriteLine($"    - {account.AccountNumber} - {account.AccountName}");
            Console.WriteLine($"       Type: {account.AccountType} | Balance: ${account.Balance:N2}");
        }

        Console.WriteLine();
        Console.WriteLine("Key Benefits of Named Query Filters:");
        Console.WriteLine($"   - Multiple filters per entity ({AppDbContext.TenantFilterName} + {AppDbContext.SoftDeleteFilterName})");
        Console.WriteLine("   - Selective filter ignoring by name");
        Console.WriteLine("   - Automatic application on all queries");
        Console.WriteLine("   - Perfect for: SaaS applications, audit trails, compliance");
        
        PrintQuerySummary();

        // Reset tenant for other demos
        AppDbContext.CurrentTenantId = 1;
    }
}

