using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models;

public class GiangVienDashboardViewModel
{
    public int TongLop { get; set; }
    public int TongSinhVien { get; set; }
    public int TongPhienDiemDanh { get; set; }
    public double TyLeChuyenCan { get; set; }
    public List<LichHoc> LopHomNay { get; set; } = [];
    public List<LopVangNhieuViewModel> LopVangNhieu { get; set; } = [];
}

public class LopVangNhieuViewModel
{
    public required LopHoc LopHoc { get; set; }
    public int LuotVang { get; set; }
    public double TyLeVang { get; set; }
}

public class LopCuaToiItemViewModel
{
    public required LopHoc LopHoc { get; set; }
    public int SoSinhVien { get; set; }
    public int SoBuoiDiemDanh { get; set; }
}

public class LopChiTietViewModel
{
    public required LopHoc LopHoc { get; set; }
    public List<LichHoc> LichHoc { get; set; } = [];
    public List<SinhVienChuyenCanViewModel> SinhVien { get; set; } = [];
    public string Search { get; set; } = "";
}

public class SinhVienChuyenCanViewModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public int TongBuoi { get; set; }
    public int CoMat { get; set; }
    public int DiMuon { get; set; }
    public int Vang { get; set; }
    public int CoPhep { get; set; }
    public double TyLeChuyenCan { get; set; }
}

public class TaoPhienDiemDanhViewModel
{
    [Required] public int LopHocId { get; set; }
    [Required, DataType(DataType.Date)] public DateTime NgayDiemDanh { get; set; } = DateTime.Today;
    [Required] public TimeSpan GioBatDau { get; set; } = DateTime.Now.TimeOfDay;
    [Required] public TimeSpan GioKetThuc { get; set; } = DateTime.Now.AddMinutes(15).TimeOfDay;
}

public class PhienDiemDanhChiTietViewModel
{
    public required PhienDiemDanh Phien { get; set; }
    public List<DiemDanh> DanhSach { get; set; } = [];
    public string CheckInUrl { get; set; } = "";
    public long BatDauUnixMs { get; set; }
    public long KetThucUnixMs { get; set; }
    public long ServerNowUnixMs { get; set; }
}

public class CheckInViewModel
{
    public string Token { get; set; } = "";
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã phiên phải gồm 6 ký tự.")]
    public string? MaPhien { get; set; }
    public bool ThanhCong { get; set; }
    public string? ThongBao { get; set; }
}

public class QuanLyDiemViewModel
{
    public required LopHoc LopHoc { get; set; }
    public List<DiemSinhVienViewModel> DanhSach { get; set; } = [];
    public string Search { get; set; } = "";
}

public class DiemSinhVienViewModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public decimal? DiemChuyenCan { get; set; }
    public decimal? DiemBaiTap { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTongKet { get; set; }
    public string? DiemChu { get; set; }
}
