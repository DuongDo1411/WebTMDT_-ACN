using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTMDT_DACN.Migrations
{
    /// <inheritdoc />
    public partial class Soldout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Soldout",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Soldout",
                table: "Products");
        }
    }
}
