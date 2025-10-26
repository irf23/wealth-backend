using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssetName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PrimaryAssetCategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WealthAssetType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BalanceCurrent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAsOf = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetBalanceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<string>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAsOf = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetBalanceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetBalanceHistories_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetBalanceHistories_BalanceAsOf",
                table: "AssetBalanceHistories",
                column: "BalanceAsOf");

            migrationBuilder.CreateIndex(
                name: "IX_AssetBalanceHistory_AssetId_BalanceAsOf",
                table: "AssetBalanceHistories",
                columns: new[] { "AssetId", "BalanceAsOf" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_BalanceAsOf",
                table: "Assets",
                column: "BalanceAsOf");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_PrimaryAssetCategory",
                table: "Assets",
                column: "PrimaryAssetCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WealthAssetType",
                table: "Assets",
                column: "WealthAssetType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetBalanceHistories");

            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
