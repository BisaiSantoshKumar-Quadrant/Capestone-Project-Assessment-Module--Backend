using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QAssessment_project.Migrations
{
    /// <inheritdoc />
    public partial class zxfesxcd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionConduct",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionConduct",
                table: "Assessments");
        }
    }
}
