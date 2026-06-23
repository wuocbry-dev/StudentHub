using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public class GioiHanHocVuotService(SimsDbContext db) : IGioiHanHocVuotService
{
    private const int SoMonHocVuotToiDaMacDinh = 5;

    public Task<int> LaySoMonHocVuotDaDangKyAsync(int sinhVienId, string hocKy, string namHoc)
    {
        hocKy = ChuanHoa(hocKy);
        namHoc = ChuanHoa(namHoc);

        return db.DangKyHoc
            .AsNoTracking()
            .CountAsync(x => x.SinhVienId == sinhVienId
                && x.LaHocVuot
                && x.HocKy.Trim() == hocKy
                && x.NamHoc.Trim() == namHoc
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
    }

    public Task<int> LaySoMonHocVuotToiDaAsync(string hocKy, string namHoc)
    {
        return Task.FromResult(SoMonHocVuotToiDaMacDinh);
    }

    public async Task<bool> CoTheDangKyHocVuotAsync(int sinhVienId, string hocKy, string namHoc)
    {
        var soDaDangKy = await LaySoMonHocVuotDaDangKyAsync(sinhVienId, hocKy, namHoc);
        var soToiDa = await LaySoMonHocVuotToiDaAsync(hocKy, namHoc);
        return soDaDangKy < soToiDa;
    }

    public async Task<string> LayThongBaoGioiHanAsync(int sinhVienId, string hocKy, string namHoc)
    {
        var soDaDangKy = await LaySoMonHocVuotDaDangKyAsync(sinhVienId, hocKy, namHoc);
        var soToiDa = await LaySoMonHocVuotToiDaAsync(hocKy, namHoc);
        return soDaDangKy >= soToiDa
            ? $"Sinh viên không được đăng ký quá {soToiDa} môn học vượt trong học kỳ {hocKy} năm học {namHoc}."
            : $"Sinh viên đã đăng ký {soDaDangKy}/{soToiDa} môn học vượt trong học kỳ {hocKy} năm học {namHoc}.";
    }

    private static string ChuanHoa(string value) => value.Trim();
}
