using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummaryYearSalary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    InviteCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerLinks");
        }
    }
}
