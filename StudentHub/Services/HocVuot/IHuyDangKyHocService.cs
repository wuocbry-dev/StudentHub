using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public interface IHuyDangKyHocService
{
    Task<KetQuaThaoTacDangKyViewModel> HuyDangKyAsync(int sinhVienId, int dangKyHocId, string? lyDoHuy);
    Task<KetQuaThaoTacDangKyViewModel> DangKyLaiAsync(int sinhVienId, int dangKyHocId);
    Task<bool> ConTrongThoiGianHuyDangKyAsync(int lopHocId);
    Task<DateTime?> LayNgayBatDauLopHocAsync(int lopHocId);
    Task<List<SinhVienHocVuotItemViewModel>> LayDangKyHocVuotCuaSinhVienAsync(int sinhVienId, string hocKy, string namHoc);
}
