using Microsoft.EntityFrameworkCore;
using StudentHub.Models;

namespace StudentHub.Data;

public class SimsDbContext(DbContextOptions<SimsDbContext> options) : DbContext(options)
{
    public DbSet<TaiKhoan> TaiKhoan => Set<TaiKhoan>();
    public DbSet<Khoa> Khoa => Set<Khoa>();
    public DbSet<SinhVien> SinhVien => Set<SinhVien>();
    public DbSet<GiangVien> GiangVien => Set<GiangVien>();
    public DbSet<MonHoc> MonHoc => Set<MonHoc>();
    public DbSet<PhongHoc> PhongHoc => Set<PhongHoc>();
    public DbSet<LopHoc> LopHoc => Set<LopHoc>();
    public DbSet<LichHoc> LichHoc => Set<LichHoc>();
    public DbSet<DangKyHoc> DangKyHoc => Set<DangKyHoc>();
    public DbSet<PhienDiemDanh> PhienDiemDanh => Set<PhienDiemDanh>();
    public DbSet<DiemDanh> DiemDanh => Set<DiemDanh>();
    public DbSet<BangDiem> BangDiem => Set<BangDiem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        foreach (var entity in b.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.ClrType.Name);
            foreach (var property in entity.GetProperties()) property.SetColumnName(property.Name);
        }
        b.Entity<TaiKhoan>().HasIndex(x => x.TenDangNhap).IsUnique();
        b.Entity<Khoa>().HasIndex(x => x.MaKhoa).IsUnique();
        b.Entity<SinhVien>().HasIndex(x => x.MaSinhVien).IsUnique();
        b.Entity<GiangVien>().HasIndex(x => x.MaGiangVien).IsUnique();
        b.Entity<MonHoc>().HasIndex(x => x.MaMonHoc).IsUnique();
        b.Entity<PhongHoc>().HasIndex(x => x.MaPhong).IsUnique();
        b.Entity<LopHoc>().HasIndex(x => x.MaLop).IsUnique();
        b.Entity<DangKyHoc>().HasIndex(x => new { x.SinhVienId, x.LopHocId }).IsUnique();
        b.Entity<PhienDiemDanh>().HasIndex(x => x.MaPhien).IsUnique();
        b.Entity<PhienDiemDanh>().HasIndex(x => x.QrToken).IsUnique();
        b.Entity<DiemDanh>().HasIndex(x => new { x.PhienDiemDanhId, x.SinhVienId }).IsUnique();
        b.Entity<BangDiem>().HasIndex(x => new { x.SinhVienId, x.LopHocId }).IsUnique();
        b.Entity<BangDiem>().Property(x => x.DiemChuyenCan).HasPrecision(4, 2);
        b.Entity<BangDiem>().Property(x => x.DiemBaiTap).HasPrecision(4, 2);
        b.Entity<BangDiem>().Property(x => x.DiemGiuaKy).HasPrecision(4, 2);
        b.Entity<BangDiem>().Property(x => x.DiemCuoiKy).HasPrecision(4, 2);
        b.Entity<BangDiem>().Property(x => x.DiemTongKet).HasPrecision(4, 2);

        foreach (var fk in b.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        b.Entity<TaiKhoan>().Property(x => x.VaiTro).HasConversion<string>();
        b.Entity<TaiKhoan>().Property(x => x.TrangThai).HasConversion<string>();
        b.Entity<SinhVien>().Property(x => x.GioiTinh).HasConversion<string>();
        b.Entity<SinhVien>().Property(x => x.TrangThai).HasConversion<string>();
        b.Entity<LopHoc>().Property(x => x.TrangThai).HasConversion<string>();
        b.Entity<DangKyHoc>().Property(x => x.TrangThai).HasConversion<string>();
        b.Entity<DiemDanh>().Property(x => x.TrangThai).HasConversion<string>();

        var admin = new TaiKhoan
        {
            Id = 1, TenDangNhap = "admin", HoTen = "Quan tri he thong", Email = "admin@sims.edu",
            VaiTro = VaiTro.Admin, TrangThai = TrangThaiTaiKhoan.HoatDong,
            NgayTao = new DateTime(2026, 1, 1),
            MatKhauHash = "AQAAAAIAAYagAAAAELJ93BmJta2AOWPv9xfXLLrTHv+they+ddyb08DDifof1TJY2qkgmhI7IopjW8HTlw=="
        };
        b.Entity<TaiKhoan>().HasData(admin);
    }
}
