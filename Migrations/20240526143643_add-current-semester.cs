using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTable.Migrations
{
    /// <inheritdoc />
    public partial class addcurrentsemester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "Semesters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Semesters",
                keyColumn: "SemesterId",
                keyValue: 1,
                column: "IsCurrent",
                value: false);

            migrationBuilder.UpdateData(
                table: "Semesters",
                keyColumn: "SemesterId",
                keyValue: 2,
                column: "IsCurrent",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "Semesters");
        }
    }
}
