using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Migrations
{
    /// <inheritdoc />
    public partial class AddGiangVienPortalAttendanceGrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyHocId = table.Column<int>(type: "int", nullable: false),
                    DiemChuyenCan = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemGiuaKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemCuoiKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemTongKet = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    XepLoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diem_DangKyHoc_DangKyHocId",
                        column: x => x.DangKyHocId,
                        principalTable: "DangKyHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhienDiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    GiangVienId = table.Column<int>(type: "int", nullable: false),
                    NgayDiemDanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    MaPhien = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    QrToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DangMo = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienDiemDanh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhienDiemDanh_GiangVien_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhienDiemDanh_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhienDiemDanhId = table.Column<int>(type: "int", nullable: false),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianCheckIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemDanh_PhienDiemDanh_PhienDiemDanhId",
                        column: x => x.PhienDiemDanhId,
                        principalTable: "PhienDiemDanh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemDanh_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diem_DangKyHocId",
                table: "Diem",
                column: "DangKyHocId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_PhienDiemDanhId_SinhVienId",
                table: "DiemDanh",
                columns: new[] { "PhienDiemDanhId", "SinhVienId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_SinhVienId",
                table: "DiemDanh",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienDiemDanh_GiangVienId",
                table: "PhienDiemDanh",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienDiemDanh_LopHocId",
                table: "PhienDiemDanh",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienDiemDanh_MaPhien",
                table: "PhienDiemDanh",
                column: "MaPhien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhienDiemDanh_QrToken",
                table: "PhienDiemDanh",
                column: "QrToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diem");

            migrationBuilder.DropTable(
                name: "DiemDanh");

            migrationBuilder.DropTable(
                name: "PhienDiemDanh");
        }
    }
}
