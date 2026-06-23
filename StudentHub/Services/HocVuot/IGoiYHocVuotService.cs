using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public interface IGoiYHocVuotService
{
    Task<List<GoiYHocVuotViewModel>> TaoGoiYChoSinhVienAsync(int sinhVienId, string hocKy, string namHoc);
    Task<List<GoiYHocVuotViewModel>> LayGoiYCuaSinhVienAsync(int sinhVienId);
    Task CapNhatTrangThaiGoiYAsync(int goiYHocVuotId, string trangThai);
    Task<bool> KiemTraMonTienQuyetAsync(int sinhVienId, int monHocId);
    Task<bool> KiemTraTrungLichAsync(int sinhVienId, int lopHocId);
    Task<decimal> TinhGPAAsync(int sinhVienId);
    Task<(bool ThanhCong, string ThongBao)> DangKyTuGoiYAsync(int sinhVienId, int goiYHocVuotId);
}
