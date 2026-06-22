using System.Security.Claims;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers;

[Authorize(Roles = "SinhVien")]
[Route("SinhVien/CheckIn")]
[Route("CheckIn")]
public class CheckInController(SimsDbContext db) : Controller
{
    private const int SoPhutDuocPhepTre = 15;

    [HttpGet("")]
    public IActionResult Index(string? token = null) => View(new CheckInViewModel { Token = token ?? "" });

    [HttpPost(""), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckInViewModel model)
    {
        try
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId)) return Forbid();
            var sinhVienId = await db.SinhVien.Where(x => x.TaiKhoanId == taiKhoanId).Select(x => (int?)x.Id).SingleOrDefaultAsync();
            if (sinhVienId == null) return KetQua(model, "Tài khoản chưa được liên kết với hồ sơ sinh viên.");

            var maPhien = model.MaPhien?.Trim().ToUpperInvariant();
            var token = model.Token?.Trim();
            var phien = await db.PhienDiemDanh.SingleOrDefaultAsync(x =>
                (!string.IsNullOrEmpty(token) && x.QrToken == token) ||
                (!string.IsNullOrEmpty(maPhien) && x.MaPhien == maPhien));
            if (phien == null) return KetQua(model, "Mã phiên không tồn tại.");

            var now = DateTime.Now;
            var batDau = phien.NgayDiemDanh.Date + phien.GioBatDau;
            var ketThuc = phien.NgayDiemDanh.Date + phien.GioKetThuc;
            if (!phien.DangMo || now > ketThuc) return KetQua(model, "Phiên điểm danh đã đóng.");
            if (now < batDau) return KetQua(model, "Phiên điểm danh chưa bắt đầu.");

            var thuocLop = await db.DangKyHoc.AnyAsync(x => x.SinhVienId == sinhVienId && x.LopHocId == phien.LopHocId
                && x.TrangThai != TrangThaiDangKy.DaHuy);
            if (!thuocLop) return KetQua(model, "Bạn không thuộc lớp học này.");

            var diemDanh = await db.DiemDanh.SingleOrDefaultAsync(x => x.PhienDiemDanhId == phien.Id && x.SinhVienId == sinhVienId);
            if (diemDanh?.ThoiGianCheckIn.HasValue == true) return KetQua(model, "Bạn đã check-in phiên này rồi.");

            var trangThai = now > batDau.AddMinutes(SoPhutDuocPhepTre) ? TrangThaiDiemDanh.DiMuon : TrangThaiDiemDanh.CoMat;
            if (diemDanh == null)
            {
                diemDanh = new DiemDanh { PhienDiemDanhId = phien.Id, SinhVienId = sinhVienId.Value };
                db.DiemDanh.Add(diemDanh);
            }
            diemDanh.TrangThai = trangThai;
            diemDanh.ThoiGianCheckIn = now;
            await db.SaveChangesAsync();

            model.ThanhCong = true;
            model.ThongBao = trangThai == TrangThaiDiemDanh.DiMuon
                ? "Check-in thành công. Bạn được ghi nhận đi muộn."
                : "Check-in thành công. Bạn được ghi nhận có mặt.";
            return View(model);
        }
        catch (DbUpdateException)
        {
            return KetQua(model, "Bạn đã check-in phiên này rồi.");
        }
        catch (DbException)
        {
            return KetQua(model, "Module điểm danh chưa sẵn sàng. Vui lòng liên hệ giảng viên hoặc quản trị viên.");
        }
    }

    private IActionResult KetQua(CheckInViewModel model, string thongBao)
    {
        model.ThanhCong = false;
        model.ThongBao = thongBao;
        return View(model);
    }
}
