using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { "f8a5c2d1-4b6e-4a9f-9c3d-1e2f3a4b5c6d", "Admin", "ADMIN", "role-admin-concurrency-stamp" },
                    { "a3b7d9e2-5c4f-4a8b-9d1e-2f3a4b5c6d7e", "User", "USER", "role-user-concurrency-stamp" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f8a5c2d1-4b6e-4a9f-9c3d-1e2f3a4b5c6d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a3b7d9e2-5c4f-4a8b-9d1e-2f3a4b5c6d7e");
        }
    }
}
