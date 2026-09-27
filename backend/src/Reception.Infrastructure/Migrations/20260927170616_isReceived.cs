using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reception.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class isReceived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Commands");

            migrationBuilder.AddColumn<bool>(
                name: "IsReceived",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReceived",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Commands",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
