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
    public List<CanhBaoSinhVien> CanhBaoMoiNhat { get; set; } = [];
    public int SoCanhBaoChuaDoc { get; set; }
}

public class SinhVienCanhBaoViewModel
{
    public List<CanhBaoSinhVien> DanhSach { get; set; } = [];
    public int SoChuaDoc => DanhSach.Count(x => !x.DaDoc);
    public bool CoDuLieuCanhBao { get; set; } = true;
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

public class SinhVienLichHocViewModel
{
    public string CheDo { get; set; } = "tuan";
    public DateTime Ngay { get; set; } = DateTime.Today;
    public string? HocKy { get; set; }
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public List<string> HocKyOptions { get; set; } = [];
    public List<SinhVienLichHocItemViewModel> LichHoc { get; set; } = [];
    public SinhVienLichHocItemViewModel? LopTiepTheo { get; set; }
    public int SoLichTrung { get; set; }
}

public class SinhVienLichHocItemViewModel
{
    public int LichHocId { get; set; }
    public DateTime NgayHoc { get; set; }
    public TimeSpan GioBatDau { get; set; }
    public TimeSpan GioKetThuc { get; set; }
    public string TenMonHoc { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string PhongHoc { get; set; } = "Chưa xếp phòng";
    public string GiangVien { get; set; } = "Chưa phân công";
    public string HocKy { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public bool TrungLich { get; set; }
    public bool HomNay => NgayHoc.Date == DateTime.Today;
    public DateTime BatDau => NgayHoc.Date + GioBatDau;
    public DateTime KetThuc => NgayHoc.Date + GioKetThuc;
}

public enum TrangThaiKetQua { Dat, Rot, DangHoc, ThieuDiemCuoiKy }

public class SinhVienBangDiemViewModel
{
    public List<SinhVienBangDiemItemViewModel> DanhSach { get; set; } = [];
    public List<SinhVienGpaHocKyViewModel> GpaTheoHocKy { get; set; } = [];
    public bool CoDuLieuBangDiem { get; set; } = true;
    public bool CoDuDuLieuGpa => GpaTheoHocKy.Count >= 2;
}

public class SinhVienBangDiemItemViewModel
{
    public string TenMonHoc { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string HocKy { get; set; } = "";
    public string NamHoc { get; set; } = "";
    public int SoTinChi { get; set; }
    public decimal? DiemChuyenCan { get; set; }
    public decimal? DiemBaiTap { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTongKet { get; set; }
    public string? DiemChu { get; set; }
    public TrangThaiKetQua TrangThai { get; set; }
}

public class SinhVienGpaHocKyViewModel
{
    public string HocKy { get; set; } = "";
    public decimal Gpa { get; set; }
}

public class SinhVienChuyenCanPageViewModel
{
    public List<SinhVienChuyenCanItemViewModel> DanhSach { get; set; } = [];
    public bool CoDuLieuDiemDanh { get; set; } = true;
}

public class SinhVienChuyenCanItemViewModel
{
    public string TenMonHoc { get; set; } = "";
    public string MaLop { get; set; } = "";
    public int CoMat { get; set; }
    public int DiMuon { get; set; }
    public int Vang { get; set; }
    public int TongSoBuoi { get; set; }
    public double? TyLeChuyenCan { get; set; }
    public bool ChuyenCanThap => TyLeChuyenCan < 70;
}
