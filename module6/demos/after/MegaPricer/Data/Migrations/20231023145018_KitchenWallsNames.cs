using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class KitchenWallsNames : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "Name",
          table: "Wall",
          type: "TEXT",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<string>(
          name: "Name",
          table: "Kitchens",
          type: "TEXT",
          nullable: false,
          defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "Name",
          table: "Wall");

      migrationBuilder.DropColumn(
          name: "Name",
          table: "Kitchens");
    }
  }
}
