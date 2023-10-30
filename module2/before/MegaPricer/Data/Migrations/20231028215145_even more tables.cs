using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class evenmoretables : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<float>(
          name: "Height",
          table: "Walls",
          type: "REAL",
          nullable: false,
          defaultValue: 0f);

      migrationBuilder.AddColumn<string>(
          name: "CompanyShortName",
          table: "AspNetUsers",
          type: "TEXT",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "Discriminator",
          table: "AspNetUsers",
          type: "TEXT",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<string>(
          name: "PricingOff",
          table: "AspNetUsers",
          type: "TEXT",
          nullable: true);

      migrationBuilder.CreateTable(
          name: "PricingSkus",
          columns: table => new
          {
            PricingSkuId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            SKU = table.Column<string>(type: "TEXT", nullable: false),
            WholesalePrice = table.Column<float>(type: "REAL", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PricingSkus", x => x.PricingSkuId);
          });

      migrationBuilder.InsertData(
          table: "AspNetRoles",
          columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
          values: new object[,]
          {
                    { "a7b013f0-5201-4317-abd8-c211f91b7330", null, "Sales", "SALES" },
                    { "aab4fac1-c546-41de-aebc-a14da6895711", null, "Admin", "ADMIN" }
          });

      migrationBuilder.InsertData(
          table: "AspNetUsers",
          columns: new[] { "Id", "AccessFailedCount", "CompanyShortName", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PricingOff", "SecurityStamp", "TwoFactorEnabled", "UserName" },
          values: new object[] { "b74ddd14-6340-4840-95c2-abcdef4843e5", 0, "ACME", "46eefd44-2090-4075-a65f-623ff058a162", "AppUser", "admin@test.com", true, false, null, "ADMIN@TEST.COM", "ADMIN@TEST.COM", "AQAAAAIAAYagAAAAEO3scqozI6mwcfxlH4ODxeGhi7G5swywz5ZqBxbyHk5xWJrdXVRg51Y6JuaGSc/+cA==", null, false, "N", "64cb98de-bcb3-45d3-9f18-5ebd49967b23", false, "admin@test.com" });

      migrationBuilder.InsertData(
          table: "AspNetUserRoles",
          columns: new[] { "RoleId", "UserId" },
          values: new object[] { "aab4fac1-c546-41de-aebc-a14da6895711", "b74ddd14-6340-4840-95c2-abcdef4843e5" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "PricingSkus");

      migrationBuilder.DeleteData(
          table: "AspNetRoles",
          keyColumn: "Id",
          keyValue: "a7b013f0-5201-4317-abd8-c211f91b7330");

      migrationBuilder.DeleteData(
          table: "AspNetUserRoles",
          keyColumns: new[] { "RoleId", "UserId" },
          keyValues: new object[] { "aab4fac1-c546-41de-aebc-a14da6895711", "b74ddd14-6340-4840-95c2-abcdef4843e5" });

      migrationBuilder.DeleteData(
          table: "AspNetRoles",
          keyColumn: "Id",
          keyValue: "aab4fac1-c546-41de-aebc-a14da6895711");

      migrationBuilder.DeleteData(
          table: "AspNetUsers",
          keyColumn: "Id",
          keyValue: "b74ddd14-6340-4840-95c2-abcdef4843e5");

      migrationBuilder.DropColumn(
          name: "Height",
          table: "Walls");

      migrationBuilder.DropColumn(
          name: "CompanyShortName",
          table: "AspNetUsers");

      migrationBuilder.DropColumn(
          name: "Discriminator",
          table: "AspNetUsers");

      migrationBuilder.DropColumn(
          name: "PricingOff",
          table: "AspNetUsers");
    }
  }
}
