using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotProject.API.Migrations
{
    /// <inheritdoc />
    public partial class DeviceDataUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "DeviceData",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "DeviceData");
        }
    }
}
