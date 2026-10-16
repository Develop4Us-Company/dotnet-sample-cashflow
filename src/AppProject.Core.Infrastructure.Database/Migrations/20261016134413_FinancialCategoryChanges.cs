using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppProject.Core.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class FinancialCategoryChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialCategories_Name",
                table: "FinancialCategories");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialCategories_Name",
                table: "FinancialCategories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialCategories_Name",
                table: "FinancialCategories");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialCategories_Name",
                table: "FinancialCategories",
                column: "Name");
        }
    }
}
