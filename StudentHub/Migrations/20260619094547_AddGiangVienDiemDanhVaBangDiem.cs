using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Migrations
{
    /// <inheritdoc />
    public partial class AddGiangVienDiemDanhVaBangDiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BangDiem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    DiemChuyenCan = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemBaiTap = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemGiuaKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemCuoiKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemTongKet = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemChu = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangDiem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangDiem_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BangDiem_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangDiem_LopHocId",
                table: "BangDiem",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_BangDiem_SinhVienId_LopHocId",
                table: "BangDiem",
                columns: new[] { "SinhVienId", "LopHocId" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO BangDiem (SinhVienId, LopHocId, DiemChuyenCan, DiemGiuaKy, DiemCuoiKy, DiemTongKet, NgayCapNhat)
                SELECT dk.SinhVienId, dk.LopHocId, d.DiemChuyenCan, d.DiemGiuaKy, d.DiemCuoiKy, d.DiemTongKet, d.NgayCapNhat
                FROM Diem d
                INNER JOIN DangKyHoc dk ON dk.Id = d.DangKyHocId;
                """);

            migrationBuilder.DropTable(
                name: "Diem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangDiem");

            migrationBuilder.CreateTable(
                name: "Diem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyHocId = table.Column<int>(type: "int", nullable: false),
                    DiemChuyenCan = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemCuoiKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemGiuaKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemTongKet = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    XepLoai = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Diem_DangKyHocId",
                table: "Diem",
                column: "DangKyHocId",
                unique: true);
        }
    }
}
