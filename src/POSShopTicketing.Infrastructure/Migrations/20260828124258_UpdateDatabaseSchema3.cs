using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSShopTicketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchema3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatorName",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatorName",
                table: "Tickets");
        }
    }
}
