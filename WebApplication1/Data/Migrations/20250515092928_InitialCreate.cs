using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Валюты",
                columns: table => new
                {
                    Id_валюты = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название_валюты = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Валюты", x => x.Id_валюты);
                });

            migrationBuilder.CreateTable(
                name: "КурсыВалют",
                columns: table => new
                {
                    ID_курса = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Дата = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Значение = table.Column<decimal>(type: "decimal(15,6)", nullable: false),
                    ID_валюты = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_КурсыВалют", x => x.ID_курса);
                    table.ForeignKey(
                        name: "FK_КурсыВалют_Валюты_ID_валюты",
                        column: x => x.ID_валюты,
                        principalTable: "Валюты",
                        principalColumn: "Id_валюты",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_КурсыВалют_Дата_ID_валюты",
                table: "КурсыВалют",
                columns: new[] { "Дата", "ID_валюты" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_КурсыВалют_ID_валюты",
                table: "КурсыВалют",
                column: "ID_валюты");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "КурсыВалют");

            migrationBuilder.DropTable(
                name: "Валюты");
        }
    }
}
