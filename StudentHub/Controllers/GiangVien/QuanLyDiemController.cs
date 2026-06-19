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
        var lop = await Db.LopHoc.Include(x => x.MonHoc).Where(x => x.GiangVienId == giangVienId).OrderByDescending(x => x.Id).ToListAsync();
        return View("~/Views/GiangVien/QuanLyDiem/Index.cshtml", lop);
    }

    [HttpGet("Lop/{lopHocId:int}")]
    public async Task<IActionResult> Lop(int lopHocId, string search = "")
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(lopHocId, giangVienId.Value)) return Forbid();
        var lop = await Db.LopHoc.Include(x => x.MonHoc).SingleAsync(x => x.Id == lopHocId);
        var query = Db.DangKyHoc.Include(x => x.SinhVien).Include(x => x.LopHoc)
            .Where(x => x.LopHocId == lopHocId && x.TrangThai != TrangThaiDangKy.DaHuy);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.SinhVien!.MaSinhVien.Contains(search) || x.SinhVien.HoTen.Contains(search));
        var dangKy = await query.OrderBy(x => x.SinhVien!.MaSinhVien).ToListAsync();
        var dangKyIds = dangKy.Select(x => x.Id).ToList();
        var bangDiem = await Db.Diem.Where(x => dangKyIds.Contains(x.DangKyHocId)).ToDictionaryAsync(x => x.DangKyHocId);
        var rows = dangKy.Select(x =>
        {
            bangDiem.TryGetValue(x.Id, out var diem);
            return new DiemSinhVienViewModel
            {
                DangKyHocId = x.Id, MaSinhVien = x.SinhVien!.MaSinhVien, HoTen = x.SinhVien.HoTen,
                DiemChuyenCan = diem?.DiemChuyenCan, DiemGiuaKy = diem?.DiemGiuaKy, DiemCuoiKy = diem?.DiemCuoiKy,
                DiemTongKet = diem?.DiemTongKet, XepLoai = diem?.XepLoai ?? XepLoaiDiem.ChuaXepLoai
            };
        }).ToList();
        return View("~/Views/GiangVien/QuanLyDiem/Lop.cshtml", new QuanLyDiemViewModel { LopHoc = lop, DanhSach = rows, Search = search });
    }

    [HttpPost("Luu"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Luu(int lopHocId, int dangKyHocId, decimal? diemChuyenCan, decimal? diemGiuaKy, decimal? diemCuoiKy)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(lopHocId, giangVienId.Value)) return Forbid();
        var dangKy = await Db.DangKyHoc.SingleOrDefaultAsync(x => x.Id == dangKyHocId && x.LopHocId == lopHocId);
        if (dangKy == null) return NotFound();
        if (!HopLe(diemChuyenCan) || !HopLe(diemGiuaKy) || !HopLe(diemCuoiKy))
        {
            TempData["Error"] = "Điểm phải nằm trong khoảng từ 0 đến 10.";
            return RedirectToAction(nameof(Lop), new { lopHocId });
        }
        var diem = await Db.Diem.SingleOrDefaultAsync(x => x.DangKyHocId == dangKyHocId) ?? new Diem { DangKyHocId = dangKyHocId };
        diem.DiemChuyenCan = diemChuyenCan; diem.DiemGiuaKy = diemGiuaKy; diem.DiemCuoiKy = diemCuoiKy;
        TinhTongKet(diem);
        diem.NgayCapNhat = DateTime.Now;
        if (diem.Id == 0) Db.Diem.Add(diem);
        await Db.SaveChangesAsync();
        TempData["Success"] = $"Đã lưu điểm cho sinh viên.";
        return RedirectToAction(nameof(Lop), new { lopHocId });
    }

    [HttpPost("DongBoChuyenCan"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DongBoChuyenCan(int lopHocId)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(lopHocId, giangVienId.Value)) return Forbid();
        var dangKy = await Db.DangKyHoc.Where(x => x.LopHocId == lopHocId && x.TrangThai != TrangThaiDangKy.DaHuy).ToListAsync();
        var tongBuoi = await Db.PhienDiemDanh.CountAsync(x => x.LopHocId == lopHocId);
        foreach (var dk in dangKy)
        {
            var diem = await Db.Diem.SingleOrDefaultAsync(x => x.DangKyHocId == dk.Id) ?? new Diem { DangKyHocId = dk.Id };
            var dat = await Db.DiemDanh.CountAsync(x => x.SinhVienId == dk.SinhVienId && x.PhienDiemDanh!.LopHocId == lopHocId && (x.TrangThai == TrangThaiDiemDanh.CoMat || x.TrangThai == TrangThaiDiemDanh.DiMuon));
            diem.DiemChuyenCan = tongBuoi == 0 ? null : Math.Round(dat * 10m / tongBuoi, 2);
            TinhTongKet(diem); diem.NgayCapNhat = DateTime.Now;
            if (diem.Id == 0) Db.Diem.Add(diem);
        }
        await Db.SaveChangesAsync();
        TempData["Success"] = "Đã đồng bộ điểm chuyên cần từ dữ liệu điểm danh.";
        return RedirectToAction(nameof(Lop), new { lopHocId });
    }

    private static bool HopLe(decimal? value) => value is null or >= 0 and <= 10;

    private static void TinhTongKet(Diem diem)
    {
        if (diem.DiemChuyenCan == null || diem.DiemGiuaKy == null || diem.DiemCuoiKy == null)
        {
            diem.DiemTongKet = null; diem.XepLoai = XepLoaiDiem.ChuaXepLoai; return;
        }
        diem.DiemTongKet = Math.Round(diem.DiemChuyenCan.Value * 0.1m + diem.DiemGiuaKy.Value * 0.3m + diem.DiemCuoiKy.Value * 0.6m, 2);
        diem.XepLoai = diem.DiemTongKet.Value switch
        {
            >= 9 => XepLoaiDiem.XuatSac, >= 8 => XepLoaiDiem.Gioi, >= 7 => XepLoaiDiem.Kha,
            >= 5 => XepLoaiDiem.TrungBinh, >= 4 => XepLoaiDiem.Yeu, _ => XepLoaiDiem.Kem
        };
    }
}
