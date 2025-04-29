using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QAssessment_project.Migrations
{
    /// <inheritdoc />
    public partial class santosh03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "AssessmentScores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReattemptCount",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "AssessmentScores");

            migrationBuilder.DropColumn(
                name: "ReattemptCount",
                table: "Assessments");
        }
    }
}
