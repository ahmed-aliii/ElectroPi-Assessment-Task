using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain;

namespace TMS.Infrastructure
{
    public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public static readonly string AdminRoleId = "f8a5c2d1-4b6e-4a9f-9c3d-1e2f3a4b5c6d";
        public static readonly string UserRoleId = "a3b7d9e2-5c4f-4a8b-9d1e-2f3a4b5c6d7e";

        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Id = AdminRoleId,
                    Name = AppRoles.Admin,
                    NormalizedName = AppRoles.Admin.ToUpperInvariant(),
                    ConcurrencyStamp = "role-admin-concurrency-stamp"
                },
                new IdentityRole
                {
                    Id = UserRoleId,
                    Name = AppRoles.User,
                    NormalizedName = AppRoles.User.ToUpperInvariant(),
                    ConcurrencyStamp = "role-user-concurrency-stamp"
                });
        }
    }
}
