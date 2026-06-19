using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers;

[Authorize(Roles = "SinhVien")]
public class CheckInController(SimsDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Index(string? token = null) => View(new CheckInViewModel { Token = token ?? "" });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckInViewModel model)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId)) return Forbid();
        var sinhVienId = await db.SinhVien.Where(x => x.TaiKhoanId == taiKhoanId).Select(x => (int?)x.Id).SingleOrDefaultAsync();
        if (sinhVienId == null)
        {
            model.ThongBao = "Tài khoản chưa được liên kết với hồ sơ sinh viên.";
            return View(model);
        }
        var maPhien = model.MaPhien?.Trim().ToUpperInvariant();
        var phien = await db.PhienDiemDanh.SingleOrDefaultAsync(x =>
            (!string.IsNullOrEmpty(model.Token) && x.QrToken == model.Token) || (!string.IsNullOrEmpty(maPhien) && x.MaPhien == maPhien));
        if (phien == null) { model.ThongBao = "Mã hoặc QR phiên điểm danh không hợp lệ."; return View(model); }
        var now = DateTime.Now;
        var batDau = phien.NgayDiemDanh.Date + phien.GioBatDau;
        var ketThuc = phien.NgayDiemDanh.Date + phien.GioKetThuc;
        if (now >= ketThuc && phien.DangMo)
        {
            phien.DangMo = false;
            await db.SaveChangesAsync();
        }
        if (!phien.DangMo) { model.ThongBao = "Phiên điểm danh đã đóng hoặc đã hết thời gian."; return View(model); }
        if (now < batDau || now > ketThuc) { model.ThongBao = "Hiện không nằm trong thời gian điểm danh."; return View(model); }
        var diemDanh = await db.DiemDanh.SingleOrDefaultAsync(x => x.PhienDiemDanhId == phien.Id && x.SinhVienId == sinhVienId);
        if (diemDanh == null) { model.ThongBao = "Bạn không có trong danh sách lớp này."; return View(model); }
        if (diemDanh.ThoiGianCheckIn.HasValue) { model.ThongBao = "Bạn đã điểm danh phiên này rồi."; return View(model); }
        diemDanh.TrangThai = now > batDau.AddMinutes(15) ? TrangThaiDiemDanh.DiMuon : TrangThaiDiemDanh.CoMat;
        diemDanh.ThoiGianCheckIn = now;
        await db.SaveChangesAsync();
        model.ThanhCong = true;
        model.ThongBao = diemDanh.TrangThai == TrangThaiDiemDanh.DiMuon ? "Điểm danh thành công, trạng thái đi muộn." : "Điểm danh thành công.";
        return View(model);
    }
}
