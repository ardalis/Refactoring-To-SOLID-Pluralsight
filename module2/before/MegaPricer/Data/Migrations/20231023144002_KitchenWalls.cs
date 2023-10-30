using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class KitchenWalls : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Kitchens",
          columns: table => new
          {
            KitchenId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            UserId = table.Column<Guid>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Kitchens", x => x.KitchenId);
          });

      migrationBuilder.CreateTable(
          name: "Wall",
          columns: table => new
          {
            WallId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            KitchenId = table.Column<int>(type: "INTEGER", nullable: false),
            Sequence = table.Column<int>(type: "INTEGER", nullable: false),
            CabinetColor = table.Column<int>(type: "INTEGER", nullable: false),
            VertColor = table.Column<int>(type: "INTEGER", nullable: false),
            BackingColor = table.Column<int>(type: "INTEGER", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Wall", x => x.WallId);
            table.ForeignKey(
                      name: "FK_Wall_Kitchens_KitchenId",
                      column: x => x.KitchenId,
                      principalTable: "Kitchens",
                      principalColumn: "KitchenId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Wall_KitchenId",
          table: "Wall",
          column: "KitchenId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Wall");

      migrationBuilder.DropTable(
          name: "Kitchens");
    }
  }
}
