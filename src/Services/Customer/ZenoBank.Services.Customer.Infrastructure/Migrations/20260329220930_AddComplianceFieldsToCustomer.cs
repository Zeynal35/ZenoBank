using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenoBank.Services.Customer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceFieldsToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlacklistReason",
                table: "CustomerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlacklisted",
                table: "CustomerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RiskLevel",
                table: "CustomerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlacklistReason",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "IsBlacklisted",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "CustomerProfiles");
        }
    }
}
