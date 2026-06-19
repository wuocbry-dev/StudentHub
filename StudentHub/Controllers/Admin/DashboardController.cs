using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class DashboardController(SimsDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var byKhoa = await db.Khoa.Select(k => new { k.TenKhoa, Count = k.SinhViens.Count }).ToListAsync();
        var byHocKy = await db.LopHoc.GroupBy(x => new { x.HocKy, x.NamHoc }).Select(g => new { Label = g.Key.HocKy + " - " + g.Key.NamHoc, Count = g.Count() }).ToListAsync();
        return View("~/Views/Admin/Dashboard/Index.cshtml", new DashboardViewModel
        {
            TongSinhVien = await db.SinhVien.CountAsync(), TongGiangVien = await db.GiangVien.CountAsync(),
            TongMonHoc = await db.MonHoc.CountAsync(), TongLopHoc = await db.LopHoc.CountAsync(),
            SinhVienDangHoc = await db.SinhVien.CountAsync(x => x.TrangThai == TrangThaiSinhVien.DangHoc),
            KhoaLabels = byKhoa.Select(x => x.TenKhoa).ToList(), KhoaValues = byKhoa.Select(x => x.Count).ToList(),
            HocKyLabels = byHocKy.Select(x => x.Label).ToList(), HocKyValues = byHocKy.Select(x => x.Count).ToList(),
            SinhVienMoi = await db.SinhVien.Include(x => x.Khoa).OrderByDescending(x => x.NgayTao).Take(5).ToListAsync(),
            LopHocMoi = await db.LopHoc.Include(x => x.MonHoc).OrderByDescending(x => x.Id).Take(5).ToListAsync()
        });
    }
}
