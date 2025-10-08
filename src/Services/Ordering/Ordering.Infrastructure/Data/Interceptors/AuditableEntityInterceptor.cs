﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ordering.Infrastructure.Data.Interceptors;
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<IEntity>())
        {
            HandleEntryCreation(entry);
            HandleEntryModification(entry);
        }
    }
    private void HandleEntryCreation(EntityEntry<IEntity> entry)
    {
        if (entry.State != EntityState.Added) return;

        entry.Entity.CreatedBy = "mehmet";
        entry.Entity.CreatedAt = DateTime.UtcNow;
    }
    private void HandleEntryModification(EntityEntry<IEntity> entry)
    {
        if (entry is { State: EntityState.Added | EntityState.Modified }
               || entry.HasChangedOwnedEntities())
        {
            entry.Entity.LastModifiedBy = "mehmet";
            entry.Entity.LastModified = DateTime.UtcNow;
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
