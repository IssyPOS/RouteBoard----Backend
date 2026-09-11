using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSShopTicketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "OrganizationContacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "OrganizationContacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "OrganizationContacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "OrganizationContacts");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "OrganizationContacts");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "OrganizationContacts");
        }
    }
}
