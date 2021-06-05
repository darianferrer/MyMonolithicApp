using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data.EntityConfigs
{
    internal class RolePermissionEntityConfig : IEntityTypeConfiguration<RolePermissionEntity>
    {
        public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PermissionCode });
        }
    }
}
