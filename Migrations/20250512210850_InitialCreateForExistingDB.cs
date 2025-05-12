using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateForExistingDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoreInvoiceModel",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BusinessTerm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Cardinality = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SemanticDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreInvoiceModel", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ExtensionComponentsModelHeader",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ECLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtensionComponentsModelHeader", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SpecificationIdentifyingInformation",
                columns: table => new
                {
                    IdentityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecificationIdentifier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SpecificationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubSector = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecificationVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactInformation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfImplementation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GoverningEntity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoreVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecificationSourceLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsCountrySpecification = table.Column<bool>(type: "bit", nullable: false),
                    UnderlyingSpecificationIdentifier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PreferredSyntax = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationIdentifyingInformation", x => x.IdentityID);
                });

            migrationBuilder.CreateTable(
                name: "ExtensionComponentModelElements",
                columns: table => new
                {
                    EntityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtensionComponentID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BusinessTermID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BusinessTerm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Cardinality = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SemanticDescription = table.Column<string>(type: "text", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExtensionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtensionComponentModelElements", x => x.EntityID);
                    table.UniqueConstraint("AK_ExtensionComponentModelElements_ExtensionComponentID_BusinessTermID", x => new { x.ExtensionComponentID, x.BusinessTermID });
                    table.ForeignKey(
                        name: "FK_ExtensionComponentModelElements_ExtensionComponentsModelHeader_ExtensionComponentID",
                        column: x => x.ExtensionComponentID,
                        principalTable: "ExtensionComponentsModelHeader",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpecificationCore",
                columns: table => new
                {
                    EntityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityID = table.Column<int>(type: "int", nullable: false),
                    BusinessTermID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Cardinality = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsageNote = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TypeOfChange = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationCore", x => x.EntityID);
                    table.ForeignKey(
                        name: "FK_SpecificationCore_CoreInvoiceModel_BusinessTermID",
                        column: x => x.BusinessTermID,
                        principalTable: "CoreInvoiceModel",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpecificationCore_SpecificationIdentifyingInformation_IdentityID",
                        column: x => x.IdentityID,
                        principalTable: "SpecificationIdentifyingInformation",
                        principalColumn: "IdentityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecificationExtensionComponents",
                columns: table => new
                {
                    EntityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityID = table.Column<int>(type: "int", nullable: false),
                    ExtensionComponentID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BusinessTermID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Cardinality = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsageNote = table.Column<string>(type: "text", nullable: true),
                    Justification = table.Column<string>(type: "text", nullable: true),
                    TypeOfExtension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationExtensionComponents", x => x.EntityID);
                    table.ForeignKey(
                        name: "FK_SpecificationExtensionComponents_ExtensionComponentModelElements_ExtensionComponentID_BusinessTermID",
                        columns: x => new { x.ExtensionComponentID, x.BusinessTermID },
                        principalTable: "ExtensionComponentModelElements",
                        principalColumns: new[] { "ExtensionComponentID", "BusinessTermID" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpecificationExtensionComponents_SpecificationIdentifyingInformation_IdentityID",
                        column: x => x.IdentityID,
                        principalTable: "SpecificationIdentifyingInformation",
                        principalColumn: "IdentityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ExtensionComponentModelElements_Component_BusinessTerm",
                table: "ExtensionComponentModelElements",
                columns: new[] { "ExtensionComponentID", "BusinessTermID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationCore_BusinessTermID",
                table: "SpecificationCore",
                column: "BusinessTermID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationCore_IdentityID",
                table: "SpecificationCore",
                column: "IdentityID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationExtensionComponents_ExtensionComponentID_BusinessTermID",
                table: "SpecificationExtensionComponents",
                columns: new[] { "ExtensionComponentID", "BusinessTermID" });

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationExtensionComponents_IdentityID",
                table: "SpecificationExtensionComponents",
                column: "IdentityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecificationCore");

            migrationBuilder.DropTable(
                name: "SpecificationExtensionComponents");

            migrationBuilder.DropTable(
                name: "CoreInvoiceModel");

            migrationBuilder.DropTable(
                name: "ExtensionComponentModelElements");

            migrationBuilder.DropTable(
                name: "SpecificationIdentifyingInformation");

            migrationBuilder.DropTable(
                name: "ExtensionComponentsModelHeader");
        }
    }
}
