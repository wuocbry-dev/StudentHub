using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public class HuyDangKyHocService(
    SimsDbContext db,
    IGioiHanHocVuotService gioiHanHocVuotService,
    IDangKyHocVuotService dangKyHocVuotService) : IHuyDangKyHocService
{
    private const int SoNgayKhoaTruocKhiHoc = 10;
    private const string QuaHanMessage = "Đã quá thời hạn hủy/đăng ký lại môn học.";
    private const string KhongCoNgayBatDauMessage = "Không xác định được ngày bắt đầu lớp học.";

    public async Task<KetQuaThaoTacDangKyViewModel> HuyDangKyAsync(int sinhVienId, int dangKyHocId, string? lyDoHuy)
    {
        var dangKy = await db.DangKyHoc
            .Include(x => x.LopHoc)
            .SingleOrDefaultAsync(x => x.Id == dangKyHocId && x.SinhVienId == sinhVienId);
        if (dangKy == null) return await KetQuaLoiAsync(sinhVienId, "", "", "Không tìm thấy đăng ký học.");
        if (dangKy.TrangThai is not (TrangThaiDangKy.ChoDuyet or TrangThaiDangKy.DaDuyet or TrangThaiDangKy.DaDangKy))
            return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, "Trạng thái hiện tại không cho phép hủy đăng ký.");

        var (hopLe, lyDo) = await KiemTraThoiHanAsync(dangKy.LopHocId);
        if (!hopLe) return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, lyDo);

        dangKy.TrangThai = TrangThaiDangKy.DaHuy;
        await db.SaveChangesAsync();

        return new KetQuaThaoTacDangKyViewModel
        {
            ThanhCong = true,
            ThongBao = "Đã hủy đăng ký môn học.",
            TrangThaiMoi = TrangThaiDangKy.DaHuy.ToString(),
            SoMonHocVuotDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc),
            SoMonHocVuotToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(dangKy.HocKy, dangKy.NamHoc)
        };
    }

    public async Task<KetQuaThaoTacDangKyViewModel> DangKyLaiAsync(int sinhVienId, int dangKyHocId)
    {
        var dangKy = await db.DangKyHoc
            .Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc)
            .SingleOrDefaultAsync(x => x.Id == dangKyHocId && x.SinhVienId == sinhVienId);
        if (dangKy == null) return await KetQuaLoiAsync(sinhVienId, "", "", "Không tìm thấy đăng ký học.");
        if (dangKy.TrangThai != TrangThaiDangKy.DaHuy)
            return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, "Chỉ có thể đăng ký lại môn đã hủy.");

        var (hopLe, lyDo) = await KiemTraThoiHanAsync(dangKy.LopHocId);
        if (!hopLe) return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, lyDo);

        var soLuong = await DemSoLuongDangKyLopAsync(dangKy.LopHocId);
        if (dangKy.LopHoc != null && soLuong >= dangKy.LopHoc.SoLuongToiDa)
            return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, "Lớp học đã đủ số lượng.");

        if (await BiTrungLichAsync(sinhVienId, dangKy.LopHocId, dangKy.Id))
            return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, "Lớp học bị trùng lịch với lớp đã đăng ký.");

        if (dangKy.LaHocVuot)
        {
            if (!await gioiHanHocVuotService.CoTheDangKyHocVuotAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc))
                return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc,
                    await gioiHanHocVuotService.LayThongBaoGioiHanAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc));

            var dieuKienHocVuot = await dangKyHocVuotService.KiemTraDieuKienHocVuotAsync(sinhVienId, dangKy.LopHocId, dangKy.HocKy, dangKy.NamHoc);
            if (!dieuKienHocVuot.HopLe)
                return await KetQuaLoiAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc, dieuKienHocVuot.ThongBao);
        }

        dangKy.TrangThai = TrangThaiDangKy.ChoDuyet;
        dangKy.NgayDangKy = DateTime.Now;
        await db.SaveChangesAsync();

        return new KetQuaThaoTacDangKyViewModel
        {
            ThanhCong = true,
            ThongBao = "Đăng ký lại môn học thành công. Trạng thái đăng ký đang chờ duyệt.",
            TrangThaiMoi = TrangThaiDangKy.ChoDuyet.ToString(),
            SoMonHocVuotDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVienId, dangKy.HocKy, dangKy.NamHoc),
            SoMonHocVuotToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(dangKy.HocKy, dangKy.NamHoc)
        };
    }

    public async Task<List<SinhVienHocVuotItemViewModel>> LayDangKyHocVuotCuaSinhVienAsync(int sinhVienId, string hocKy, string namHoc)
    {
        hocKy = ChuanHoa(hocKy);
        namHoc = ChuanHoa(namHoc);

        var rows = await db.DangKyHoc.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId
                && x.LaHocVuot
                && x.HocKy.Trim() == hocKy
                && x.NamHoc.Trim() == namHoc
                && x.TrangThai != TrangThaiDangKy.TuChoi)
            .Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc)
            .Include(x => x.LopHoc).ThenInclude(x => x!.GiangVien)
            .OrderByDescending(x => x.NgayDangKy)
            .ToListAsync();

        var lopIds = rows.Select(x => x.LopHocId).Distinct().ToList();
        var lichHoc = await db.LichHoc.AsNoTracking()
            .Where(x => lopIds.Contains(x.LopHocId))
            .Include(x => x.PhongHoc)
            .ToListAsync();

        var result = new List<SinhVienHocVuotItemViewModel>();
        foreach (var row in rows)
        {
            var ngayBatDau = await LayNgayBatDauLopHocAsync(row.LopHocId);
            var han = ngayBatDau?.Date.AddDays(-SoNgayKhoaTruocKhiHoc);
            var conHan = han.HasValue && DateTime.Today <= han.Value;
            var soNgayConLai = han.HasValue ? (int?)(han.Value - DateTime.Today).Days : null;
            var lichText = FormatLichHoc(lichHoc.Where(x => x.LopHocId == row.LopHocId).ToList());

            result.Add(new SinhVienHocVuotItemViewModel
            {
                DangKyHocId = row.Id,
                LopHocId = row.LopHocId,
                MaLop = row.LopHoc?.MaLop ?? "",
                TenLop = row.LopHoc?.TenLop ?? "",
                MaMonHoc = row.LopHoc?.MonHoc?.MaMonHoc ?? "",
                TenMonHoc = row.LopHoc?.MonHoc?.TenMonHoc ?? "",
                SoTinChi = row.LopHoc?.MonHoc?.SoTinChi ?? 0,
                GiangVien = row.LopHoc?.GiangVien?.HoTen ?? "Chưa phân công",
                LichHocText = lichText,
                HocKy = row.HocKy,
                NamHoc = row.NamHoc,
                NgayDangKy = row.NgayDangKy,
                NgayBatDau = ngayBatDau,
                HanHuyDangKy = han,
                ConDuocHuy = conHan && row.TrangThai is TrangThaiDangKy.ChoDuyet or TrangThaiDangKy.DaDuyet or TrangThaiDangKy.DaDangKy,
                ConDuocDangKyLai = conHan && row.TrangThai == TrangThaiDangKy.DaHuy,
                SoNgayConLai = soNgayConLai,
                LaHocVuot = row.LaHocVuot,
                TrangThai = row.TrangThai,
                LyDoKhongTheHuy = TaoLyDoKhongTheThaoTac(row.TrangThai, conHan, ngayBatDau, "hủy"),
                LyDoKhongTheDangKyLai = TaoLyDoKhongTheThaoTac(row.TrangThai, conHan, ngayBatDau, "đăng ký lại")
            });
        }

        return result;
    }

    public async Task<bool> ConTrongThoiGianHuyDangKyAsync(int lopHocId)
    {
        var (hopLe, _) = await KiemTraThoiHanAsync(lopHocId);
        return hopLe;
    }

    public Task<DateTime?> LayNgayBatDauLopHocAsync(int lopHocId)
    {
        return db.LichHoc.AsNoTracking()
            .Where(x => x.LopHocId == lopHocId)
            .OrderBy(x => x.NgayBatDau)
            .Select(x => (DateTime?)x.NgayBatDau.Date)
            .FirstOrDefaultAsync();
    }

    private async Task<(bool HopLe, string LyDo)> KiemTraThoiHanAsync(int lopHocId)
    {
        var ngayBatDau = await LayNgayBatDauLopHocAsync(lopHocId);
        if (ngayBatDau == null) return (false, KhongCoNgayBatDauMessage);
        var han = ngayBatDau.Value.Date.AddDays(-SoNgayKhoaTruocKhiHoc);
        return DateTime.Today <= han ? (true, "") : (false, QuaHanMessage);
    }

    private Task<int> DemSoLuongDangKyLopAsync(int lopHocId)
    {
        return db.DangKyHoc.AsNoTracking()
            .CountAsync(x => x.LopHocId == lopHocId
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
    }

    private async Task<bool> BiTrungLichAsync(int sinhVienId, int lopHocId, int dangKyHocIdBoQua)
    {
        var lichLopMoi = await db.LichHoc.AsNoTracking().Where(x => x.LopHocId == lopHocId).ToListAsync();
        if (lichLopMoi.Count == 0) return false;

        var lopDaDangKyIds = db.DangKyHoc.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId
                && x.Id != dangKyHocIdBoQua
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi)
            .Select(x => x.LopHocId);

        var lichDangKy = await db.LichHoc.AsNoTracking()
            .Where(x => lopDaDangKyIds.Contains(x.LopHocId))
            .ToListAsync();

        return lichLopMoi.Any(a => lichDangKy.Any(b => TrungLich(a, b)));
    }

    private async Task<KetQuaThaoTacDangKyViewModel> KetQuaLoiAsync(int sinhVienId, string hocKy, string namHoc, string thongBao)
    {
        return new KetQuaThaoTacDangKyViewModel
        {
            ThanhCong = false,
            ThongBao = thongBao,
            SoMonHocVuotDaDangKy = string.IsNullOrWhiteSpace(hocKy) || string.IsNullOrWhiteSpace(namHoc)
                ? 0
                : await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVienId, hocKy, namHoc),
            SoMonHocVuotToiDa = string.IsNullOrWhiteSpace(hocKy) || string.IsNullOrWhiteSpace(namHoc)
                ? 0
                : await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(hocKy, namHoc)
        };
    }

    private static string TaoLyDoKhongTheThaoTac(TrangThaiDangKy trangThai, bool conHan, DateTime? ngayBatDau, string thaoTac)
    {
        if (ngayBatDau == null) return KhongCoNgayBatDauMessage;
        if (!conHan) return QuaHanMessage;
        return thaoTac == "hủy"
            ? trangThai == TrangThaiDangKy.DaHuy ? "Môn học đã hủy." : ""
            : trangThai != TrangThaiDangKy.DaHuy ? "Chỉ có thể đăng ký lại môn đã hủy." : "";
    }

    private static string FormatLichHoc(List<LichHoc> lich)
    {
        if (lich.Count == 0) return "Chưa xếp lịch";
        return string.Join("; ", lich.OrderBy(x => x.ThuTrongTuan).ThenBy(x => x.GioBatDau)
            .Select(x => $"Thứ {x.ThuTrongTuan}, {x.GioBatDau:hh\\:mm}-{x.GioKetThuc:hh\\:mm}, {x.PhongHoc?.MaPhong ?? "Chưa xếp phòng"}"));
    }

    private static bool TrungLich(LichHoc a, LichHoc b)
    {
        return a.ThuTrongTuan == b.ThuTrongTuan
            && a.NgayBatDau.Date <= b.NgayKetThuc.Date
            && b.NgayBatDau.Date <= a.NgayKetThuc.Date
            && a.GioBatDau < b.GioKetThuc
            && b.GioBatDau < a.GioKetThuc;
    }

    private static string ChuanHoa(string value) => value.Trim();
}
