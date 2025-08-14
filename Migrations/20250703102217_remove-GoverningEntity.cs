// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class removeGoverningEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConformanceType",
                table: "ExtensionComponentModelElements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                table: "ExtensionComponentModelElements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageNoteCore",
                table: "ExtensionComponentModelElements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageNoteExtension",
                table: "ExtensionComponentModelElements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRules",
                table: "CoreInvoiceModel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "CoreInvoiceModel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentID",
                table: "CoreInvoiceModel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageNote",
                table: "CoreInvoiceModel",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConformanceType",
                table: "ExtensionComponentModelElements");

            migrationBuilder.DropColumn(
                name: "Justification",
                table: "ExtensionComponentModelElements");

            migrationBuilder.DropColumn(
                name: "UsageNoteCore",
                table: "ExtensionComponentModelElements");

            migrationBuilder.DropColumn(
                name: "UsageNoteExtension",
                table: "ExtensionComponentModelElements");

            migrationBuilder.DropColumn(
                name: "BusinessRules",
                table: "CoreInvoiceModel");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "CoreInvoiceModel");

            migrationBuilder.DropColumn(
                name: "ParentID",
                table: "CoreInvoiceModel");

            migrationBuilder.DropColumn(
                name: "UsageNote",
                table: "CoreInvoiceModel");
        }
    }
}
