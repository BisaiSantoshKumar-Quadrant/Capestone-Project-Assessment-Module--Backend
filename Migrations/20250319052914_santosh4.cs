using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QAssessment_project.Migrations
{
    /// <inheritdoc />
    public partial class santosh4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                columns: new[] { "JoinedDate", "Password" },
                values: new object[] { new DateTime(2025, 3, 19, 5, 29, 14, 225, DateTimeKind.Utc).AddTicks(7927), "$2a$11$bEDymHrpLakbTUZw30WoxOHWlQqCjDeAjaQch.uCbEmH9KijW1Wze" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                columns: new[] { "JoinedDate", "Password" },
                values: new object[] { new DateTime(2025, 3, 19, 5, 28, 57, 751, DateTimeKind.Utc).AddTicks(6274), "$2a$11$IDhjzgYgRlebQFBaPt59vucGItKIan2gxWAGfoI4fwga4nCoKy0HG" });
        }
    }
}
