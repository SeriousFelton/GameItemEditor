using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameItemEditor.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<string>(type: "text", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    PropertiesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameItems_CreatedAt",
                table: "GameItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GameItems_Name",
                table: "GameItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_GameItems_Rarity",
                table: "GameItems",
                column: "Rarity");

            migrationBuilder.CreateIndex(
                name: "IX_GameItems_Type",
                table: "GameItems",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameItems");
        }
    }
}
