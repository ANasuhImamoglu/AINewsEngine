using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AINewsEngine.Migrations
{
    /// <inheritdoc />
    public partial class HaberlerTablosuOlustur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Haberler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Icerik = table.Column<string>(type: "TEXT", nullable: true),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: true),
                    YayinTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Onaylandi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Haberler", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Haberler");
        }
    }
}
