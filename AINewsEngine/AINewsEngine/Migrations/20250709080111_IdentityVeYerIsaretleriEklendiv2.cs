using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AINewsEngine.Migrations
{
    /// <inheritdoc />
    public partial class IdentityVeYerIsaretleriEklendiv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Haberler_Kategoriler_KategoriId",
                table: "Haberler");

            migrationBuilder.AlterColumn<int>(
                name: "KategoriId",
                table: "Haberler",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Haberler_Kategoriler_KategoriId",
                table: "Haberler",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Haberler_Kategoriler_KategoriId",
                table: "Haberler");

            migrationBuilder.AlterColumn<int>(
                name: "KategoriId",
                table: "Haberler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Haberler_Kategoriler_KategoriId",
                table: "Haberler",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
