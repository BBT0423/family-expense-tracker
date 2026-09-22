using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummaryYearSalary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Transactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Transactions SET CreatedByUserId = UserId WHERE CreatedByUserId = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Transactions");
        }
    }
}
