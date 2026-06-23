using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Migrations
{
    /// <inheritdoc />
    public partial class AddGoiYMonHocVuot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoiYHocVuot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    HocKyGoiY = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NamHocGoiY = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiemPhuHop = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LyDoGoiY = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MucDoGoiY = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiYHocVuot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoiYHocVuot_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoiYHocVuot_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoiYHocVuot_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonHocTienQuyet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    MonHocTienQuyetId = table.Column<int>(type: "int", nullable: false),
                    BatBuoc = table.Column<bool>(type: "bit", nullable: false),
                    MucDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocTienQuyet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonHocTienQuyet_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonHocTienQuyet_MonHoc_MonHocTienQuyetId",
                        column: x => x.MonHocTienQuyetId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoiYHocVuot_HocKyGoiY_NamHocGoiY_MucDoGoiY_TrangThai",
                table: "GoiYHocVuot",
                columns: new[] { "HocKyGoiY", "NamHocGoiY", "MucDoGoiY", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_GoiYHocVuot_LopHocId",
                table: "GoiYHocVuot",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiYHocVuot_MonHocId",
                table: "GoiYHocVuot",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiYHocVuot_SinhVienId_LopHocId_HocKyGoiY_NamHocGoiY",
                table: "GoiYHocVuot",
                columns: new[] { "SinhVienId", "LopHocId", "HocKyGoiY", "NamHocGoiY" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTienQuyet_MonHocId_MonHocTienQuyetId",
                table: "MonHocTienQuyet",
                columns: new[] { "MonHocId", "MonHocTienQuyetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTienQuyet_MonHocTienQuyetId",
                table: "MonHocTienQuyet",
                column: "MonHocTienQuyetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoiYHocVuot");

            migrationBuilder.DropTable(
                name: "MonHocTienQuyet");
        }
    }
}
