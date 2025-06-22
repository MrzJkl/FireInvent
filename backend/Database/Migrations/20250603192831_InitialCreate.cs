using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlameGuardLaundry.Database.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ClothingProducts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Manufacturer = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClothingProducts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Departments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Departments", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Persons",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                LastName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                ContactInfo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Persons", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "StorageLocations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Remarks = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StorageLocations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                ProviderKey = table.Column<string>(type: "text", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                RoleId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ClothingVariants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                AdditionalSpecs = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClothingVariants", x => x.Id);
                table.ForeignKey(
                    name: "FK_ClothingVariants_ClothingProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "ClothingProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DepartmentPerson",
            columns: table => new
            {
                DepartmentsId = table.Column<Guid>(type: "uuid", nullable: false),
                PersonsId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DepartmentPerson", x => new { x.DepartmentsId, x.PersonsId });
                table.ForeignKey(
                    name: "FK_DepartmentPerson_Departments_DepartmentsId",
                    column: x => x.DepartmentsId,
                    principalTable: "Departments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DepartmentPerson_Persons_PersonsId",
                    column: x => x.PersonsId,
                    principalTable: "Persons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ClothingItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                Identifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                StorageLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                Condition = table.Column<int>(type: "integer", nullable: false),
                PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                RetirementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClothingItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_ClothingItems_ClothingVariants_VariantId",
                    column: x => x.VariantId,
                    principalTable: "ClothingVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ClothingItems_StorageLocations_StorageLocationId",
                    column: x => x.StorageLocationId,
                    principalTable: "StorageLocations",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "ClothingItemAssignmentHistories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                AssignedFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AssignedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClothingItemAssignmentHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_ClothingItemAssignmentHistories_ClothingItems_ItemId",
                    column: x => x.ItemId,
                    principalTable: "ClothingItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ClothingItemAssignmentHistories_Persons_PersonId",
                    column: x => x.PersonId,
                    principalTable: "Persons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Maintenances",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                PeformedById = table.Column<string>(type: "text", nullable: true),
                MaintenanceType = table.Column<int>(type: "integer", nullable: false),
                Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Maintenances", x => x.Id);
                table.ForeignKey(
                    name: "FK_Maintenances_AspNetUsers_PeformedById",
                    column: x => x.PeformedById,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Maintenances_ClothingItems_ItemId",
                    column: x => x.ItemId,
                    principalTable: "ClothingItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClothingItemAssignmentHistories_ItemId_AssignedFrom",
            table: "ClothingItemAssignmentHistories",
            columns: new[] { "ItemId", "AssignedFrom" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClothingItemAssignmentHistories_PersonId",
            table: "ClothingItemAssignmentHistories",
            column: "PersonId");

        migrationBuilder.CreateIndex(
            name: "IX_ClothingItems_Identifier",
            table: "ClothingItems",
            column: "Identifier",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClothingItems_StorageLocationId",
            table: "ClothingItems",
            column: "StorageLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_ClothingItems_VariantId",
            table: "ClothingItems",
            column: "VariantId");

        migrationBuilder.CreateIndex(
            name: "IX_ClothingProducts_Manufacturer",
            table: "ClothingProducts",
            column: "Manufacturer");

        migrationBuilder.CreateIndex(
            name: "IX_ClothingProducts_Name",
            table: "ClothingProducts",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_ClothingProducts_Name_Manufacturer",
            table: "ClothingProducts",
            columns: new[] { "Name", "Manufacturer" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClothingVariants_Name",
            table: "ClothingVariants",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_ClothingVariants_ProductId_Name",
            table: "ClothingVariants",
            columns: new[] { "ProductId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DepartmentPerson_PersonsId",
            table: "DepartmentPerson",
            column: "PersonsId");

        migrationBuilder.CreateIndex(
            name: "IX_Departments_Name",
            table: "Departments",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Maintenances_ItemId",
            table: "Maintenances",
            column: "ItemId");

        migrationBuilder.CreateIndex(
            name: "IX_Maintenances_PeformedById",
            table: "Maintenances",
            column: "PeformedById");

        migrationBuilder.CreateIndex(
            name: "IX_Persons_ExternalId",
            table: "Persons",
            column: "ExternalId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Persons_FirstName",
            table: "Persons",
            column: "FirstName");

        migrationBuilder.CreateIndex(
            name: "IX_Persons_FirstName_LastName",
            table: "Persons",
            columns: new[] { "FirstName", "LastName" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Persons_LastName",
            table: "Persons",
            column: "LastName");

        migrationBuilder.CreateIndex(
            name: "IX_StorageLocations_Name",
            table: "StorageLocations",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "ClothingItemAssignmentHistories");

        migrationBuilder.DropTable(
            name: "DepartmentPerson");

        migrationBuilder.DropTable(
            name: "Maintenances");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "Departments");

        migrationBuilder.DropTable(
            name: "Persons");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "ClothingItems");

        migrationBuilder.DropTable(
            name: "ClothingVariants");

        migrationBuilder.DropTable(
            name: "StorageLocations");

        migrationBuilder.DropTable(
            name: "ClothingProducts");
    }
}
