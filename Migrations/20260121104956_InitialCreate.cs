using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SOb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fileName = table.Column<string>(type: "text", nullable: false),
                    dTime = table.Column<double>(type: "double precision", nullable: false),
                    startTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    avgExecutionTime = table.Column<double>(type: "double precision", nullable: false),
                    avgValue = table.Column<double>(type: "double precision", nullable: false),
                    maxValue = table.Column<double>(type: "double precision", nullable: false),
                    midValue = table.Column<double>(type: "double precision", nullable: false),
                    minValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fileName = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    executionTime = table.Column<double>(type: "double precision", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Values_fileName",
                table: "Values",
                column: "fileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "Values");
        }
    }
}
