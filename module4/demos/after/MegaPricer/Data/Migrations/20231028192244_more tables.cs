using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class moretables : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Wall_Kitchens_KitchenId",
          table: "Wall");

      migrationBuilder.DropPrimaryKey(
          name: "PK_Wall",
          table: "Wall");

      migrationBuilder.RenameTable(
          name: "Wall",
          newName: "Walls");

      migrationBuilder.RenameColumn(
          name: "Price",
          table: "PricingColors",
          newName: "WholesalePrice");

      migrationBuilder.RenameIndex(
          name: "IX_Wall_KitchenId",
          table: "Walls",
          newName: "IX_Walls_KitchenId");

      migrationBuilder.AddColumn<string>(
          name: "Name",
          table: "PricingColors",
          type: "TEXT",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<bool>(
          name: "IsIsland",
          table: "Walls",
          type: "INTEGER",
          nullable: false,
          defaultValue: false);

      migrationBuilder.AddPrimaryKey(
          name: "PK_Walls",
          table: "Walls",
          column: "WallId");

      migrationBuilder.CreateTable(
          name: "Cabinets",
          columns: table => new
          {
            CabinetId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            WallId = table.Column<int>(type: "INTEGER", nullable: false),
            CabinetOrder = table.Column<int>(type: "INTEGER", nullable: false),
            Color = table.Column<int>(type: "INTEGER", nullable: false),
            SKU = table.Column<string>(type: "TEXT", nullable: false),
            TopOffset = table.Column<float>(type: "REAL", nullable: false),
            LeftOffset = table.Column<float>(type: "REAL", nullable: false),
            Width = table.Column<float>(type: "REAL", nullable: false),
            Depth = table.Column<float>(type: "REAL", nullable: false),
            Height = table.Column<float>(type: "REAL", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Cabinets", x => x.CabinetId);
            table.ForeignKey(
                      name: "FK_Cabinets_Walls_WallId",
                      column: x => x.WallId,
                      principalTable: "Walls",
                      principalColumn: "WallId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "UserMarkups",
          columns: table => new
          {
            UserMarkupId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            UserName = table.Column<string>(type: "TEXT", nullable: false),
            MarkupPercent = table.Column<int>(type: "INTEGER", nullable: false),
            UseCustomPricing = table.Column<bool>(type: "INTEGER", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UserMarkups", x => x.UserMarkupId);
          });

      migrationBuilder.CreateTable(
          name: "Features",
          columns: table => new
          {
            FeatureId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            CabinetId = table.Column<int>(type: "INTEGER", nullable: false),
            FeatureOrder = table.Column<int>(type: "INTEGER", nullable: false),
            SKU = table.Column<string>(type: "TEXT", nullable: false),
            Quantity = table.Column<int>(type: "INTEGER", nullable: false),
            IsDoor = table.Column<bool>(type: "INTEGER", nullable: false),
            Color = table.Column<int>(type: "INTEGER", nullable: false),
            Width = table.Column<float>(type: "REAL", nullable: false),
            Depth = table.Column<float>(type: "REAL", nullable: false),
            Height = table.Column<float>(type: "REAL", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Features", x => x.FeatureId);
            table.ForeignKey(
                      name: "FK_Features_Cabinets_CabinetId",
                      column: x => x.CabinetId,
                      principalTable: "Cabinets",
                      principalColumn: "CabinetId",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Cabinets_WallId",
          table: "Cabinets",
          column: "WallId");

      migrationBuilder.CreateIndex(
          name: "IX_Features_CabinetId",
          table: "Features",
          column: "CabinetId");

      migrationBuilder.AddForeignKey(
          name: "FK_Walls_Kitchens_KitchenId",
          table: "Walls",
          column: "KitchenId",
          principalTable: "Kitchens",
          principalColumn: "KitchenId",
          onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Walls_Kitchens_KitchenId",
          table: "Walls");

      migrationBuilder.DropTable(
          name: "Features");

      migrationBuilder.DropTable(
          name: "UserMarkups");

      migrationBuilder.DropTable(
          name: "Cabinets");

      migrationBuilder.DropPrimaryKey(
          name: "PK_Walls",
          table: "Walls");

      migrationBuilder.DropColumn(
          name: "Name",
          table: "PricingColors");

      migrationBuilder.DropColumn(
          name: "IsIsland",
          table: "Walls");

      migrationBuilder.RenameTable(
          name: "Walls",
          newName: "Wall");

      migrationBuilder.RenameColumn(
          name: "WholesalePrice",
          table: "PricingColors",
          newName: "Price");

      migrationBuilder.RenameIndex(
          name: "IX_Walls_KitchenId",
          table: "Wall",
          newName: "IX_Wall_KitchenId");

      migrationBuilder.AddPrimaryKey(
          name: "PK_Wall",
          table: "Wall",
          column: "WallId");

      migrationBuilder.AddForeignKey(
          name: "FK_Wall_Kitchens_KitchenId",
          table: "Wall",
          column: "KitchenId",
          principalTable: "Kitchens",
          principalColumn: "KitchenId",
          onDelete: ReferentialAction.Cascade);
    }
  }
}
