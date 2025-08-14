// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class AddRowPos2Core : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "RowPos",
                table: "CoreInvoiceModel",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowPos",
                table: "CoreInvoiceModel");
        }
    }
}
