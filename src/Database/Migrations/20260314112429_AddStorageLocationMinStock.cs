using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FireInvent.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageLocationMinStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageLocationMinStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinStock = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocationMinStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocationMinStocks_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StorageLocationMinStocks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StorageLocationMinStocks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StorageLocationMinStocks_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StorageLocationMinStocks_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationMinStocks_CreatedById",
                table: "StorageLocationMinStocks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationMinStocks_ModifiedById",
                table: "StorageLocationMinStocks",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationMinStocks_StorageLocationId_VariantId",
                table: "StorageLocationMinStocks",
                columns: new[] { "StorageLocationId", "VariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationMinStocks_TenantId",
                table: "StorageLocationMinStocks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationMinStocks_VariantId",
                table: "StorageLocationMinStocks",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageLocationMinStocks");
        }
    }
}
