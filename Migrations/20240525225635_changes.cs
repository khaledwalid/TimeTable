using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTable.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Settings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "SettingId",
                keyValue: 1,
                columns: new[] { "SpecificStartTimesJson", "Type" },
                values: new object[] { "[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"]", "2-day" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "SettingId",
                keyValue: 2,
                columns: new[] { "SpecificStartTimesJson", "Type" },
                values: new object[] { "[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"]", "1-day" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Settings");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "SettingId",
                keyValue: 1,
                column: "SpecificStartTimesJson",
                value: "{\"1\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"],\"2\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"],\"3\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"],\"4\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"]}");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "SettingId",
                keyValue: 2,
                column: "SpecificStartTimesJson",
                value: "{\"1\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"2\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"3\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"4\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"5\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"]}");
        }
    }
}
