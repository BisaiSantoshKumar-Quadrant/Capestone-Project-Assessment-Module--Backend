using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QAssessment_project.Migrations
{
    /// <inheritdoc />
    public partial class intialsecret11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CategoryId",
                table: "Assessments",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Categories_CategoryId",
                table: "Assessments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Categories_CategoryId",
                table: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_CategoryId",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Assessments");
        }
    }
}
