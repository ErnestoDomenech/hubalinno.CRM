using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubalinno.CRM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestorCRMFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Opportunities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "OpportunityType",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InvestorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    InvestmentStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypicalTicketMin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TypicalTicketMax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Thesis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Geography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThesisFit = table.Column<int>(type: "int", nullable: false),
                    AccessScore = table.Column<int>(type: "int", nullable: false),
                    PortfolioConflict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarmIntroRoute = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipTemperature = table.Column<int>(type: "int", nullable: false),
                    LastInteraction = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextActionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestorProfiles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestorProfiles_AccountId",
                table: "InvestorProfiles",
                column: "AccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestorProfiles");

            migrationBuilder.DropColumn(
                name: "OpportunityType",
                table: "Opportunities");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Opportunities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
