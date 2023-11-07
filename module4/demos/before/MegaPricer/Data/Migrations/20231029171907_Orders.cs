using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class Orders : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Orders",
          columns: table => new
          {
            OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            KitchenId = table.Column<int>(type: "INTEGER", nullable: false),
            OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
            OrderStatus = table.Column<string>(type: "TEXT", nullable: true),
            OrderType = table.Column<string>(type: "TEXT", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Orders", x => x.OrderId);
          });

      migrationBuilder.CreateTable(
          name: "OrderItem",
          columns: table => new
          {
            OrderItemId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            OrderId = table.Column<int>(type: "INTEGER", nullable: false),
            SKU = table.Column<string>(type: "TEXT", nullable: true),
            Quantity = table.Column<int>(type: "INTEGER", nullable: false),
            BasePrice = table.Column<float>(type: "REAL", nullable: false),
            Markup = table.Column<float>(type: "REAL", nullable: false),
            UserMarkup = table.Column<float>(type: "REAL", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_OrderItem", x => x.OrderItemId);
            table.ForeignKey(
                      name: "FK_OrderItem_Orders_OrderId",
                      column: x => x.OrderId,
                      principalTable: "Orders",
                      principalColumn: "OrderId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.UpdateData(
          table: "AspNetUsers",
          keyColumn: "Id",
          keyValue: "b74ddd14-6340-4840-95c2-abcdef4843e5",
          columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
          values: new object[] { "d9e55c51-5c33-4d00-b4a1-c65c72d2556f", "AQAAAAIAAYagAAAAEJakOqI7SK737L9BHVnyUSN1fiY3n0gaba4HMR+Xn/+shQ08SPLVOEWlK9oId/jJdw==", "b2194aef-0f4b-4323-9b20-10e91f2c6e4d" });

      migrationBuilder.CreateIndex(
          name: "IX_OrderItem_OrderId",
          table: "OrderItem",
          column: "OrderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "OrderItem");

      migrationBuilder.DropTable(
          name: "Orders");

      migrationBuilder.UpdateData(
          table: "AspNetUsers",
          keyColumn: "Id",
          keyValue: "b74ddd14-6340-4840-95c2-abcdef4843e5",
          columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
          values: new object[] { "faab6afc-23fd-46d4-8376-b100674c7534", "AQAAAAIAAYagAAAAECbzx+JACHLuz1ly/ZM8eliYOLK6tDbmPXOVbuUfKBj1dfMdl2fmt2y+KIir3PJyDw==", "895fd239-7e39-405f-80c6-5f18906a89c9" });
    }
  }
}
