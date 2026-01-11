using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities.Interfaces;

namespace IdentityService.Infrastructure.Data.Configurations;

public class BaseIdentifierConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IIdentifier
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
