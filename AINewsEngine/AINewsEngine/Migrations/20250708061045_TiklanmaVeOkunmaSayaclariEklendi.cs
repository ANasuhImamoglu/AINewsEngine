using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AINewsEngine.Migrations
{
    /// <inheritdoc />
    public partial class TiklanmaVeOkunmaSayaclariEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OkunmaSayisi",
                table: "Haberler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TiklanmaSayisi",
                table: "Haberler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OkunmaSayisi",
                table: "Haberler");

            migrationBuilder.DropColumn(
                name: "TiklanmaSayisi",
                table: "Haberler");
        }
    }
}
