using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models;

public enum VaiTro { Admin, GiangVien, SinhVien }
public enum TrangThaiTaiKhoan { HoatDong, Khoa }
public enum TrangThaiSinhVien { DangHoc, BaoLuu, DaTotNghiep, ThoiHoc }
public enum GioiTinh { Nam, Nu, Khac }
public enum TrangThaiLopHoc { SapMo, DangHoc, DaKetThuc, DaHuy }
public enum TrangThaiDangKy { ChoDuyet, DaDuyet, TuChoi, DaHuy }
public enum TrangThaiDiemDanh { CoMat, DiMuon, Vang, CoPhep }
public enum LoaiCanhBao { ChuyenCanThap, DiemThap, TrungLichHoc, ThieuDiem }
public enum MucDo { ThongTin, CanhBao, NguyHiem }
public enum MucDoTienQuyet { BatBuoc, KhuyenNghi, NenHocTruoc }
public enum MucDoGoiY { RatPhuHop, PhuHop, CanCanNhac, KhongPhuHop }
public enum TrangThaiGoiY { Moi, DaXem, DaDangKy, DaBoQua, HetHan }

public class TaiKhoan
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string TenDangNhap { get; set; } = "";
    [Required] public string MatKhauHash { get; set; } = "";
    [Required, StringLength(150)] public string HoTen { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required] public VaiTro VaiTro { get; set; }
    [Required] public TrangThaiTaiKhoan TrangThai { get; set; } = TrangThaiTaiKhoan.HoatDong;
    public DateTime NgayTao { get; set; } = DateTime.Now;
}

public class Khoa
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaKhoa { get; set; } = "";
    [Required, StringLength(150)] public string TenKhoa { get; set; } = "";
    [StringLength(500)] public string? MoTa { get; set; }
    public ICollection<SinhVien> SinhViens { get; set; } = [];
}

public class SinhVien
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaSinhVien { get; set; } = "";
    [Required, StringLength(150)] public string HoTen { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Phone, StringLength(20)] public string? SoDienThoai { get; set; }
    [DataType(DataType.Date)] public DateTime? NgaySinh { get; set; }
    public GioiTinh GioiTinh { get; set; }
    [StringLength(300)] public string? DiaChi { get; set; }
    [Required] public int KhoaId { get; set; }
    public Khoa? Khoa { get; set; }
    public int? TaiKhoanId { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }
    public TrangThaiSinhVien TrangThai { get; set; } = TrangThaiSinhVien.DangHoc;
    public DateTime NgayTao { get; set; } = DateTime.Now;
}

public class GiangVien
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaGiangVien { get; set; } = "";
    [Required, StringLength(150)] public string HoTen { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Phone, StringLength(20)] public string? SoDienThoai { get; set; }
    [Required] public int KhoaId { get; set; }
    public Khoa? Khoa { get; set; }
    public int? TaiKhoanId { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }
    [StringLength(100)] public string? HocVi { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
}

public class MonHoc
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaMonHoc { get; set; } = "";
    [Required, StringLength(150)] public string TenMonHoc { get; set; } = "";
    [Range(1, 20)] public int SoTinChi { get; set; }
    [Required] public int KhoaId { get; set; }
    public Khoa? Khoa { get; set; }
    [StringLength(500)] public string? MoTa { get; set; }
}

public class MonHocTienQuyet
{
    public int Id { get; set; }
    [Required] public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }
    [Required] public int MonHocTienQuyetId { get; set; }
    public MonHoc? MonHocTienQuyetCuaMon { get; set; }
    public bool BatBuoc { get; set; } = true;
    public MucDoTienQuyet MucDo { get; set; } = MucDoTienQuyet.BatBuoc;
    [StringLength(500)] public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
}

public class PhongHoc
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaPhong { get; set; } = "";
    [Required, StringLength(150)] public string TenPhong { get; set; } = "";
    [Range(1, 1000)] public int SucChua { get; set; }
    [StringLength(200)] public string? ViTri { get; set; }
}

public class LopHoc
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaLop { get; set; } = "";
    [Required, StringLength(150)] public string TenLop { get; set; } = "";
    [Required] public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }
    [Required] public int GiangVienId { get; set; }
    public GiangVien? GiangVien { get; set; }
    [Required, StringLength(20)] public string HocKy { get; set; } = "";
    [Required, StringLength(20)] public string NamHoc { get; set; } = "";
    [Range(1, 1000)] public int SoLuongToiDa { get; set; }
    public TrangThaiLopHoc TrangThai { get; set; } = TrangThaiLopHoc.SapMo;
}

public class LichHoc
{
    public int Id { get; set; }
    [Required] public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }
    [Required] public int PhongHocId { get; set; }
    public PhongHoc? PhongHoc { get; set; }
    [Range(2, 8)] public int ThuTrongTuan { get; set; }
    [Required] public TimeSpan GioBatDau { get; set; }
    [Required] public TimeSpan GioKetThuc { get; set; }
    [DataType(DataType.Date)] public DateTime NgayBatDau { get; set; }
    [DataType(DataType.Date)] public DateTime NgayKetThuc { get; set; }
}

public class DangKyHoc
{
    public int Id { get; set; }
    [Required] public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    [Required] public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }
    [Required, StringLength(20)] public string HocKy { get; set; } = "";
    [Required, StringLength(20)] public string NamHoc { get; set; } = "";
    public bool LaHocVuot { get; set; }
    public DateTime NgayDangKy { get; set; } = DateTime.Now;
    public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;
}

public class PhienDiemDanh
{
    public int Id { get; set; }
    [Required] public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }
    [Required] public int GiangVienId { get; set; }
    public GiangVien? GiangVien { get; set; }
    [DataType(DataType.Date)] public DateTime NgayDiemDanh { get; set; }
    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }
    [Required, StringLength(6, MinimumLength = 6)] public string MaPhien { get; set; } = "";
    [Required, StringLength(100)] public string QrToken { get; set; } = "";
    public bool DangMo { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.Now;
}

public class DiemDanh
{
    public int Id { get; set; }
    [Required] public int PhienDiemDanhId { get; set; }
    public PhienDiemDanh? PhienDiemDanh { get; set; }
    [Required] public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    public TrangThaiDiemDanh TrangThai { get; set; } = TrangThaiDiemDanh.Vang;
    public DateTime? ThoiGianCheckIn { get; set; }
    [StringLength(500)] public string? GhiChu { get; set; }
}

public class BangDiem
{
    public int Id { get; set; }
    [Required] public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    [Required] public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }
    [Range(0, 10)] public decimal? DiemChuyenCan { get; set; }
    [Range(0, 10)] public decimal? DiemBaiTap { get; set; }
    [Range(0, 10)] public decimal? DiemGiuaKy { get; set; }
    [Range(0, 10)] public decimal? DiemCuoiKy { get; set; }
    [Range(0, 10)] public decimal? DiemTongKet { get; set; }
    [StringLength(1)] public string? DiemChu { get; set; }
    public DateTime NgayCapNhat { get; set; } = DateTime.Now;
}

public class GoiYHocVuot
{
    public int Id { get; set; }
    [Required] public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    [Required] public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }
    [Required] public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }
    [Required, StringLength(20)] public string HocKyGoiY { get; set; } = "";
    [Required, StringLength(20)] public string NamHocGoiY { get; set; } = "";
    [Range(0, 100)] public decimal DiemPhuHop { get; set; }
    [Required, StringLength(1000)] public string LyDoGoiY { get; set; } = "";
    public MucDoGoiY MucDoGoiY { get; set; } = MucDoGoiY.PhuHop;
    public TrangThaiGoiY TrangThai { get; set; } = TrangThaiGoiY.Moi;
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime NgayCapNhat { get; set; } = DateTime.Now;
}

public class CanhBaoSinhVien
{
    public int Id { get; set; }
    [Required] public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    [Required] public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }
    [Required] public LoaiCanhBao LoaiCanhBao { get; set; }
    [Required, StringLength(500)] public string NoiDung { get; set; } = "";
    [Required] public MucDo MucDo { get; set; }
    public bool DaDoc { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
}
