using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities.Interfaces;

namespace IdentityService.Infrastructure.Data.Configurations;

public class BaseEntityConfiguration<T> : BaseIdentifierConfiguration<T> where T : class, IBaseEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.Removed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.UpdatedBy).IsRequired(false);
        builder.Property(x => x.RemovedAt).IsRequired(false);
        builder.Property(x => x.RemovedBy).IsRequired(false);
    }
}
