using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models;

public class SinhVienDashboardViewModel
{
    public string HoTen { get; set; } = "";
    public string MaSinhVien { get; set; } = "";
    public int TongMonDangHoc { get; set; }
    public decimal? Gpa { get; set; }
    public double? TyLeChuyenCan { get; set; }
    public int SoLopHomNay { get; set; }
    public int SoMonNguyCoRot { get; set; }
    public List<LichHoc> LopHomNay { get; set; } = [];
}

public class HoSoSinhVienViewModel
{
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";

    [Display(Name = "Số điện thoại"), Phone(ErrorMessage = "Số điện thoại không hợp lệ."), StringLength(20)]
    public string? SoDienThoai { get; set; }

    public DateTime? NgaySinh { get; set; }
    public GioiTinh GioiTinh { get; set; }

    [Display(Name = "Địa chỉ"), StringLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
    public string? DiaChi { get; set; }

    public string TenKhoa { get; set; } = "";
    public TrangThaiSinhVien TrangThai { get; set; }
}

public class SinhVienPlaceholderViewModel
{
    public string TieuDe { get; set; } = "";
    public string MoTa { get; set; } = "";
    public string Icon { get; set; } = "bi-hourglass-split";
}
