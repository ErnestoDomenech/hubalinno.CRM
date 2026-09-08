using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubalinno.CRM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestorPipelineStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PipelineStage",
                table: "InvestorProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PipelineStage",
                table: "InvestorProfiles");
        }
    }
}
