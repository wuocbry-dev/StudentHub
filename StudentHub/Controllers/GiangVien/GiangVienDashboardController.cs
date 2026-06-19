using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers.GiangVienPortal;

[Route("GiangVien/Dashboard")]
public class GiangVienDashboardController(SimsDbContext db) : GiangVienPortalController(db)
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null) return View("~/Views/GiangVien/Shared/ChuaLienKet.cshtml");

        var lopIds = Db.LopHoc.Where(x => x.GiangVienId == giangVienId).Select(x => x.Id);
        var diemDanh = Db.DiemDanh.Where(x => lopIds.Contains(x.PhienDiemDanh!.LopHocId));
        var tongDiemDanh = await diemDanh.CountAsync();
        var coMat = await diemDanh.CountAsync(x => x.TrangThai == TrangThaiDiemDanh.CoMat || x.TrangThai == TrangThaiDiemDanh.DiMuon);
        var homNay = DateTime.Today;
        var thu = homNay.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)homNay.DayOfWeek + 1;

        var lopVang = await Db.LopHoc.Where(x => x.GiangVienId == giangVienId)
            .Select(x => new LopVangNhieuViewModel
            {
                LopHoc = x,
                LuotVang = Db.DiemDanh.Count(d => d.PhienDiemDanh!.LopHocId == x.Id && d.TrangThai == TrangThaiDiemDanh.Vang),
                TyLeVang = Db.DiemDanh.Count(d => d.PhienDiemDanh!.LopHocId == x.Id) == 0 ? 0 :
                    Db.DiemDanh.Count(d => d.PhienDiemDanh!.LopHocId == x.Id && d.TrangThai == TrangThaiDiemDanh.Vang) * 100.0 /
                    Db.DiemDanh.Count(d => d.PhienDiemDanh!.LopHocId == x.Id)
            }).OrderByDescending(x => x.TyLeVang).Take(5).ToListAsync();

        return View("~/Views/GiangVien/Dashboard/Index.cshtml", new GiangVienDashboardViewModel
        {
            TongLop = await lopIds.CountAsync(),
            TongSinhVien = await Db.DangKyHoc.Where(x => lopIds.Contains(x.LopHocId) && x.TrangThai != TrangThaiDangKy.DaHuy).Select(x => x.SinhVienId).Distinct().CountAsync(),
            TongPhienDiemDanh = await Db.PhienDiemDanh.CountAsync(x => x.GiangVienId == giangVienId),
            TyLeChuyenCan = tongDiemDanh == 0 ? 0 : coMat * 100.0 / tongDiemDanh,
            LopHomNay = await Db.LichHoc.Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc).Include(x => x.PhongHoc)
                .Where(x => x.LopHoc!.GiangVienId == giangVienId && x.ThuTrongTuan == thu && x.NgayBatDau.Date <= homNay && x.NgayKetThuc.Date >= homNay)
                .OrderBy(x => x.GioBatDau).ToListAsync(),
            LopVangNhieu = lopVang
        });
    }
}
