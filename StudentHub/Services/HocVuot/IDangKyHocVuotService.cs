using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public interface IDangKyHocVuotService
{
    Task<List<LopHocVuotDangKyViewModel>> LayLopHocVuotCoTheDangKyAsync(int sinhVienId, string hocKy, string namHoc);
    Task<KetQuaDangKyHocVuotViewModel> DangKyHocVuotAsync(int sinhVienId, int lopHocId, string hocKy, string namHoc);
    Task<KetQuaKiemTraHocVuotViewModel> KiemTraDieuKienHocVuotAsync(int sinhVienId, int lopHocId, string hocKy, string namHoc);
}
