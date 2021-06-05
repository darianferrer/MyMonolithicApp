using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data.EntityConfigs
{
    internal class RoleEntityConfig : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(64);
            builder.Property(x => x.NormalisedName)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasIndex(x => x.NormalisedName)
                .IsUnique();

            builder.HasMany(x => x.Permissions)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
