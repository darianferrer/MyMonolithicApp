using System.Collections.Generic;
using MyMonolithicApp.Domain.Users;

namespace MyMonolithicApp.Infrastructure.Data.Entities
{
    public class PermissionEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public virtual ICollection<RolePermissionEntity> Roles { get; set; }

        public static Permission ToModel(PermissionEntity permission) =>
            new(permission.Code, permission.Name);
    }
}
