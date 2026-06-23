namespace StudentHub.Models;

public record AdminListColumn(string Path, string Label);

public static class AdminUiMetadata
{
    private static readonly Lazy<Dictionary<Type, AdminListColumn[]>> ListColumns = new(() => new()
    {
        [typeof(Khoa)] = [C("MaKhoa"), C("TenKhoa"), C("MoTa")],
        [typeof(SinhVien)] = [C("MaSinhVien"), C("HoTen"), C("Email"), C("SoDienThoai"), C("Khoa.MaKhoa", "Mã khoa"), C("Khoa.TenKhoa", "Tên khoa"), C("TrangThai"), C("NgayTao")],
        [typeof(GiangVien)] = [C("MaGiangVien"), C("HoTen"), C("Email"), C("SoDienThoai"), C("Khoa.MaKhoa", "Mã khoa"), C("Khoa.TenKhoa", "Tên khoa"), C("HocVi"), C("NgayTao")],
        [typeof(MonHoc)] = [C("MaMonHoc"), C("TenMonHoc"), C("SoTinChi"), C("Khoa.MaKhoa", "Mã khoa"), C("Khoa.TenKhoa", "Tên khoa"), C("MoTa")],
        [typeof(PhongHoc)] = [C("MaPhong"), C("TenPhong"), C("SucChua"), C("ViTri")],
        [typeof(LopHoc)] = [C("MaLop"), C("TenLop"), C("MonHoc.MaMonHoc", "Mã môn học"), C("MonHoc.TenMonHoc", "Tên môn học"), C("GiangVien.MaGiangVien", "Mã giảng viên"), C("GiangVien.HoTen", "Tên giảng viên"), C("HocKy"), C("NamHoc"), C("SoLuongToiDa"), C("TrangThai")],
        [typeof(LichHoc)] = [C("LopHoc.MaLop", "Mã lớp"), C("LopHoc.TenLop", "Tên lớp"), C("PhongHoc.MaPhong", "Mã phòng"), C("PhongHoc.TenPhong", "Tên phòng"), C("ThuTrongTuan"), C("GioBatDau"), C("GioKetThuc"), C("NgayBatDau"), C("NgayKetThuc")],
        [typeof(DangKyHoc)] = [C("SinhVien.MaSinhVien", "Mã sinh viên"), C("SinhVien.HoTen", "Tên sinh viên"), C("LopHoc.MaLop", "Mã lớp"), C("LopHoc.TenLop", "Tên lớp"), C("HocKy"), C("NamHoc"), C("LaHocVuot"), C("NgayDangKy"), C("TrangThai")],
        [typeof(TaiKhoan)] = [C("TenDangNhap"), C("HoTen"), C("Email"), C("VaiTro"), C("TrangThai"), C("NgayTao")]
    });

    private static readonly Dictionary<string, string> Labels = new()
    {
        ["MaKhoa"] = "Mã khoa", ["TenKhoa"] = "Tên khoa", ["MoTa"] = "Mô tả",
        ["MaSinhVien"] = "Mã sinh viên", ["MaGiangVien"] = "Mã giảng viên",
        ["MaMonHoc"] = "Mã môn học", ["TenMonHoc"] = "Tên môn học", ["SoTinChi"] = "Số tín chỉ",
        ["MaPhong"] = "Mã phòng", ["TenPhong"] = "Tên phòng", ["SucChua"] = "Sức chứa", ["ViTri"] = "Vị trí",
        ["MaLop"] = "Mã lớp", ["TenLop"] = "Tên lớp", ["MonHoc"] = "Môn học", ["MonHocId"] = "Môn học",
        ["GiangVien"] = "Giảng viên", ["GiangVienId"] = "Giảng viên", ["HocKy"] = "Học kỳ", ["NamHoc"] = "Năm học",
        ["SoLuongToiDa"] = "Sĩ số tối đa", ["LopHoc"] = "Lớp học", ["LopHocId"] = "Lớp học",
        ["PhongHoc"] = "Phòng học", ["PhongHocId"] = "Phòng học", ["ThuTrongTuan"] = "Thứ trong tuần",
        ["GioBatDau"] = "Giờ bắt đầu", ["GioKetThuc"] = "Giờ kết thúc", ["NgayBatDau"] = "Ngày bắt đầu", ["NgayKetThuc"] = "Ngày kết thúc",
        ["SinhVien"] = "Sinh viên", ["SinhVienId"] = "Sinh viên", ["NgayDangKy"] = "Ngày đăng ký",
        ["LaHocVuot"] = "Học vượt",
        ["HoTen"] = "Họ tên", ["Email"] = "Email", ["SoDienThoai"] = "Số điện thoại", ["NgaySinh"] = "Ngày sinh",
        ["GioiTinh"] = "Giới tính", ["DiaChi"] = "Địa chỉ", ["Khoa"] = "Khoa", ["KhoaId"] = "Khoa",
        ["TaiKhoanId"] = "Tài khoản", ["HocVi"] = "Học vị", ["TrangThai"] = "Trạng thái", ["NgayTao"] = "Ngày tạo",
        ["TenDangNhap"] = "Tên đăng nhập", ["VaiTro"] = "Vai trò"
    };

    private static AdminListColumn C(string path, string? label = null) =>
        new(path, label ?? Label(path));

    public static IReadOnlyList<AdminListColumn> GetListColumns(Type type) => ListColumns.Value[type];

    public static object? GetValue(object item, string path)
    {
        object? value = item;
        foreach (var segment in path.Split('.'))
        {
            if (value == null) return null;
            value = value.GetType().GetProperty(segment)?.GetValue(value);
        }
        return value;
    }

    public static string Label(string propertyName) => Labels.GetValueOrDefault(propertyName.Split('.').Last(), propertyName);
}
