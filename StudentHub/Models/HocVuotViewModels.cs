using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models;

public class GoiYHocVuotViewModel
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTenSinhVien { get; set; } = "";
    public string TenKhoa { get; set; } = "";
    public int MonHocId { get; set; }
    public string MaMonHoc { get; set; } = "";
    public string TenMonHoc { get; set; } = "";
    public int SoTinChi { get; set; }
    public int LopHocId { get; set; }
    public string MaLop { get; set; } = "";
    public string TenLop { get; set; } = "";
    public string GiangVien { get; set; } = "";
    public string LichHoc { get; set; } = "";
    public string HocKyGoiY { get; set; } = "";
    public string NamHocGoiY { get; set; } = "";
    public decimal DiemPhuHop { get; set; }
    public string LyDoGoiY { get; set; } = "";
    public MucDoGoiY MucDoGoiY { get; set; }
    public TrangThaiGoiY TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
    public bool DaDatGioiHan { get; set; }
}

public class SinhVienGoiYHocVuotPageViewModel
{
    public string HocKy { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public decimal Gpa { get; set; }
    public int SoMonDaHocDat { get; set; }
    public int SoMonHocVuotDaDangKy { get; set; }
    public int SoMonHocVuotToiDa { get; set; } = 5;
    public bool DaDatGioiHan => SoMonHocVuotDaDangKy >= SoMonHocVuotToiDa;
    public List<SinhVienHocVuotHocKyOptionViewModel> HocKyOptions { get; set; } = [];
    public List<GoiYHocVuotViewModel> GoiY { get; set; } = [];
}

public class AdminGoiYHocVuotPageViewModel
{
    public int? KhoaId { get; set; }
    public int? SinhVienId { get; set; }
    public string? HocKy { get; set; }
    public string? NamHoc { get; set; }
    public List<Khoa> KhoaOptions { get; set; } = [];
    public List<SinhVien> SinhVienOptions { get; set; } = [];
    public List<SinhVienHocVuotHocKyOptionViewModel> HocKyOptions { get; set; } = [];
    public List<GoiYHocVuotViewModel> GoiY { get; set; } = [];
}

public class AdminMonHocTienQuyetPageViewModel
{
    public string Search { get; set; } = "";
    public int? EditId { get; set; }
    public MonHocTienQuyetFormViewModel Form { get; set; } = new();
    public List<MonHocTienQuyet> Items { get; set; } = [];
    public List<MonHoc> MonHocOptions { get; set; } = [];
}

public class MonHocTienQuyetFormViewModel
{
    public int Id { get; set; }
    [Required] public int MonHocId { get; set; }
    [Required] public int MonHocTienQuyetId { get; set; }
    public bool BatBuoc { get; set; } = true;
    public MucDoTienQuyet MucDo { get; set; } = MucDoTienQuyet.BatBuoc;
    [StringLength(500)] public string? GhiChu { get; set; }
}
