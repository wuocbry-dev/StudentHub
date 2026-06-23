using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.Services.HocVuot;

namespace StudentHub.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/GoiYHocVuot")]
public class GoiYHocVuotController(SimsDbContext db, IGoiYHocVuotService goiYHocVuotService) : Controller
{
    private const string IndexView = "~/Views/Admin/GoiYHocVuot/Index.cshtml";

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int? khoaId = null, int? sinhVienId = null, string? hocKy = null, string? namHoc = null)
    {
        var model = await BuildPageAsync(khoaId, sinhVienId, hocKy, namHoc);
        return View(IndexView, model);
    }

    [HttpPost("TaoLai"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoLai(int? khoaId, int? sinhVienId, string? hocKy, string? namHoc)
    {
        var hocKyNamHoc = await ResolveHocKyNamHocAsync(hocKy, namHoc);
        if (hocKyNamHoc == null)
        {
            TempData["Error"] = "Chua co lop hoc de xac dinh hoc ky va nam hoc.";
            return RedirectToAction(nameof(Index));
        }

        var query = db.SinhVien.AsNoTracking().Where(x => x.TrangThai == TrangThaiSinhVien.DangHoc);
        if (khoaId.HasValue) query = query.Where(x => x.KhoaId == khoaId);
        if (sinhVienId.HasValue) query = query.Where(x => x.Id == sinhVienId);

        var sinhVienIds = await query.Select(x => x.Id).ToListAsync();
        var tongGoiY = 0;
        foreach (var id in sinhVienIds)
        {
            var goiY = await goiYHocVuotService.TaoGoiYChoSinhVienAsync(id, hocKyNamHoc.Value.HocKy, hocKyNamHoc.Value.NamHoc);
            tongGoiY += goiY.Count(x => x.HocKyGoiY == hocKyNamHoc.Value.HocKy && x.NamHocGoiY == hocKyNamHoc.Value.NamHoc);
        }

        TempData["Success"] = $"Da tao lai goi y cho {sinhVienIds.Count} sinh vien, tong {tongGoiY} goi y dang hien thi.";
        return RedirectToAction(nameof(Index), new { khoaId, sinhVienId, hocKy = hocKyNamHoc.Value.HocKy, namHoc = hocKyNamHoc.Value.NamHoc });
    }

    private async Task<AdminGoiYHocVuotPageViewModel> BuildPageAsync(int? khoaId, int? sinhVienId, string? hocKy, string? namHoc)
    {
        var hocKyOptions = await db.LopHoc.AsNoTracking()
            .Select(x => new SinhVienHocVuotHocKyOptionViewModel { HocKy = x.HocKy, NamHoc = x.NamHoc })
            .Distinct()
            .OrderByDescending(x => x.NamHoc)
            .ThenByDescending(x => x.HocKy)
            .ToListAsync();

        var query = db.GoiYHocVuot.AsNoTracking()
            .Include(x => x.SinhVien).ThenInclude(x => x!.Khoa)
            .Include(x => x.MonHoc)
            .Include(x => x.LopHoc).ThenInclude(x => x!.GiangVien)
            .AsQueryable();

        if (khoaId.HasValue) query = query.Where(x => x.SinhVien!.KhoaId == khoaId);
        if (sinhVienId.HasValue) query = query.Where(x => x.SinhVienId == sinhVienId);
        if (!string.IsNullOrWhiteSpace(hocKy)) query = query.Where(x => x.HocKyGoiY == hocKy);
        if (!string.IsNullOrWhiteSpace(namHoc)) query = query.Where(x => x.NamHocGoiY == namHoc);

        var items = await query.OrderByDescending(x => x.NgayTao).Take(300).ToListAsync();

        return new AdminGoiYHocVuotPageViewModel
        {
            KhoaId = khoaId,
            SinhVienId = sinhVienId,
            HocKy = hocKy,
            NamHoc = namHoc,
            KhoaOptions = await db.Khoa.AsNoTracking().OrderBy(x => x.MaKhoa).ToListAsync(),
            SinhVienOptions = await db.SinhVien.AsNoTracking().OrderBy(x => x.MaSinhVien).ToListAsync(),
            HocKyOptions = hocKyOptions,
            GoiY = items.Select(ToViewModel).ToList()
        };
    }

    private async Task<(string HocKy, string NamHoc)?> ResolveHocKyNamHocAsync(string? hocKy, string? namHoc)
    {
        if (!string.IsNullOrWhiteSpace(hocKy) && !string.IsNullOrWhiteSpace(namHoc))
            return (hocKy.Trim(), namHoc.Trim());

        var value = await db.LopHoc.AsNoTracking()
            .OrderByDescending(x => x.NamHoc)
            .ThenByDescending(x => x.HocKy)
            .Select(x => new { x.HocKy, x.NamHoc })
            .FirstOrDefaultAsync();

        return value == null ? null : (value.HocKy, value.NamHoc);
    }

    private static GoiYHocVuotViewModel ToViewModel(GoiYHocVuot item) => new()
    {
        Id = item.Id,
        SinhVienId = item.SinhVienId,
        MaSinhVien = item.SinhVien?.MaSinhVien ?? "",
        HoTenSinhVien = item.SinhVien?.HoTen ?? "",
        TenKhoa = item.SinhVien?.Khoa?.TenKhoa ?? "",
        MonHocId = item.MonHocId,
        MaMonHoc = item.MonHoc?.MaMonHoc ?? "",
        TenMonHoc = item.MonHoc?.TenMonHoc ?? "",
        SoTinChi = item.MonHoc?.SoTinChi ?? 0,
        LopHocId = item.LopHocId,
        MaLop = item.LopHoc?.MaLop ?? "",
        TenLop = item.LopHoc?.TenLop ?? "",
        GiangVien = item.LopHoc?.GiangVien?.HoTen ?? "Chua phan cong",
        HocKyGoiY = item.HocKyGoiY,
        NamHocGoiY = item.NamHocGoiY,
        DiemPhuHop = item.DiemPhuHop,
        LyDoGoiY = item.LyDoGoiY,
        MucDoGoiY = item.MucDoGoiY,
        TrangThai = item.TrangThai,
        NgayTao = item.NgayTao
    };
}
