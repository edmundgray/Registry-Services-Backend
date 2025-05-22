using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServices.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusFieldsToSpecificationIdentifyingInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImplementationStatus",
                table: "SpecificationIdentifyingInformation",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationStatus",
                table: "SpecificationIdentifyingInformation",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImplementationStatus",
                table: "SpecificationIdentifyingInformation");

            migrationBuilder.DropColumn(
                name: "RegistrationStatus",
                table: "SpecificationIdentifyingInformation");
        }
    }
}
