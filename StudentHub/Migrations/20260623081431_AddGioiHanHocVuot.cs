using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHub.Migrations
{
    /// <inheritdoc />
    public partial class AddGioiHanHocVuot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HocKy",
                table: "DangKyHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LaHocVuot",
                table: "DangKyHoc",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NamHoc",
                table: "DangKyHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHoc_SinhVienId_HocKy_NamHoc_LaHocVuot",
                table: "DangKyHoc",
                columns: new[] { "SinhVienId", "HocKy", "NamHoc", "LaHocVuot" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DangKyHoc_SinhVienId_HocKy_NamHoc_LaHocVuot",
                table: "DangKyHoc");

            migrationBuilder.DropColumn(
                name: "HocKy",
                table: "DangKyHoc");

            migrationBuilder.DropColumn(
                name: "LaHocVuot",
                table: "DangKyHoc");

            migrationBuilder.DropColumn(
                name: "NamHoc",
                table: "DangKyHoc");
        }
    }
}
