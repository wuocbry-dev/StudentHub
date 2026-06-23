namespace StudentHub.Services.HocVuot;

public interface IGioiHanHocVuotService
{
    Task<int> LaySoMonHocVuotDaDangKyAsync(int sinhVienId, string hocKy, string namHoc);
    Task<int> LaySoMonHocVuotToiDaAsync(string hocKy, string namHoc);
    Task<bool> CoTheDangKyHocVuotAsync(int sinhVienId, string hocKy, string namHoc);
    Task<string> LayThongBaoGioiHanAsync(int sinhVienId, string hocKy, string namHoc);
}
