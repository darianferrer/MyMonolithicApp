using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data.EntityConfigs
{
    internal class UserEntityConfig : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(64);
            builder.Property(x => x.NormalisedUsername)
                .IsRequired()
                .HasMaxLength(64);
            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.HasIndex(x => x.NormalisedUsername)
                .IsUnique();

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId);
        }
    }
}
