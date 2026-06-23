using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/MonHocTienQuyet")]
public class MonHocTienQuyetController(SimsDbContext db) : Controller
{
    private const string IndexView = "~/Views/Admin/MonHocTienQuyet/Index.cshtml";

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string search = "", int? editId = null)
    {
        return View(IndexView, await BuildPageAsync(search, editId));
    }

    [HttpPost("Luu"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Luu(MonHocTienQuyetFormViewModel form, string search = "")
    {
        if (form.MonHocId == form.MonHocTienQuyetId)
            ModelState.AddModelError(nameof(form.MonHocTienQuyetId), "Môn tiên quyết phải khác môn học.");

        var biTrung = await db.MonHocTienQuyet.AnyAsync(x => x.Id != form.Id
            && x.MonHocId == form.MonHocId
            && x.MonHocTienQuyetId == form.MonHocTienQuyetId);
        if (biTrung)
            ModelState.AddModelError(nameof(form.MonHocTienQuyetId), "Cặp môn học tiên quyết này đã tồn tại.");

        if (!ModelState.IsValid)
        {
            var page = await BuildPageAsync(search, form.Id == 0 ? null : form.Id);
            page.Form = form;
            return View(IndexView, page);
        }

        MonHocTienQuyet entity;
        if (form.Id == 0)
        {
            entity = new MonHocTienQuyet { NgayTao = DateTime.Now };
            db.MonHocTienQuyet.Add(entity);
        }
        else
        {
            entity = await db.MonHocTienQuyet.FindAsync(form.Id) ?? new MonHocTienQuyet { NgayTao = DateTime.Now };
            if (entity.Id == 0) db.MonHocTienQuyet.Add(entity);
        }

        entity.MonHocId = form.MonHocId;
        entity.MonHocTienQuyetId = form.MonHocTienQuyetId;
        entity.BatBuoc = form.BatBuoc;
        entity.MucDo = form.MucDo;
        entity.GhiChu = string.IsNullOrWhiteSpace(form.GhiChu) ? null : form.GhiChu.Trim();

        await db.SaveChangesAsync();
        TempData["Success"] = "Lưu môn học tiên quyết thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Xoa/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var entity = await db.MonHocTienQuyet.FindAsync(id);
        if (entity != null)
        {
            db.MonHocTienQuyet.Remove(entity);
            await db.SaveChangesAsync();
            TempData["Success"] = "Xóa môn học tiên quyết thành công.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminMonHocTienQuyetPageViewModel> BuildPageAsync(string search, int? editId)
    {
        var query = db.MonHocTienQuyet.AsNoTracking()
            .Include(x => x.MonHoc)
            .Include(x => x.MonHocTienQuyetCuaMon)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.MonHoc!.MaMonHoc.Contains(search)
                || x.MonHoc.TenMonHoc.Contains(search)
                || x.MonHocTienQuyetCuaMon!.MaMonHoc.Contains(search)
                || x.MonHocTienQuyetCuaMon.TenMonHoc.Contains(search));
        }

        var model = new AdminMonHocTienQuyetPageViewModel
        {
            Search = search,
            EditId = editId,
            Items = await query.OrderBy(x => x.MonHoc!.MaMonHoc).ThenBy(x => x.MonHocTienQuyetCuaMon!.MaMonHoc).ToListAsync(),
            MonHocOptions = await db.MonHoc.AsNoTracking().OrderBy(x => x.MaMonHoc).ToListAsync()
        };

        if (editId.HasValue)
        {
            var item = await db.MonHocTienQuyet.AsNoTracking().SingleOrDefaultAsync(x => x.Id == editId);
            if (item != null)
            {
                model.Form = new MonHocTienQuyetFormViewModel
                {
                    Id = item.Id,
                    MonHocId = item.MonHocId,
                    MonHocTienQuyetId = item.MonHocTienQuyetId,
                    BatBuoc = item.BatBuoc,
                    MucDo = item.MucDo,
                    GhiChu = item.GhiChu
                };
            }
        }

        return model;
    }
}
