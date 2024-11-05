using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForpostModbusTcpPoller.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Devices_IpAddress",
                table: "Devices",
                column: "IpAddress",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_IpAddress",
                table: "Devices");
        }
    }
}
