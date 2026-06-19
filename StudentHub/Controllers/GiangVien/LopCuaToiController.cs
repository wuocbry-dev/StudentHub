using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers.GiangVienPortal;

[Route("GiangVien/LopCuaToi")]
public class LopCuaToiController(SimsDbContext db) : GiangVienPortalController(db)
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string search = "")
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null) return View("~/Views/GiangVien/Shared/ChuaLienKet.cshtml");
        var query = Db.LopHoc.Include(x => x.MonHoc).Where(x => x.GiangVienId == giangVienId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.MaLop.Contains(search) || x.TenLop.Contains(search) || x.MonHoc!.TenMonHoc.Contains(search));
        ViewBag.Search = search;
        var items = await query.OrderByDescending(x => x.Id).Select(x => new LopCuaToiItemViewModel
        {
            LopHoc = x,
            SoSinhVien = Db.DangKyHoc.Count(d => d.LopHocId == x.Id && d.TrangThai != TrangThaiDangKy.DaHuy),
            SoBuoiDiemDanh = Db.PhienDiemDanh.Count(p => p.LopHocId == x.Id)
        }).ToListAsync();
        return View("~/Views/GiangVien/LopCuaToi/Index.cshtml", items);
    }

    [HttpGet("ChiTiet/{id:int}")]
    public async Task<IActionResult> ChiTiet(int id, string search = "")
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(id, giangVienId.Value)) return Forbid();
        var lop = await Db.LopHoc.Include(x => x.MonHoc).SingleAsync(x => x.Id == id);
        var sinhVienQuery = Db.DangKyHoc.Include(x => x.SinhVien).Where(x => x.LopHocId == id && x.TrangThai != TrangThaiDangKy.DaHuy);
        if (!string.IsNullOrWhiteSpace(search)) sinhVienQuery = sinhVienQuery.Where(x => x.SinhVien!.MaSinhVien.Contains(search) || x.SinhVien.HoTen.Contains(search));
        var tongBuoi = await Db.PhienDiemDanh.CountAsync(x => x.LopHocId == id);
        var sinhVien = await sinhVienQuery.Select(x => new SinhVienChuyenCanViewModel
        {
            SinhVienId = x.SinhVienId, MaSinhVien = x.SinhVien!.MaSinhVien, HoTen = x.SinhVien.HoTen, Email = x.SinhVien.Email,
            TongBuoi = tongBuoi,
            CoMat = Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai == TrangThaiDiemDanh.CoMat),
            DiMuon = Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai == TrangThaiDiemDanh.DiMuon),
            Vang = Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai == TrangThaiDiemDanh.Vang),
            CoPhep = Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai == TrangThaiDiemDanh.CoPhep),
            TyLeChuyenCan = Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai != TrangThaiDiemDanh.CoPhep) == 0 ? 0 :
                (Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai == TrangThaiDiemDanh.CoMat) +
                 Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai == TrangThaiDiemDanh.DiMuon) * 0.5) * 100.0 /
                Db.DiemDanh.Count(d => d.SinhVienId == x.SinhVienId && d.PhienDiemDanh!.LopHocId == id && d.TrangThai != TrangThaiDiemDanh.CoPhep)
        }).OrderBy(x => x.MaSinhVien).ToListAsync();
        return View("~/Views/GiangVien/LopCuaToi/ChiTiet.cshtml", new LopChiTietViewModel
        {
            LopHoc = lop, Search = search, SinhVien = sinhVien,
            LichHoc = await Db.LichHoc.Include(x => x.PhongHoc).Where(x => x.LopHocId == id).OrderBy(x => x.ThuTrongTuan).ThenBy(x => x.GioBatDau).ToListAsync()
        });
    }
}
