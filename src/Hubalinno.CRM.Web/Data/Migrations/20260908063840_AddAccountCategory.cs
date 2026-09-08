using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubalinno.CRM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountCategory",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountCategory",
                table: "Accounts");
        }
    }
}
