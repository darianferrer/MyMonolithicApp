using System;

namespace MyMonolithicApp.Infrastructure.Data.Entities
{
    public class RolePermissionEntity
    {
        public Guid RoleId { get; set; }
        public string PermissionCode { get; set; }
        public virtual RoleEntity Role { get; set; }
        public virtual PermissionEntity Permission { get; set; }
    }
}
