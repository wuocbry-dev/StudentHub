using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Migrations
{
    /// <inheritdoc />
    public partial class AddNhapDuLieuTuFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LichSuNhapDuLieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    TenFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoaiFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiDuLieu = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TongSoDong = table.Column<int>(type: "int", nullable: false),
                    SoDongHopLe = table.Column<int>(type: "int", nullable: false),
                    SoDongLoi = table.Column<int>(type: "int", nullable: false),
                    SoDongDaNhap = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuNhapDuLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuNhapDuLieu_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DuLieuNhapTam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichSuNhapDuLieuId = table.Column<int>(type: "int", nullable: false),
                    SoDong = table.Column<int>(type: "int", nullable: false),
                    LoaiDuLieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDungJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HopLe = table.Column<bool>(type: "bit", nullable: false),
                    NoiDungLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuLieuNhapTam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuLieuNhapTam_LichSuNhapDuLieu_LichSuNhapDuLieuId",
                        column: x => x.LichSuNhapDuLieuId,
                        principalTable: "LichSuNhapDuLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoiNhapDuLieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LichSuNhapDuLieuId = table.Column<int>(type: "int", nullable: false),
                    SoDong = table.Column<int>(type: "int", nullable: false),
                    NoiDungDong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDungLoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoiNhapDuLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoiNhapDuLieu_LichSuNhapDuLieu_LichSuNhapDuLieuId",
                        column: x => x.LichSuNhapDuLieuId,
                        principalTable: "LichSuNhapDuLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuLieuNhapTam_LichSuNhapDuLieuId_SoDong",
                table: "DuLieuNhapTam",
                columns: new[] { "LichSuNhapDuLieuId", "SoDong" });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhapDuLieu_LoaiDuLieu_TrangThai_NgayNhap",
                table: "LichSuNhapDuLieu",
                columns: new[] { "LoaiDuLieu", "TrangThai", "NgayNhap" });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhapDuLieu_TaiKhoanId",
                table: "LichSuNhapDuLieu",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoiNhapDuLieu_LichSuNhapDuLieuId_SoDong",
                table: "LoiNhapDuLieu",
                columns: new[] { "LichSuNhapDuLieuId", "SoDong" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuLieuNhapTam");

            migrationBuilder.DropTable(
                name: "LoiNhapDuLieu");

            migrationBuilder.DropTable(
                name: "LichSuNhapDuLieu");
        }
    }
}
