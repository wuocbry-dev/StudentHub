using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.Services.Import;

namespace StudentHub.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class NhapDuLieuController(
    SimsDbContext db,
    IFileImportService importService,
    FileMauService fileMauService,
    FileLoiService fileLoiService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new NhapDuLieuIndexViewModel
        {
            LoaiDuLieuOptions = LoaiDuLieuOptions(),
            LoaiFileOptions = LoaiFileOptions(),
            LichSuGanDay = await db.LichSuNhapDuLieu
                .AsNoTracking()
                .Include(x => x.TaiKhoan)
                .OrderByDescending(x => x.NgayNhap)
                .Take(5)
                .ToListAsync()
        };
        return View("~/Views/Admin/NhapDuLieu/Index.cshtml", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KiemTraDuLieu(IFormFile file, LoaiDuLieuNhap loaiDuLieu)
    {
        try
        {
            var result = await importService.PreviewAsync(file, loaiDuLieu, CurrentTaiKhoanId());
            return RedirectToAction(nameof(Preview), new { id = result.LichSuNhapDuLieuId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Preview(int id)
    {
        var model = await BuildPreviewModel(id);
        return View("~/Views/Admin/NhapDuLieu/Preview.cshtml", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanNhap(int id, bool chiNhapDongHopLe = false)
    {
        try
        {
            await importService.ConfirmAsync(id, CurrentTaiKhoanId(), chiNhapDongHopLe);
            TempData["Success"] = "Nhap du lieu thanh cong.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(KetQua), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyNhap(int id)
    {
        await importService.HuyAsync(id, CurrentTaiKhoanId());
        TempData["Success"] = "Da huy import.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    public async Task<IActionResult> KetQua(int id)
    {
        var lichSu = await LoadLichSu(id);
        return View("~/Views/Admin/NhapDuLieu/KetQua.cshtml", new ImportResultViewModel { LichSu = lichSu });
    }

    public async Task<IActionResult> LichSu()
    {
        var items = await db.LichSuNhapDuLieu.AsNoTracking()
            .Include(x => x.TaiKhoan)
            .OrderByDescending(x => x.NgayNhap)
            .ToListAsync();
        return View("~/Views/Admin/NhapDuLieu/LichSu.cshtml", new LichSuNhapDuLieuViewModel { Items = items });
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var model = await BuildPreviewModel(id);
        return View("~/Views/Admin/NhapDuLieu/ChiTiet.cshtml", model);
    }

    public IActionResult TaiFileMau(LoaiDuLieuNhap loaiDuLieu, LoaiFileNhap loaiFile)
    {
        var file = fileMauService.Create(loaiDuLieu, loaiFile);
        return File(file.Content, file.ContentType, file.FileName);
    }

    public async Task<IActionResult> TaiFileLoi(int id)
    {
        var file = await fileLoiService.CreateAsync(id);
        return File(file.Content, file.ContentType, file.FileName);
    }

    private async Task<ImportPreviewViewModel> BuildPreviewModel(int id)
    {
        var lichSu = await LoadLichSu(id);
        var rows = await db.DuLieuNhapTam.AsNoTracking()
            .Where(x => x.LichSuNhapDuLieuId == id)
            .OrderBy(x => x.SoDong)
            .Select(x => new ImportRowPreviewViewModel
            {
                SoDong = x.SoDong,
                NoiDungHienThi = x.NoiDungJson,
                HopLe = x.HopLe,
                NoiDungLoi = x.NoiDungLoi
            })
            .ToListAsync();
        return new ImportPreviewViewModel { LichSu = lichSu, Rows = rows };
    }

    private async Task<LichSuNhapDuLieu> LoadLichSu(int id)
    {
        var lichSu = await db.LichSuNhapDuLieu.AsNoTracking()
            .Include(x => x.TaiKhoan)
            .SingleOrDefaultAsync(x => x.Id == id);
        return lichSu ?? throw new InvalidOperationException("Khong tim thay lich su import.");
    }

    private int CurrentTaiKhoanId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? HttpContext.Session.GetInt32("TaiKhoanId")?.ToString();
        return int.TryParse(raw, out var id) ? id : 1;
    }

    private static List<LoaiDuLieuOption> LoaiDuLieuOptions() =>
    [
        new(LoaiDuLieuNhap.SinhVien, "Sinh viên"),
        new(LoaiDuLieuNhap.GiangVien, "Giảng viên"),
        new(LoaiDuLieuNhap.MonHoc, "Môn học"),
        new(LoaiDuLieuNhap.LopHoc, "Lớp học"),
        new(LoaiDuLieuNhap.DangKyHoc, "Đăng ký học"),
        new(LoaiDuLieuNhap.BangDiem, "Bảng điểm"),
        new(LoaiDuLieuNhap.DiemDanh, "Điểm danh")
    ];

    private static List<LoaiFileOption> LoaiFileOptions() =>
    [
        new(LoaiFileNhap.Xlsx, "Excel", ".xlsx"),
        new(LoaiFileNhap.Txt, "TXT", ".txt"),
        new(LoaiFileNhap.Docx, "Word", ".docx")
    ];
}
