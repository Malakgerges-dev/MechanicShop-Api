using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MechanicShop.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(
    IUser user,
    TimeProvider DateTime
) : SaveChangesInterceptor
{
    private readonly IUser _user = user;
    private readonly TimeProvider _DateTime = DateTime;

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
        if(context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if(entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var Utc = _DateTime.GetUtcNow();
                if(entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.CreatedAtUtc = Utc;
                }

                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModifiedUtc = Utc;


                foreach (var ownedEntry in entry.References)
                {
                    if(ownedEntry.TargetEntry is{Entity: AuditableEntity ownedEntity}
                    && ownedEntry.TargetEntry.State is EntityState.Added or EntityState.Modified)
                    {
                        if (ownedEntry.TargetEntry.State == EntityState.Added)
                        {
                            ownedEntity.CreatedBy = _user.Id;
                            ownedEntity.CreatedAtUtc = Utc;
                        }

                        ownedEntity.LastModifiedBy = _user.Id;
                        ownedEntity.LastModifiedUtc = Utc ;
                    }
                 
                }
                
            }

        
        }
    }


}

public static class Extension
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry)=>
        entry.References.Any(
            r=> r.TargetEntry?.Metadata.IsOwned()==true&&
            r.TargetEntry.State == EntityState.Added ||r.TargetEntry?.State == EntityState.Modified
        );
    
}