using System.Security.Claims;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers;

[Authorize(Roles = "SinhVien")]
[Route("SinhVien")]
public class SinhVienController(SimsDbContext db) : Controller
{
    [HttpGet("")]
    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var sinhVien = await GetSinhVien().SingleOrDefaultAsync();
        if (sinhVien == null) return View("ChuaLienKet");

        var dangKy = db.DangKyHoc
            .Where(x => x.SinhVienId == sinhVien.Id && x.TrangThai != TrangThaiDangKy.DaHuy);
        var lopDangHocIds = dangKy
            .Where(x => x.LopHoc!.TrangThai == TrangThaiLopHoc.DangHoc)
            .Select(x => x.LopHocId);
        var tongMonDangHoc = await dangKy
            .Where(x => x.LopHoc!.TrangThai == TrangThaiLopHoc.DangHoc)
            .Select(x => x.LopHoc!.MonHocId)
            .Distinct()
            .CountAsync();

        var homNay = DateTime.Today;
        var thu = homNay.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)homNay.DayOfWeek + 1;
        var lichHomNay = await db.LichHoc
            .AsNoTracking()
            .Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc)
            .Include(x => x.PhongHoc)
            .Where(x => lopDangHocIds.Contains(x.LopHocId)
                && x.ThuTrongTuan == thu
                && x.NgayBatDau.Date <= homNay
                && x.NgayKetThuc.Date >= homNay)
            .OrderBy(x => x.GioBatDau)
            .ToListAsync();

        decimal? gpa = null;
        var soMonNguyCoRot = 0;
        try
        {
            var diem = await db.BangDiem
                .AsNoTracking()
                .Where(x => x.SinhVienId == sinhVien.Id && x.DiemTongKet.HasValue)
                .Select(x => new { x.DiemTongKet, SoTinChi = x.LopHoc!.MonHoc!.SoTinChi })
                .ToListAsync();
            var tongTinChi = diem.Sum(x => x.SoTinChi);
            if (tongTinChi > 0)
                gpa = Math.Round(diem.Sum(x => x.DiemTongKet!.Value * x.SoTinChi) / tongTinChi, 2);
            soMonNguyCoRot = diem.Count(x => x.DiemTongKet < 4);
        }
        catch (DbException)
        {
            // BangDiem is an optional module in older database versions.
        }

        double? tyLeChuyenCan = null;
        try
        {
            var diemDanh = await db.DiemDanh.AsNoTracking()
                .Where(x => x.SinhVienId == sinhVien.Id)
                .Select(x => x.TrangThai)
                .ToListAsync();
            var soBuoiTinhChuyenCan = diemDanh.Count;
            if (soBuoiTinhChuyenCan > 0)
            {
                var soBuoiDat = diemDanh.Count(x => x is TrangThaiDiemDanh.CoMat or TrangThaiDiemDanh.DiMuon);
                tyLeChuyenCan = soBuoiDat * 100d / soBuoiTinhChuyenCan;
            }
        }
        catch (DbException)
        {
            // DiemDanh is an optional module in older database versions.
        }

        return View(new SinhVienDashboardViewModel
        {
            HoTen = sinhVien.HoTen,
            MaSinhVien = sinhVien.MaSinhVien,
            TongMonDangHoc = tongMonDangHoc,
            Gpa = gpa,
            TyLeChuyenCan = tyLeChuyenCan,
            SoLopHomNay = lichHomNay.Count,
            SoMonNguyCoRot = soMonNguyCoRot,
            LopHomNay = lichHomNay
        });
    }

    [HttpGet("HoSo")]
    public async Task<IActionResult> HoSo()
    {
        var sinhVien = await GetSinhVien().Include(x => x.Khoa).SingleOrDefaultAsync();
        if (sinhVien == null) return View("ChuaLienKet");
        return View(ToHoSoViewModel(sinhVien));
    }

    [HttpPost("HoSo"), ValidateAntiForgeryToken]
    public async Task<IActionResult> HoSo(HoSoSinhVienViewModel model)
    {
        var sinhVien = await GetSinhVien().Include(x => x.Khoa).SingleOrDefaultAsync();
        if (sinhVien == null) return View("ChuaLienKet");

        if (!ModelState.IsValid)
        {
            CopyReadOnlyProfile(model, sinhVien);
            return View(model);
        }

        sinhVien.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
        sinhVien.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();
        await db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật hồ sơ thành công.";
        return RedirectToAction(nameof(HoSo));
    }

    [HttpGet("LichHoc")]
    public async Task<IActionResult> LichHoc(string cheDo = "tuan", DateTime? ngay = null, string? hocKy = null)
    {
        var sinhVien = await GetSinhVien().AsNoTracking().SingleOrDefaultAsync();
        if (sinhVien == null) return View("ChuaLienKet");

        cheDo = cheDo is "thang" or "hocky" ? cheDo : "tuan";
        var ngayChon = (ngay ?? DateTime.Today).Date;
        var lichCoSo = await GetLichHocQuery(sinhVien.Id).ToListAsync();
        var hocKyOptions = lichCoSo.Select(x => $"{x.LopHoc!.HocKy} - {x.LopHoc.NamHoc}")
            .Distinct().OrderByDescending(x => x).ToList();
        if (cheDo == "hocky" && string.IsNullOrWhiteSpace(hocKy)) hocKy = hocKyOptions.FirstOrDefault();

        var (tuNgay, denNgay) = GetKhoangThoiGian(cheDo, ngayChon, hocKy, lichCoSo);
        var lichDaLoc = FilterHocKy(lichCoSo, hocKy, cheDo == "hocky");
        var lichHoc = TaoCacBuoiHoc(lichDaLoc, tuNgay, denNgay);
        DanhDauTrungLich(lichHoc);

        var tuHomNay = DateTime.Now;
        var denTuongLai = lichCoSo.Count == 0 ? DateTime.Today.AddDays(30) : lichCoSo.Max(x => x.NgayKetThuc).Date;
        var lopTiepTheo = TaoCacBuoiHoc(lichCoSo, DateTime.Today, denTuongLai)
            .Where(x => x.BatDau >= tuHomNay).OrderBy(x => x.BatDau).FirstOrDefault();

        return View(new SinhVienLichHocViewModel
        {
            CheDo = cheDo, Ngay = ngayChon, HocKy = hocKy, TuNgay = tuNgay, DenNgay = denNgay,
            HocKyOptions = hocKyOptions, LichHoc = lichHoc, LopTiepTheo = lopTiepTheo,
            SoLichTrung = lichHoc.Count(x => x.TrungLich)
        });
    }

    [HttpGet("GetLichHocEvents")]
    public async Task<IActionResult> GetLichHocEvents(DateTime start, DateTime end, string? hocKy = null)
    {
        var sinhVien = await GetSinhVien().AsNoTracking().SingleOrDefaultAsync();
        if (sinhVien == null) return Json(Array.Empty<object>());
        var lichCoSo = await GetLichHocQuery(sinhVien.Id).ToListAsync();
        var items = TaoCacBuoiHoc(FilterHocKy(lichCoSo, hocKy, !string.IsNullOrWhiteSpace(hocKy)), start.Date, end.Date.AddDays(-1));
        DanhDauTrungLich(items);
        return Json(items.Select(x => new
        {
            id = $"{x.LichHocId}-{x.NgayHoc:yyyyMMdd}",
            title = $"{x.TenMonHoc} - {x.MaLop}",
            start = x.BatDau.ToString("yyyy-MM-ddTHH:mm:ss"),
            end = x.KetThuc.ToString("yyyy-MM-ddTHH:mm:ss"),
            classNames = x.TrungLich ? new[] { "calendar-conflict" } : Array.Empty<string>(),
            extendedProps = new { x.PhongHoc, x.GiangVien, x.TrungLich }
        }));
    }

    [HttpGet("BangDiem")]
    public async Task<IActionResult> BangDiem()
    {
        var sinhVien = await GetSinhVien().AsNoTracking().SingleOrDefaultAsync();
        if (sinhVien == null) return View("ChuaLienKet");
        var model = new SinhVienBangDiemViewModel();
        try
        {
            var rows = await db.DangKyHoc.AsNoTracking()
                .Where(x => x.SinhVienId == sinhVien.Id && x.TrangThai != TrangThaiDangKy.DaHuy)
                .Select(x => new
                {
                    Lop = x.LopHoc!, Mon = x.LopHoc!.MonHoc!,
                    Diem = db.BangDiem.FirstOrDefault(d => d.SinhVienId == sinhVien.Id && d.LopHocId == x.LopHocId)
                }).OrderByDescending(x => x.Lop.NamHoc).ThenByDescending(x => x.Lop.HocKy).ToListAsync();

            model.DanhSach = rows.Select(x => new SinhVienBangDiemItemViewModel
            {
                TenMonHoc = x.Mon.TenMonHoc, MaLop = x.Lop.MaLop, HocKy = x.Lop.HocKy,
                NamHoc = x.Lop.NamHoc, SoTinChi = x.Mon.SoTinChi,
                DiemChuyenCan = x.Diem?.DiemChuyenCan, DiemBaiTap = x.Diem?.DiemBaiTap,
                DiemGiuaKy = x.Diem?.DiemGiuaKy, DiemCuoiKy = x.Diem?.DiemCuoiKy,
                DiemTongKet = x.Diem?.DiemTongKet, DiemChu = x.Diem?.DiemChu,
                TrangThai = GetTrangThaiKetQua(x.Lop.TrangThai, x.Diem)
            }).ToList();

            model.GpaTheoHocKy = model.DanhSach.Where(x => x.DiemTongKet.HasValue)
                .GroupBy(x => $"{x.HocKy} - {x.NamHoc}")
                .Select(g => new SinhVienGpaHocKyViewModel
                {
                    HocKy = g.Key,
                    Gpa = Math.Round(g.Sum(x => x.DiemTongKet!.Value * x.SoTinChi) / g.Sum(x => x.SoTinChi), 2)
                }).OrderBy(x => x.HocKy).ToList();
        }
        catch (DbException)
        {
            model.CoDuLieuBangDiem = false;
        }
        return View(model);
    }

    [HttpGet("ChuyenCan")]
    public async Task<IActionResult> ChuyenCan()
    {
        var sinhVien = await GetSinhVien().AsNoTracking().SingleOrDefaultAsync();
        if (sinhVien == null) return View("ChuaLienKet");
        var model = new SinhVienChuyenCanPageViewModel();
        try
        {
            var lop = await db.DangKyHoc.AsNoTracking()
                .Where(x => x.SinhVienId == sinhVien.Id && x.TrangThai != TrangThaiDangKy.DaHuy
                    && x.LopHoc!.TrangThai == TrangThaiLopHoc.DangHoc)
                .Select(x => new { x.LopHocId, x.LopHoc!.MaLop, TenMonHoc = x.LopHoc.MonHoc!.TenMonHoc })
                .OrderBy(x => x.MaLop).ToListAsync();
            var lopIds = lop.Select(x => x.LopHocId).ToList();
            var diemDanh = await db.DiemDanh.AsNoTracking()
                .Where(x => x.SinhVienId == sinhVien.Id && lopIds.Contains(x.PhienDiemDanh!.LopHocId))
                .Select(x => new { x.PhienDiemDanh!.LopHocId, x.TrangThai }).ToListAsync();
            model.DanhSach = lop.Select(x =>
            {
                var ds = diemDanh.Where(d => d.LopHocId == x.LopHocId).ToList();
                var tong = ds.Count;
                return new SinhVienChuyenCanItemViewModel
                {
                    TenMonHoc = x.TenMonHoc, MaLop = x.MaLop, TongSoBuoi = tong,
                    CoMat = ds.Count(d => d.TrangThai == TrangThaiDiemDanh.CoMat),
                    DiMuon = ds.Count(d => d.TrangThai == TrangThaiDiemDanh.DiMuon),
                    Vang = ds.Count(d => d.TrangThai == TrangThaiDiemDanh.Vang),
                    TyLeChuyenCan = tong == 0 ? null : (ds.Count(d => d.TrangThai is TrangThaiDiemDanh.CoMat or TrangThaiDiemDanh.DiMuon) * 100d / tong)
                };
            }).ToList();
        }
        catch (DbException)
        {
            model.CoDuLieuDiemDanh = false;
        }
        return View(model);
    }

    [HttpGet("CanhBao")]
    public IActionResult CanhBao() => Placeholder("Cảnh báo học tập", "Hệ thống chưa có dữ liệu cảnh báo sinh viên.", "bi-exclamation-triangle");

    private IQueryable<Models.SinhVien> GetSinhVien()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
            return db.SinhVien.Where(x => false);
        return db.SinhVien.Where(x => x.TaiKhoanId == taiKhoanId);
    }

    private IQueryable<Models.LichHoc> GetLichHocQuery(int sinhVienId)
    {
        var lopIds = db.DangKyHoc.Where(x => x.SinhVienId == sinhVienId && x.TrangThai != TrangThaiDangKy.DaHuy)
            .Select(x => x.LopHocId);
        return db.LichHoc.AsNoTracking().Include(x => x.PhongHoc)
            .Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc)
            .Include(x => x.LopHoc).ThenInclude(x => x!.GiangVien)
            .Where(x => lopIds.Contains(x.LopHocId));
    }

    private static (DateTime TuNgay, DateTime DenNgay) GetKhoangThoiGian(string cheDo, DateTime ngay,
        string? hocKy, List<Models.LichHoc> lich)
    {
        if (cheDo == "thang")
        {
            var dauThang = new DateTime(ngay.Year, ngay.Month, 1);
            return (dauThang, dauThang.AddMonths(1).AddDays(-1));
        }
        if (cheDo == "hocky")
        {
            var ds = FilterHocKy(lich, hocKy, true);
            return ds.Count == 0 ? (ngay, ngay) : (ds.Min(x => x.NgayBatDau).Date, ds.Max(x => x.NgayKetThuc).Date);
        }
        var offset = ((int)ngay.DayOfWeek + 6) % 7;
        var dauTuan = ngay.AddDays(-offset);
        return (dauTuan, dauTuan.AddDays(6));
    }

    private static List<Models.LichHoc> FilterHocKy(List<Models.LichHoc> lich, string? hocKy, bool apply)
    {
        if (!apply || string.IsNullOrWhiteSpace(hocKy)) return lich;
        return lich.Where(x => $"{x.LopHoc!.HocKy} - {x.LopHoc.NamHoc}" == hocKy).ToList();
    }

    private static List<SinhVienLichHocItemViewModel> TaoCacBuoiHoc(IEnumerable<Models.LichHoc> lich,
        DateTime tuNgay, DateTime denNgay)
    {
        var result = new List<SinhVienLichHocItemViewModel>();
        if (denNgay < tuNgay) return result;
        foreach (var item in lich)
        {
            var batDau = item.NgayBatDau.Date > tuNgay.Date ? item.NgayBatDau.Date : tuNgay.Date;
            var ketThuc = item.NgayKetThuc.Date < denNgay.Date ? item.NgayKetThuc.Date : denNgay.Date;
            for (var ngay = batDau; ngay <= ketThuc; ngay = ngay.AddDays(1))
            {
                var thu = ngay.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)ngay.DayOfWeek + 1;
                if (thu != item.ThuTrongTuan) continue;
                result.Add(new SinhVienLichHocItemViewModel
                {
                    LichHocId = item.Id, NgayHoc = ngay, GioBatDau = item.GioBatDau, GioKetThuc = item.GioKetThuc,
                    TenMonHoc = item.LopHoc?.MonHoc?.TenMonHoc ?? item.LopHoc?.TenLop ?? "Môn học",
                    MaLop = item.LopHoc?.MaLop ?? "", PhongHoc = item.PhongHoc?.MaPhong ?? "Chưa xếp phòng",
                    GiangVien = item.LopHoc?.GiangVien?.HoTen ?? "Chưa phân công",
                    HocKy = item.LopHoc?.HocKy ?? "", NamHoc = item.LopHoc?.NamHoc ?? ""
                });
            }
        }
        return result.OrderBy(x => x.BatDau).ToList();
    }

    private static void DanhDauTrungLich(List<SinhVienLichHocItemViewModel> lich)
    {
        foreach (var ngay in lich.GroupBy(x => x.NgayHoc.Date))
        {
            var items = ngay.OrderBy(x => x.GioBatDau).ToList();
            for (var i = 0; i < items.Count; i++)
            for (var j = i + 1; j < items.Count && items[j].GioBatDau < items[i].GioKetThuc; j++)
            {
                if (items[i].GioBatDau < items[j].GioKetThuc)
                    items[i].TrungLich = items[j].TrungLich = true;
            }
        }
    }

    private static TrangThaiKetQua GetTrangThaiKetQua(TrangThaiLopHoc trangThaiLop, Models.BangDiem? diem)
    {
        if (diem?.DiemTongKet is decimal tongKet) return tongKet >= 4 ? TrangThaiKetQua.Dat : TrangThaiKetQua.Rot;
        if (diem?.DiemCuoiKy == null && (trangThaiLop == TrangThaiLopHoc.DaKetThuc
            || diem != null && (diem.DiemChuyenCan.HasValue || diem.DiemBaiTap.HasValue || diem.DiemGiuaKy.HasValue)))
            return TrangThaiKetQua.ThieuDiemCuoiKy;
        return TrangThaiKetQua.DangHoc;
    }

    private IActionResult Placeholder(string title, string description, string icon) =>
        View("Placeholder", new SinhVienPlaceholderViewModel { TieuDe = title, MoTa = description, Icon = icon });

    private static HoSoSinhVienViewModel ToHoSoViewModel(Models.SinhVien sinhVien) => new()
    {
        MaSinhVien = sinhVien.MaSinhVien,
        HoTen = sinhVien.HoTen,
        Email = sinhVien.Email,
        SoDienThoai = sinhVien.SoDienThoai,
        NgaySinh = sinhVien.NgaySinh,
        GioiTinh = sinhVien.GioiTinh,
        DiaChi = sinhVien.DiaChi,
        TenKhoa = sinhVien.Khoa?.TenKhoa ?? "Chưa cập nhật",
        TrangThai = sinhVien.TrangThai
    };

    private static void CopyReadOnlyProfile(HoSoSinhVienViewModel model, Models.SinhVien sinhVien)
    {
        model.MaSinhVien = sinhVien.MaSinhVien;
        model.HoTen = sinhVien.HoTen;
        model.Email = sinhVien.Email;
        model.NgaySinh = sinhVien.NgaySinh;
        model.GioiTinh = sinhVien.GioiTinh;
        model.TenKhoa = sinhVien.Khoa?.TenKhoa ?? "Chưa cập nhật";
        model.TrangThai = sinhVien.TrangThai;
    }
}
