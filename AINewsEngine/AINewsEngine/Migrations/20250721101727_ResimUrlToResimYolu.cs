using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AINewsEngine.Migrations
{
    /// <inheritdoc />
    public partial class ResimUrlToResimYolu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResimUrl",
                table: "Haberler",
                newName: "ResimYolu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResimYolu",
                table: "Haberler",
                newName: "ResimUrl");
        }
    }
}
