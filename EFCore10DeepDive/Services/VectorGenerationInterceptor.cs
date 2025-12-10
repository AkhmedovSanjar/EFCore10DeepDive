using EFCore10DeepDive.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Automatically generates vector embeddings for Product entities before saving
/// </summary>
public class VectorGenerationInterceptor : SaveChangesInterceptor
{
    private readonly EmbeddingService _embeddingService;

    public VectorGenerationInterceptor()
    {
        _embeddingService = new();
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var productEntries = eventData.Context.ChangeTracker
            .Entries<Product>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in productEntries)
        {
            var product = entry.Entity;
            if (product.SearchVector.IsNull || product.SearchVector.Length == 0)
            {
                var textToEmbed = $"{product.Name} {product.Description} {product.Category}";
                product.SearchVector = await _embeddingService.GenerateEmbeddingAsync(textToEmbed);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}