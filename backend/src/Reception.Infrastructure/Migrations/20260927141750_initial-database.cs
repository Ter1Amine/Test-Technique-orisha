using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reception.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    CommandId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.CommandId);
                });

            migrationBuilder.CreateTable(
                name: "Palettes",
                columns: table => new
                {
                    PaletteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommandId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Palettes", x => x.PaletteId);
                    table.ForeignKey(
                        name: "FK_Palettes_Commands_CommandId",
                        column: x => x.CommandId,
                        principalTable: "Commands",
                        principalColumn: "CommandId");
                });

            migrationBuilder.CreateTable(
                name: "Cartons",
                columns: table => new
                {
                    CartonId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaletteId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartons", x => x.CartonId);
                    table.ForeignKey(
                        name: "FK_Cartons_Palettes_PaletteId",
                        column: x => x.PaletteId,
                        principalTable: "Palettes",
                        principalColumn: "PaletteId");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    RefId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CartonId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.RefId);
                    table.ForeignKey(
                        name: "FK_Products_Cartons_CartonId",
                        column: x => x.CartonId,
                        principalTable: "Cartons",
                        principalColumn: "CartonId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cartons_PaletteId",
                table: "Cartons",
                column: "PaletteId");

            migrationBuilder.CreateIndex(
                name: "IX_Palettes_CommandId",
                table: "Palettes",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CartonId",
                table: "Products",
                column: "CartonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Cartons");

            migrationBuilder.DropTable(
                name: "Palettes");

            migrationBuilder.DropTable(
                name: "Commands");
        }
    }
}
