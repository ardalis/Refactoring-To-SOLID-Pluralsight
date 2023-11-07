using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class KitchenPropsAndColors : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<float>(
          name: "BaseDepth",
          table: "Kitchens",
          type: "REAL",
          nullable: false,
          defaultValue: 0f);

      migrationBuilder.AddColumn<float>(
          name: "BaseHeight",
          table: "Kitchens",
          type: "REAL",
          nullable: false,
          defaultValue: 0f);

      migrationBuilder.CreateTable(
          name: "PricingColors",
          columns: table => new
          {
            PricingColorId = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            Price = table.Column<float>(type: "REAL", nullable: false),
            PercentMarkup = table.Column<float>(type: "REAL", nullable: false),
            ColorPerSquareFoot = table.Column<float>(type: "REAL", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PricingColors", x => x.PricingColorId);
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "PricingColors");

      migrationBuilder.DropColumn(
          name: "BaseDepth",
          table: "Kitchens");

      migrationBuilder.DropColumn(
          name: "BaseHeight",
          table: "Kitchens");
    }
  }
}
