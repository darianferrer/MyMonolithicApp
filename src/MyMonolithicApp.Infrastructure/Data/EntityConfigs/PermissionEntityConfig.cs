using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data.EntityConfigs
{
    internal class PermissionEntityConfig : IEntityTypeConfiguration<PermissionEntity>
    {
        public void Configure(EntityTypeBuilder<PermissionEntity> builder)
        {
            builder.HasKey(x => x.Code);
            builder.Property(x => x.Code)
                .HasMaxLength(128);
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasMany(x => x.Roles)
                .WithOne(x => x.Permission)
                .HasForeignKey(x => x.PermissionCode);
        }
    }
}
