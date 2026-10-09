using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppProject.Core.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class FinancialAccountChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_Name",
                table: "FinancialAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_Name",
                table: "FinancialAccounts",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_Name",
                table: "FinancialAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_Name",
                table: "FinancialAccounts",
                column: "Name");
        }
    }
}
