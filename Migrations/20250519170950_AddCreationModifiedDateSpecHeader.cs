// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class AddCreationModifiedDateSpecHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "SpecificationIdentifyingInformation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "SpecificationIdentifyingInformation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "SpecificationIdentifyingInformation");

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
    }
}
