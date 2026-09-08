using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubalinno.CRM.Web.Migrations
{
    /// <inheritdoc />
    public partial class BackfillInvestorAccountCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            UPDATE Accounts
            SET AccountCategory = 1
            WHERE Id IN (
                SELECT AccountId
                FROM InvestorProfiles
            );
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            UPDATE Accounts
            SET AccountCategory = 0
            WHERE Id IN (
                SELECT AccountId
                FROM InvestorProfiles
            );
        """);
        }
    }
}
