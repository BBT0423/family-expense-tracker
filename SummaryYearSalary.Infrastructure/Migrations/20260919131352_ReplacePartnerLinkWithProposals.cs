using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummaryYearSalary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePartnerLinkWithProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerLinks");

            // Old notification types (partner flow) were renumbered; existing rows are test data.
            migrationBuilder.Sql("DELETE FROM Notifications");

            migrationBuilder.CreateTable(
                name: "TransactionProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProposedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionProposals_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionProposals_CategoryId",
                table: "TransactionProposals",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionProposals_OwnerUserId_Status",
                table: "TransactionProposals",
                columns: new[] { "OwnerUserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionProposals");

            migrationBuilder.CreateTable(
                name: "PartnerLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InviteCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PartnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RequesterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerLinks_InviteCode",
                table: "PartnerLinks",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerLinks_PartnerUserId",
                table: "PartnerLinks",
                column: "PartnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerLinks_RequesterUserId",
                table: "PartnerLinks",
                column: "RequesterUserId");
        }
    }
}
