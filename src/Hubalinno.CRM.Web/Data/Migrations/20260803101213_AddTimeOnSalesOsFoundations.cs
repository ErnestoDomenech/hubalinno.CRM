using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hubalinno.CRM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeOnSalesOsFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NextActionActivityId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextActionDueDate",
                table: "Opportunities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextActionSubject",
                table: "Opportunities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PipelineStageId",
                table: "Opportunities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecisionRole",
                table: "Contacts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "Contacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastContactAt",
                table: "Contacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredChannel",
                table: "Contacts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusinessLine",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentTimeTrackingSystem",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoNotContact",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCountMax",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCountMin",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IcpScore",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadPriority",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadSource",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PainHypothesis",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountBusinessLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    BusinessLine = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastContactedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBusinessLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBusinessLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessLine = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsWon = table.Column<bool>(type: "bit", nullable: false),
                    IsLost = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineStages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_NextActionActivityId",
                table: "Opportunities",
                column: "NextActionActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_NextActionDueDate",
                table: "Opportunities",
                column: "NextActionDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_PipelineStageId",
                table: "Opportunities",
                column: "PipelineStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_BusinessLine_Status_DueDate",
                table: "Activities",
                columns: new[] { "BusinessLine", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ContactId",
                table: "Activities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBusinessLines_AccountId_BusinessLine",
                table: "AccountBusinessLines",
                columns: new[] { "AccountId", "BusinessLine" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineStages_BusinessLine_Key",
                table: "PipelineStages",
                columns: new[] { "BusinessLine", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Contacts_ContactId",
                table: "Activities",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Activities_NextActionActivityId",
                table: "Opportunities",
                column: "NextActionActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_PipelineStages_PipelineStageId",
                table: "Opportunities",
                column: "PipelineStageId",
                principalTable: "PipelineStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Contacts_ContactId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Activities_NextActionActivityId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_PipelineStages_PipelineStageId",
                table: "Opportunities");

            migrationBuilder.DropTable(
                name: "AccountBusinessLines");

            migrationBuilder.DropTable(
                name: "PipelineStages");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_NextActionActivityId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_NextActionDueDate",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_PipelineStageId",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_BusinessLine_Status_DueDate",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ContactId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "NextActionActivityId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NextActionDueDate",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "NextActionSubject",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PipelineStageId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "DecisionRole",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LastContactAt",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredChannel",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "BusinessLine",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CurrentTimeTrackingSystem",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DoNotContact",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EmployeeCountMax",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EmployeeCountMin",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IcpScore",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LeadPriority",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LeadSource",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PainHypothesis",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Accounts");
        }
    }
}
