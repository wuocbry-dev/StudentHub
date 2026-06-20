using System.Security.Claims;
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
        var diem = await db.BangDiem
            .AsNoTracking()
            .Where(x => x.SinhVienId == sinhVien.Id && x.DiemTongKet.HasValue)
            .Select(x => new { x.DiemTongKet, SoTinChi = x.LopHoc!.MonHoc!.SoTinChi })
            .ToListAsync();
        var tongTinChi = diem.Sum(x => x.SoTinChi);
        if (tongTinChi > 0)
            gpa = Math.Round(diem.Sum(x => x.DiemTongKet!.Value * x.SoTinChi) / tongTinChi, 2);

        double? tyLeChuyenCan = null;
        var diemDanh = await db.DiemDanh.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVien.Id)
            .Select(x => x.TrangThai)
            .ToListAsync();
        var soBuoiTinhChuyenCan = diemDanh.Count(x => x != TrangThaiDiemDanh.CoPhep);
        if (soBuoiTinhChuyenCan > 0)
        {
            var soBuoiDat = diemDanh.Sum(x => x switch
            {
                TrangThaiDiemDanh.CoMat => 1d,
                TrangThaiDiemDanh.DiMuon => 0.5d,
                _ => 0d
            });
            tyLeChuyenCan = soBuoiDat * 100 / soBuoiTinhChuyenCan;
        }

        return View(new SinhVienDashboardViewModel
        {
            HoTen = sinhVien.HoTen,
            MaSinhVien = sinhVien.MaSinhVien,
            TongMonDangHoc = tongMonDangHoc,
            Gpa = gpa,
            TyLeChuyenCan = tyLeChuyenCan,
            SoLopHomNay = lichHomNay.Count,
            SoMonNguyCoRot = diem.Count(x => x.DiemTongKet < 4),
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
    public IActionResult LichHoc() => Placeholder("Lịch học", "Lịch học chi tiết sẽ được bổ sung trong giai đoạn tiếp theo.", "bi-calendar-week");

    [HttpGet("BangDiem")]
    public IActionResult BangDiem() => Placeholder("Bảng điểm", "Bảng điểm cá nhân sẽ được bổ sung trong giai đoạn tiếp theo.", "bi-clipboard-data");

    [HttpGet("ChuyenCan")]
    public IActionResult ChuyenCan() => Placeholder("Chuyên cần", "Lịch sử chuyên cần sẽ được bổ sung trong giai đoạn tiếp theo.", "bi-person-check");

    [HttpGet("CanhBao")]
    public IActionResult CanhBao() => Placeholder("Cảnh báo học tập", "Hệ thống chưa có dữ liệu cảnh báo sinh viên.", "bi-exclamation-triangle");

    private IQueryable<Models.SinhVien> GetSinhVien()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
            return db.SinhVien.Where(x => false);
        return db.SinhVien.Where(x => x.TaiKhoanId == taiKhoanId);
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
