using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AINewsEngine.Migrations
{
    /// <inheritdoc />
    public partial class YorumlarSistemiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Yorumlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Icerik = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Onaylandi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HaberId = table.Column<int>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yorumlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Yorumlar_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Yorumlar_Haberler_HaberId",
                        column: x => x.HaberId,
                        principalTable: "Haberler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YorumYanitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Icerik = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Onaylandi = table.Column<bool>(type: "INTEGER", nullable: false),
                    YorumId = table.Column<int>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YorumYanitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YorumYanitlari_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YorumYanitlari_Yorumlar_YorumId",
                        column: x => x.YorumId,
                        principalTable: "Yorumlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YorumLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsLike = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: false),
                    YorumId = table.Column<int>(type: "INTEGER", nullable: true),
                    YorumYanitiId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YorumLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YorumLikes_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YorumLikes_YorumYanitlari_YorumYanitiId",
                        column: x => x.YorumYanitiId,
                        principalTable: "YorumYanitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YorumLikes_Yorumlar_YorumId",
                        column: x => x.YorumId,
                        principalTable: "Yorumlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_HaberId",
                table: "Yorumlar",
                column: "HaberId");

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_KullaniciId",
                table: "Yorumlar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_YorumLikes_KullaniciId_YorumId",
                table: "YorumLikes",
                columns: new[] { "KullaniciId", "YorumId" },
                unique: true,
                filter: "[YorumId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_YorumLikes_KullaniciId_YorumYanitiId",
                table: "YorumLikes",
                columns: new[] { "KullaniciId", "YorumYanitiId" },
                unique: true,
                filter: "[YorumYanitiId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_YorumLikes_YorumId",
                table: "YorumLikes",
                column: "YorumId");

            migrationBuilder.CreateIndex(
                name: "IX_YorumLikes_YorumYanitiId",
                table: "YorumLikes",
                column: "YorumYanitiId");

            migrationBuilder.CreateIndex(
                name: "IX_YorumYanitlari_KullaniciId",
                table: "YorumYanitlari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_YorumYanitlari_YorumId",
                table: "YorumYanitlari",
                column: "YorumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YorumLikes");

            migrationBuilder.DropTable(
                name: "YorumYanitlari");

            migrationBuilder.DropTable(
                name: "Yorumlar");
        }
    }
}
