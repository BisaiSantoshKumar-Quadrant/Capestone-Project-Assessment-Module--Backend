using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QAssessment_project.Migrations
{
    /// <inheritdoc />
    public partial class santosh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Categories_CategoryId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentScores_Assessments_AssessmentID",
                table: "AssessmentScores");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeResponses_Assessments_AssessmentID",
                table: "EmployeeResponses");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Assessments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Categories_CategoryId",
                table: "Assessments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentScores_Assessments_AssessmentID",
                table: "AssessmentScores",
                column: "AssessmentID",
                principalTable: "Assessments",
                principalColumn: "AssessmentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeResponses_Assessments_AssessmentID",
                table: "EmployeeResponses",
                column: "AssessmentID",
                principalTable: "Assessments",
                principalColumn: "AssessmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Categories_CategoryId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentScores_Assessments_AssessmentID",
                table: "AssessmentScores");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeResponses_Assessments_AssessmentID",
                table: "EmployeeResponses");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Categories_CategoryId",
                table: "Assessments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentScores_Assessments_AssessmentID",
                table: "AssessmentScores",
                column: "AssessmentID",
                principalTable: "Assessments",
                principalColumn: "AssessmentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeResponses_Assessments_AssessmentID",
                table: "EmployeeResponses",
                column: "AssessmentID",
                principalTable: "Assessments",
                principalColumn: "AssessmentID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
