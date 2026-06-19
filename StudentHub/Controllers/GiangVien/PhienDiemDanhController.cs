using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers.GiangVienPortal;

[Route("GiangVien/PhienDiemDanh")]
public class PhienDiemDanhController(SimsDbContext db) : GiangVienPortalController(db)
{
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [HttpGet("")]
    public async Task<IActionResult> Index(int? lopHocId)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null) return View("~/Views/GiangVien/Shared/ChuaLienKet.cshtml");
        var query = Db.PhienDiemDanh.Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc).Where(x => x.GiangVienId == giangVienId);
        if (lopHocId.HasValue) query = query.Where(x => x.LopHocId == lopHocId);
        ViewBag.LopHocId = lopHocId;
        ViewBag.LopHoc = await Db.LopHoc.Where(x => x.GiangVienId == giangVienId).OrderBy(x => x.MaLop).ToListAsync();
        var danhSach = await query.OrderByDescending(x => x.NgayTao).ToListAsync();
        var now = DateTime.Now;
        var hetHan = danhSach.Where(x => x.DangMo && x.NgayDiemDanh.Date + x.GioKetThuc <= now).ToList();
        if (hetHan.Count != 0)
        {
            foreach (var phien in hetHan) phien.DangMo = false;
            await Db.SaveChangesAsync();
        }
        return View("~/Views/GiangVien/PhienDiemDanh/Index.cshtml", danhSach);
    }

    [HttpGet("TaoMoi")]
    public async Task<IActionResult> TaoMoi(int? lopHocId)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null) return View("~/Views/GiangVien/Shared/ChuaLienKet.cshtml");
        await LoadLop(giangVienId.Value);
        return View("~/Views/GiangVien/PhienDiemDanh/TaoMoi.cshtml", new TaoPhienDiemDanhViewModel { LopHocId = lopHocId ?? 0 });
    }

    [HttpPost("TaoMoi"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoMoi(TaoPhienDiemDanhViewModel model)
    {
        var giangVienId = await GetGiangVienId();
        if (giangVienId == null || !await SoHuuLop(model.LopHocId, giangVienId.Value)) return Forbid();
        if (model.GioKetThuc <= model.GioBatDau) ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
        if (model.NgayDiemDanh.Date + model.GioKetThuc <= DateTime.Now) ModelState.AddModelError(nameof(model.GioKetThuc), "Thời gian kết thúc phải ở tương lai.");
        if (!ModelState.IsValid) { await LoadLop(giangVienId.Value); return View("~/Views/GiangVien/PhienDiemDanh/TaoMoi.cshtml", model); }

        var maPhien = await TaoMaPhien();
        var phien = new PhienDiemDanh
        {
            LopHocId = model.LopHocId, GiangVienId = giangVienId.Value, NgayDiemDanh = model.NgayDiemDanh.Date,
            GioBatDau = model.GioBatDau, GioKetThuc = model.GioKetThuc, MaPhien = maPhien,
            QrToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(), DangMo = true, NgayTao = DateTime.Now
        };
        Db.PhienDiemDanh.Add(phien);
        var sinhVienIds = await Db.DangKyHoc.Where(x => x.LopHocId == model.LopHocId && x.TrangThai != TrangThaiDangKy.DaHuy).Select(x => x.SinhVienId).ToListAsync();
        foreach (var sinhVienId in sinhVienIds) Db.DiemDanh.Add(new DiemDanh { PhienDiemDanh = phien, SinhVienId = sinhVienId, TrangThai = TrangThaiDiemDanh.Vang });
        await Db.SaveChangesAsync();
        TempData["Success"] = $"Đã tạo phiên điểm danh {maPhien}.";
        return RedirectToAction(nameof(ChiTiet), new { id = phien.Id });
    }

    [HttpGet("ChiTiet/{id:int}")]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var giangVienId = await GetGiangVienId();
        var phien = await Db.PhienDiemDanh.Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc).SingleOrDefaultAsync(x => x.Id == id);
        if (giangVienId == null || phien == null || phien.GiangVienId != giangVienId) return Forbid();
        var now = DateTime.Now;
        var batDau = phien.NgayDiemDanh.Date + phien.GioBatDau;
        var ketThuc = phien.NgayDiemDanh.Date + phien.GioKetThuc;
        if (phien.DangMo && ketThuc <= now)
        {
            phien.DangMo = false;
            await Db.SaveChangesAsync();
        }
        var url = Url.Action("Index", "CheckIn", new { token = phien.QrToken }, Request.Scheme) ?? "";
        return View("~/Views/GiangVien/PhienDiemDanh/ChiTiet.cshtml", new PhienDiemDanhChiTietViewModel
        {
            Phien = phien, CheckInUrl = url,
            BatDauUnixMs = new DateTimeOffset(batDau).ToUnixTimeMilliseconds(),
            KetThucUnixMs = new DateTimeOffset(ketThuc).ToUnixTimeMilliseconds(),
            ServerNowUnixMs = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            DanhSach = await Db.DiemDanh.Include(x => x.SinhVien).Where(x => x.PhienDiemDanhId == id).OrderBy(x => x.SinhVien!.MaSinhVien).ToListAsync()
        });
    }

    [HttpPost("DoiTrangThai/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var giangVienId = await GetGiangVienId();
        var phien = await Db.PhienDiemDanh.SingleOrDefaultAsync(x => x.Id == id);
        if (giangVienId == null || phien == null || phien.GiangVienId != giangVienId) return Forbid();
        if (!phien.DangMo && phien.NgayDiemDanh.Date + phien.GioKetThuc <= DateTime.Now)
        {
            TempData["Error"] = "Phiên đã hết thời gian và không thể mở lại.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }
        phien.DangMo = !phien.DangMo;
        await Db.SaveChangesAsync();
        TempData["Success"] = phien.DangMo ? "Đã mở lại phiên điểm danh." : "Đã đóng phiên điểm danh.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost("CapNhatDiemDanh"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatDiemDanh(int id, TrangThaiDiemDanh trangThai, string? ghiChu)
    {
        var giangVienId = await GetGiangVienId();
        var diemDanh = await Db.DiemDanh.Include(x => x.PhienDiemDanh).SingleOrDefaultAsync(x => x.Id == id);
        if (giangVienId == null || diemDanh?.PhienDiemDanh?.GiangVienId != giangVienId) return Forbid();
        diemDanh.TrangThai = trangThai;
        diemDanh.GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();
        if (trangThai is TrangThaiDiemDanh.CoMat or TrangThaiDiemDanh.DiMuon) diemDanh.ThoiGianCheckIn ??= DateTime.Now;
        else diemDanh.ThoiGianCheckIn = null;
        await Db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái điểm danh.";
        return RedirectToAction(nameof(ChiTiet), new { id = diemDanh.PhienDiemDanhId });
    }

    private async Task LoadLop(int giangVienId) => ViewBag.LopHoc = await Db.LopHoc.Include(x => x.MonHoc).Where(x => x.GiangVienId == giangVienId).OrderBy(x => x.MaLop).ToListAsync();

    private async Task<string> TaoMaPhien()
    {
        string code;
        do { code = new string(RandomNumberGenerator.GetItems<char>(CodeChars.AsSpan(), 6)); }
        while (await Db.PhienDiemDanh.AnyAsync(x => x.MaPhien == code));
        return code;
    }
}
