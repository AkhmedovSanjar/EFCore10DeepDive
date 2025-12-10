using Microsoft.EntityFrameworkCore;
using EFCore10DeepDive.Models;
using EFCore10DeepDive.Services;

namespace EFCore10DeepDive.Data;

public class AppDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the current tenant ID for query filtering
    /// In production, this would come from authentication/authorization context
    /// </summary>
    public static int CurrentTenantId { get; set; } = 1;

    // Query Filter Constants
    public const string TenantFilterName = "Tenant";
    public const string SoftDeleteFilterName = "SoftDelete";

    public DbSet<Customer> Customers { get; set; }      // Complex Types (Table Splitting + JSON)
    public DbSet<AlternateAddress> AlternateAddresses { get; set; } // One-to-one association
    public DbSet<Order> Orders { get; set; }            // ExecuteUpdateAsync (Bulk Operations)
    public DbSet<Account> Accounts { get; set; }        // Named Query Filters
    public DbSet<Student> Students { get; set; }        // LeftJoin/RightJoin + SplitQuery
    public DbSet<Department> Departments { get; set; }  // LeftJoin/RightJoin + SplitQuery
    public DbSet<Enrollment> Enrollments { get; set; }  // SplitQuery (multiple includes)
    public DbSet<Product> Products { get; set; }        // Vector Search

    public static QueryCounterInterceptor QueryCounter { get; } = new();
    private static readonly VectorGenerationInterceptor VectorGenerator = new();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=EFCore10DeepDive;Trusted_Connection=True;TrustServerCertificate=True;")
            .AddInterceptors(QueryCounter, VectorGenerator);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCustomerEntity(modelBuilder);
        ConfigureAccountEntity(modelBuilder);
        ConfigureStudentEntity(modelBuilder);
        ConfigureProductEntity(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureCustomerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            // Complex Types - Table Splitting (no separate table, no JOINs)
            entity.ComplexProperty(c => c.ShippingAddress);

            // Stored as JSON text - More flexible, less storage, queryable
            entity.ComplexProperty(c => c.BillingAddress, builder => builder.ToJson());

            // One-to-one association - Separate table with JOIN
            entity.HasOne(c => c.AlternateAddress)
                .WithOne()
                .HasForeignKey<AlternateAddress>(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Collection of complex types stored as JSON
            entity.ComplexProperty(c => c.OrderHistories, builder =>
            {
                builder.ToJson();
            });

            // Struct complex type stored as JSON (value semantics)
            entity.ComplexProperty(c => c.Preferences, builder =>
            {
                builder.ToJson();
            });
        });
    }

    private void ConfigureAccountEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            // Named Query Filters - Multiple filters per entity
            entity.HasQueryFilter(SoftDeleteFilterName, a => !a.IsDeleted);
            entity.HasQueryFilter(TenantFilterName, a => a.TenantId == CurrentTenantId);
        });
    }

    private void ConfigureStudentEntity(ModelBuilder modelBuilder)
    {
        // LeftJoin/RightJoin + SplitQuery demonstrations
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId);
    }

    private void ConfigureProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Vector Search
            entity.Property(p => p.SearchVector)
                .HasColumnType("vector(1536)");
        });
    }
}
