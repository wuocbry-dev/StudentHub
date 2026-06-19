using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers.GiangVienPortal;

[Route("GiangVien/QuanLyDiem")]
public class QuanLyDiemController(SimsDbContext db) : GiangVienPortalController(db)
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null) return View("~/Views/GiangVien/Shared/ChuaLienKet.cshtml");
        var lop = await Db.LopHoc.Include(x => x.MonHoc).Where(x => x.GiangVienId == giangVienId)
            .OrderByDescending(x => x.Id).ToListAsync();
        return View("~/Views/GiangVien/QuanLyDiem/Index.cshtml", lop);
    }

    [HttpGet("Lop/{lopHocId:int}")]
    public async Task<IActionResult> Lop(int lopHocId)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(lopHocId, giangVienId.Value)) return Forbid();

        var lop = await Db.LopHoc.Include(x => x.MonHoc).SingleAsync(x => x.Id == lopHocId);
        var sinhVien = await Db.DangKyHoc.Include(x => x.SinhVien)
            .Where(x => x.LopHocId == lopHocId && x.TrangThai != TrangThaiDangKy.DaHuy)
            .OrderBy(x => x.SinhVien!.MaSinhVien).ToListAsync();
        var sinhVienIds = sinhVien.Select(x => x.SinhVienId).ToList();
        var bangDiem = await Db.BangDiem.Where(x => x.LopHocId == lopHocId && sinhVienIds.Contains(x.SinhVienId))
            .ToDictionaryAsync(x => x.SinhVienId);
        var rows = sinhVien.Select(x =>
        {
            bangDiem.TryGetValue(x.SinhVienId, out var diem);
            return new DiemSinhVienViewModel
            {
                SinhVienId = x.SinhVienId,
                MaSinhVien = x.SinhVien!.MaSinhVien,
                HoTen = x.SinhVien.HoTen,
                DiemChuyenCan = diem?.DiemChuyenCan,
                DiemBaiTap = diem?.DiemBaiTap,
                DiemGiuaKy = diem?.DiemGiuaKy,
                DiemCuoiKy = diem?.DiemCuoiKy,
                DiemTongKet = diem?.DiemTongKet,
                DiemChu = diem?.DiemChu
            };
        }).ToList();
        return View("~/Views/GiangVien/QuanLyDiem/Lop.cshtml", new QuanLyDiemViewModel { LopHoc = lop, DanhSach = rows });
    }

    [HttpPost("Luu"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Luu(int lopHocId, int sinhVienId, decimal? diemChuyenCan,
        decimal? diemBaiTap, decimal? diemGiuaKy, decimal? diemCuoiKy)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(lopHocId, giangVienId.Value)) return Forbid();
        var thuocLop = await Db.DangKyHoc.AnyAsync(x => x.LopHocId == lopHocId && x.SinhVienId == sinhVienId && x.TrangThai != TrangThaiDangKy.DaHuy);
        if (!thuocLop) return NotFound();
        if (new[] { diemChuyenCan, diemBaiTap, diemGiuaKy, diemCuoiKy }.Any(x => !HopLe(x)))
        {
            TempData["Error"] = "Điểm phải nằm trong khoảng từ 0 đến 10.";
            return RedirectToAction(nameof(Lop), new { lopHocId });
        }

        var diem = await Db.BangDiem.SingleOrDefaultAsync(x => x.SinhVienId == sinhVienId && x.LopHocId == lopHocId)
            ?? new BangDiem { SinhVienId = sinhVienId, LopHocId = lopHocId };
        diem.DiemChuyenCan = diemChuyenCan;
        diem.DiemBaiTap = diemBaiTap;
        diem.DiemGiuaKy = diemGiuaKy;
        diem.DiemCuoiKy = diemCuoiKy;
        TinhTongKet(diem);
        diem.NgayCapNhat = DateTime.Now;
        if (diem.Id == 0) Db.BangDiem.Add(diem);
        await Db.SaveChangesAsync();
        TempData["Success"] = "Đã lưu và tính lại điểm sinh viên.";
        return RedirectToAction(nameof(Lop), new { lopHocId });
    }

    [HttpPost("TinhLaiDiemChuyenCan"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TinhLaiDiemChuyenCan(int lopHocId)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(lopHocId, giangVienId.Value)) return Forbid();
        var sinhVienIds = await Db.DangKyHoc
            .Where(x => x.LopHocId == lopHocId && x.TrangThai != TrangThaiDangKy.DaHuy)
            .Select(x => x.SinhVienId).ToListAsync();
        var diemDanh = await Db.DiemDanh
            .Where(x => x.PhienDiemDanh!.LopHocId == lopHocId && sinhVienIds.Contains(x.SinhVienId))
            .Select(x => new { x.SinhVienId, x.TrangThai }).ToListAsync();
        var bangDiem = await Db.BangDiem.Where(x => x.LopHocId == lopHocId && sinhVienIds.Contains(x.SinhVienId))
            .ToDictionaryAsync(x => x.SinhVienId);

        foreach (var sinhVienId in sinhVienIds)
        {
            var lichSu = diemDanh.Where(x => x.SinhVienId == sinhVienId).ToList();
            var tongBatBuoc = lichSu.Count(x => x.TrangThai != TrangThaiDiemDanh.CoPhep);
            var buoiDat = lichSu.Sum(x => x.TrangThai switch
            {
                TrangThaiDiemDanh.CoMat => 1m,
                TrangThaiDiemDanh.DiMuon => 0.5m,
                _ => 0m
            });
            if (!bangDiem.TryGetValue(sinhVienId, out var diem))
            {
                diem = new BangDiem { SinhVienId = sinhVienId, LopHocId = lopHocId };
                Db.BangDiem.Add(diem);
            }
            diem.DiemChuyenCan = tongBatBuoc == 0 ? null : Math.Round(buoiDat * 10m / tongBatBuoc, 2);
            TinhTongKet(diem);
            diem.NgayCapNhat = DateTime.Now;
        }
        await Db.SaveChangesAsync();
        TempData["Success"] = "Đã tính lại điểm chuyên cần cho toàn bộ sinh viên trong lớp.";
        return RedirectToAction(nameof(Lop), new { lopHocId });
    }

    private static bool HopLe(decimal? value) => value is null or >= 0 and <= 10;

    private static void TinhTongKet(BangDiem diem)
    {
        if (diem.DiemChuyenCan == null || diem.DiemBaiTap == null || diem.DiemGiuaKy == null || diem.DiemCuoiKy == null)
        {
            diem.DiemTongKet = null;
            diem.DiemChu = null;
            return;
        }
        diem.DiemTongKet = Math.Round(diem.DiemChuyenCan.Value * 0.1m + diem.DiemBaiTap.Value * 0.2m
            + diem.DiemGiuaKy.Value * 0.3m + diem.DiemCuoiKy.Value * 0.4m, 2);
        diem.DiemChu = diem.DiemTongKet.Value switch
        {
            >= 8.5m => "A",
            >= 7m => "B",
            >= 5.5m => "C",
            >= 4m => "D",
            _ => "F"
        };
    }
}
