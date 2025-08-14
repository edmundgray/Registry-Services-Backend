// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class addUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserGroupID",
                table: "SpecificationIdentifyingInformation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "SpecificationIdentifyingInformation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    UserGroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.UserGroupID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserGroupID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_UserGroups_UserGroupID",
                        column: x => x.UserGroupID,
                        principalTable: "UserGroups",
                        principalColumn: "UserGroupID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationIdentifyingInformation_UserGroupID",
                table: "SpecificationIdentifyingInformation",
                column: "UserGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationIdentifyingInformation_UserID",
                table: "SpecificationIdentifyingInformation",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_GroupName",
                table: "UserGroups",
                column: "GroupName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGroupID",
                table: "Users",
                column: "UserGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID",
                table: "SpecificationIdentifyingInformation",
                column: "UserGroupID",
                principalTable: "UserGroups",
                principalColumn: "UserGroupID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationIdentifyingInformation_Users_UserID",
                table: "SpecificationIdentifyingInformation",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationIdentifyingInformation_Users_UserID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationIdentifyingInformation_UserGroupID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationIdentifyingInformation_UserID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "UserGroupID",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "SpecificationIdentifyingInformation");
        }
    }
}
