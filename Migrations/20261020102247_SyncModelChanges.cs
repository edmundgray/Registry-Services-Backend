// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "GoverningEntity",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserGroupID",
                table: "SpecificationIdentifyingInformation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AdditionalRequirements",
                columns: table => new
                {
                    BusinessTermID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IdentityID = table.Column<int>(type: "int", nullable: false),
                    BusinessTermName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Cardinality = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowPos = table.Column<short>(type: "smallint", nullable: false),
                    SemanticDescription = table.Column<string>(type: "text", nullable: true),
                    UsageNote = table.Column<string>(type: "text", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusinessRules = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypeOfChange = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalRequirements", x => new { x.IdentityID, x.BusinessTermID });
                    table.ForeignKey(
                        name: "FK_AdditionalRequirements_SpecificationIdentifyingInformation_IdentityID",
                        column: x => x.IdentityID,
                        principalTable: "SpecificationIdentifyingInformation",
                        principalColumn: "IdentityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID",
                table: "SpecificationIdentifyingInformation",
                column: "UserGroupID",
                principalTable: "UserGroups",
                principalColumn: "UserGroupID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropTable(
                name: "AdditionalRequirements");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "UserGroupID",
                table: "SpecificationIdentifyingInformation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "GoverningEntity",
                table: "SpecificationIdentifyingInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID",
                table: "SpecificationIdentifyingInformation",
                column: "UserGroupID",
                principalTable: "UserGroups",
                principalColumn: "UserGroupID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
