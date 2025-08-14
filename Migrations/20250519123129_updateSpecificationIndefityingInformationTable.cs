// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class updateSpecificationIndefityingInformationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConformanceLeval",
                table: "SpecificationIdentifyingInformation",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistryStatus",
                table: "SpecificationIdentifyingInformation",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecificationType",
                table: "SpecificationIdentifyingInformation",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConformanceLeval",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "RegistryStatus",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "SpecificationType",
                table: "SpecificationIdentifyingInformation");
        }
    }
}
