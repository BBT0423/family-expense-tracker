using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummaryYearSalary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneOtpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneOtpCode",
                table: "IdentityUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneOtpExpiry",
                table: "IdentityUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhoneOtpRequestCount",
                table: "IdentityUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneOtpWindowStart",
                table: "IdentityUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneOtpCode",
                table: "IdentityUsers");

            migrationBuilder.DropColumn(
                name: "PhoneOtpExpiry",
                table: "IdentityUsers");

            migrationBuilder.DropColumn(
                name: "PhoneOtpRequestCount",
                table: "IdentityUsers");

            migrationBuilder.DropColumn(
                name: "PhoneOtpWindowStart",
                table: "IdentityUsers");
        }
    }
}
