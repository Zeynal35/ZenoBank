using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenoBank.Services.Loan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisbursementAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DisbursementAccountId",
                table: "LoanApplications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisbursementAccountId",
                table: "LoanApplications");
        }
    }
}
