using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10DeepDive.DemoStrategies;

/// <summary>
/// Complex Types - Table Splitting & JSON Storage
/// </summary>
public class ComplexTypesDemo : DemoBase
{
    public override string FeatureName => "Complex Types - Table Splitting & JSON";
    public override string Description => "Compare complex types stored as columns vs JSON vs one-to-one association";

    protected override async Task ExecuteDemoAsync(AppDbContext context)
    {
        Console.WriteLine("Create customer with both storage types");
        var customer = new Customer
        {
            Name = "Sarah Johnson",
            Email = "sarah.johnson@example.com",
            
            // TABLE SPLITTING - Stored as columns
            ShippingAddress = new Address
            {
                Street = "123 Main St",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101",
                Country = "USA"
            },
            
            // JSON STORAGE - Stored as JSON
            BillingAddress = new Address
            {
                Street = "456 Business Ave",
                City = "Redmond",
                State = "WA",
                ZipCode = "98052",
                Country = "USA"
            },
            
            // JSON COLLECTION - Stored as JSON array
            OrderHistories = new List<OrderHistory>
            {
                new() 
                { 
                    OrderId = "ORD-2024-001", 
                    OrderDate = DateTime.UtcNow.AddDays(-10), 
                    TotalAmount = 299.99m, 
                    Status = "Delivered", 
                    Items = ["Laptop", "Mouse", "USB-C Cable"] 
                },
                new() 
                { 
                    OrderId = "ORD-2024-002", 
                    OrderDate = DateTime.UtcNow.AddDays(-5), 
                    TotalAmount = 49.99m, 
                    Status = "Shipped", 
                    Items = ["Keyboard"] 
                }
            },
            
            // STRUCT JSON - Value semantics stored as JSON
            Preferences = new CustomerPreferences
            {
                EmailNotifications = true,
                SmsNotifications = false,
                PreferredLanguage = "en-US",
                Currency = "USD",
                ThemeMode = "Dark"
            },
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        Console.WriteLine($"    + Created customer: {customer.Name}");

        var seattleCustomers = await context.Customers
            .Where(c => c.ShippingAddress.City == "Seattle")
            .ToListAsync();

        foreach (var c in seattleCustomers)
            Console.WriteLine($"    Name: {c.Name}, Shipping City: {c.ShippingAddress.City}");

        Console.WriteLine("\nQuery by JSON property (BillingAddress.City)");
        var redmondCustomers = await context.Customers
            .Where(c => c.BillingAddress.City == "Redmond")
            .ToListAsync();
        
        foreach (var c in redmondCustomers)
        {
            Console.WriteLine($"    Name: {c.Name}, Billing City: {c.BillingAddress.City}");
        }

        Console.WriteLine("\nQuery JSON collection (OrderHistories)");
        foreach (var c in redmondCustomers)
        {
            Console.WriteLine($"    - {c.Name} - {c.OrderHistories.Count} orders");
            foreach (var order in c.OrderHistories.Where(o => o.TotalAmount > 100))
            {
                Console.WriteLine($"       - {order.OrderId}: ${order.TotalAmount:F2} ({order.Status})");
                Console.WriteLine($"         Items: {string.Join(", ", order.Items)}");
            }
        }

        Console.WriteLine("\nQuery by Struct JSON property (Preferences)");
        Console.WriteLine("    Find dark mode users");
        var darkModeUsers = await context.Customers
            .Where(c => c.Preferences.ThemeMode == "Dark")
            .ToListAsync();
        
        foreach (var c in darkModeUsers)
        {
            Console.WriteLine($"    - {c.Name}");
            Console.WriteLine($"       Theme: {c.Preferences.ThemeMode}, Lang: {c.Preferences.PreferredLanguage}, Currency: {c.Preferences.Currency}");
            Console.WriteLine($"       Notifications: Email={c.Preferences.EmailNotifications}, SMS={c.Preferences.SmsNotifications}");
        }

        Console.WriteLine("\nUpdate TABLE SPLITTING property");
        var customerToUpdate = await context.Customers.FirstAsync();
        customerToUpdate.ShippingAddress.City = "Bellevue";
        await context.SaveChangesAsync();
        Console.WriteLine($"    + Updated ShippingAddress.City to Bellevue (column update)");

        Console.WriteLine("\nUpdate JSON property");
        customerToUpdate.BillingAddress.City = "Seattle";
        await context.SaveChangesAsync();
        Console.WriteLine($"    + Updated BillingAddress.City to Seattle (JSON update)");

        Console.WriteLine("\nCompare storage approaches");
        var allCustomers = await context.Customers.ToListAsync();
        foreach (var c in allCustomers)
        {
            Console.WriteLine($"    Customer: {c.Name}");
            Console.WriteLine($"    +- TABLE SPLIT:  ShippingAddress -> {c.ShippingAddress.City} (stored as columns)");
            Console.WriteLine($"    +- JSON:         BillingAddress -> {c.BillingAddress.City} (stored as JSON)");
            Console.WriteLine($"    +- JSON Array:   OrderHistories -> {c.OrderHistories.Count} orders (JSON array)");
            Console.WriteLine($"    +- Struct JSON:  Preferences -> {c.Preferences.ThemeMode} mode (value semantics)");
        }
        
        PrintQuerySummary();
    }
}
