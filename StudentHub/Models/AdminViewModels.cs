using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models;

public class DangNhapViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";
    public bool GhiNho { get; set; }
}

public class CrudPageViewModel
{
    public required Type EntityType { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyList<object> Items { get; init; }
    public string Search { get; init; } = "";
    public int Page { get; init; }
    public int TotalPages { get; init; }
}

public class CrudEditViewModel
{
    public required Type EntityType { get; init; }
    public required string Title { get; init; }
    public required object Entity { get; init; }
    public bool IsEdit { get; init; }
}

public class DashboardViewModel
{
    public int TongSinhVien { get; set; }
    public int TongGiangVien { get; set; }
    public int TongMonHoc { get; set; }
    public int TongLopHoc { get; set; }
    public int SinhVienDangHoc { get; set; }
    public List<string> KhoaLabels { get; set; } = [];
    public List<int> KhoaValues { get; set; } = [];
    public List<string> HocKyLabels { get; set; } = [];
    public List<int> HocKyValues { get; set; } = [];
    public List<SinhVien> SinhVienMoi { get; set; } = [];
    public List<LopHoc> LopHocMoi { get; set; } = [];
}
