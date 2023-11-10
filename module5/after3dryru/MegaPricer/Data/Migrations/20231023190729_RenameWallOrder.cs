using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaPricer.Data.Migrations
{
  /// <inheritdoc />
  public partial class RenameWallOrder : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "Sequence",
          table: "Wall",
          newName: "WallOrder");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "WallOrder",
          table: "Wall",
          newName: "Sequence");
    }
  }
}
