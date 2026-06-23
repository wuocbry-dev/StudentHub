using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.Services.HocVuot;

namespace StudentHub.Controllers.Admin;

[Authorize(Roles = "Admin")]
public abstract class CrudController<TEntity>(SimsDbContext db, string title) : Controller where TEntity : class, new()
{
    protected SimsDbContext Db => db;
    public async Task<IActionResult> Index(string search = "", int page = 1)
    {
        const int pageSize = 10;
        page = Math.Max(page, 1);
        IQueryable<TEntity> query = db.Set<TEntity>().AsNoTracking();
        foreach (var nav in db.Model.FindEntityType(typeof(TEntity))!.GetNavigations()) query = query.Include(nav.Name);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var p = Expression.Parameter(typeof(TEntity), "x");
            Expression? body = null;
            foreach (var prop in typeof(TEntity).GetProperties().Where(x => x.PropertyType == typeof(string)))
            {
                var value = Expression.Property(p, prop);
                var notNull = Expression.NotEqual(value, Expression.Constant(null, typeof(string)));
                var contains = Expression.Call(value, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search));
                var item = Expression.AndAlso(notNull, contains);
                body = body == null ? item : Expression.OrElse(body, item);
            }
            if (body != null) query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, p));
        }
        var count = await query.CountAsync();
        var items = (await query.OrderByDescending(x => EF.Property<int>(x, "Id")).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()).Cast<object>().ToList();
        return View("~/Views/Admin/Crud/Index.cshtml", new CrudPageViewModel { EntityType = typeof(TEntity), Title = title, Items = items, Search = search, Page = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)) });
    }

    public async Task<IActionResult> Create()
    {
        await LoadOptions();
        return View("~/Views/Admin/Crud/Edit.cshtml", new CrudEditViewModel { EntityType = typeof(TEntity), Title = title, Entity = new TEntity() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormCollection form)
    {
        var entity = new TEntity();
        BindForm(entity, form);
        if (entity is TaiKhoan tk) tk.MatKhauHash = HashPassword(tk, form["MatKhau"]!);
        var entityValid = await ValidateEntityAsync(entity, form);
        if (Validate(entity, form) && entityValid)
        {
            db.Add(entity);
            if (await Save()) return RedirectSuccess("Thêm dữ liệu thành công.");
        }
        await LoadOptions();
        return View("~/Views/Admin/Crud/Edit.cshtml", new CrudEditViewModel { EntityType = typeof(TEntity), Title = title, Entity = entity });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.Set<TEntity>().FindAsync(id);
        if (entity == null) return NotFound();
        await LoadOptions();
        return View("~/Views/Admin/Crud/Edit.cshtml", new CrudEditViewModel { EntityType = typeof(TEntity), Title = title, Entity = entity, IsEdit = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IFormCollection form)
    {
        var entity = await db.Set<TEntity>().FindAsync(id);
        if (entity == null) return NotFound();
        BindForm(entity, form);
        if (entity is TaiKhoan tk && !string.IsNullOrWhiteSpace(form["MatKhau"])) tk.MatKhauHash = HashPassword(tk, form["MatKhau"]!);
        var entityValid = await ValidateEntityAsync(entity, form);
        if (Validate(entity, form) && entityValid && await Save()) return RedirectSuccess("Cập nhật dữ liệu thành công.");
        await LoadOptions();
        return View("~/Views/Admin/Crud/Edit.cshtml", new CrudEditViewModel { EntityType = typeof(TEntity), Title = title, Entity = entity, IsEdit = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Set<TEntity>().FindAsync(id);
        if (entity == null) return NotFound();
        db.Remove(entity);
        if (await Save()) TempData["Success"] = "Xóa dữ liệu thành công.";
        return Redirect(AdminIndexUrl());
    }

    private IActionResult RedirectSuccess(string message) { TempData["Success"] = message; return Redirect(AdminIndexUrl()); }
    private string AdminIndexUrl() => $"/Admin/{ControllerContext.ActionDescriptor.ControllerName}";
    private async Task<bool> Save()
    {
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateException) { ModelState.AddModelError("", "Dữ liệu bị trùng hoặc đang được sử dụng, không thể lưu thay đổi."); return false; }
    }
    private bool Validate(TEntity entity, IFormCollection form)
    {
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(entity, new ValidationContext(entity), results, true);
        foreach (var e in results) ModelState.AddModelError(e.MemberNames.FirstOrDefault() ?? "", e.ErrorMessage ?? "Dữ liệu không hợp lệ.");
        if (entity is TaiKhoan && string.IsNullOrWhiteSpace(form["MatKhau"]) && Convert.ToInt32(typeof(TEntity).GetProperty("Id")!.GetValue(entity)) == 0)
        { ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu."); valid = false; }
        return valid;
    }
    protected virtual Task<bool> ValidateEntityAsync(TEntity entity, IFormCollection form) => Task.FromResult(true);
    private static string HashPassword(TaiKhoan user, string password) => new PasswordHasher<TaiKhoan>().HashPassword(user, password);

    private static void BindForm(TEntity entity, IFormCollection form)
    {
        foreach (var p in typeof(TEntity).GetProperties().Where(IsEditable))
        {
            var raw = form[p.Name].ToString().Trim();
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            if (t == typeof(bool))
            {
                p.SetValue(entity, form.ContainsKey(p.Name) && bool.TryParse(raw, out var value) && value);
                continue;
            }
            if (p.PropertyType == typeof(string) && p.Name.StartsWith("Ma", StringComparison.Ordinal))
                raw = raw.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(raw)) { if (Nullable.GetUnderlyingType(p.PropertyType) != null || p.PropertyType == typeof(string)) p.SetValue(entity, null); continue; }
            try
            {
                object value = t.IsEnum ? Enum.Parse(t, raw) : t == typeof(TimeSpan) ? TimeSpan.Parse(raw, CultureInfo.InvariantCulture) : Convert.ChangeType(raw, t, CultureInfo.InvariantCulture);
                p.SetValue(entity, value);
            }
            catch { }
        }
    }
    public static bool IsEditable(PropertyInfo p) => p.CanWrite && p.Name != "Id" && p.Name != "MatKhauHash" && !p.Name.EndsWith("NgayTao") && (p.PropertyType.IsValueType || p.PropertyType == typeof(string));
    private async Task LoadOptions()
    {
        foreach (var p in typeof(TEntity).GetProperties().Where(x => x.Name.EndsWith("Id") && x.Name != "Id"))
        {
            var target = db.Model.FindEntityType(typeof(TEntity))?.FindProperty(p.Name)?.GetContainingForeignKeys().FirstOrDefault()?.PrincipalEntityType.ClrType;
            if (target == null) continue;
            var set = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(target).Invoke(db, null)!;
            var list = await ((IQueryable<object>)set).AsNoTracking().ToListAsync();
            ViewData["Options_" + p.Name] = list;
        }
    }
}

public class KhoaController(SimsDbContext db) : CrudController<Khoa>(db, "Khoa") { }
public class SinhVienController(SimsDbContext db) : CrudController<SinhVien>(db, "Sinh viên") { }
public class GiangVienController(SimsDbContext db) : CrudController<GiangVien>(db, "Giảng viên") { }
public class MonHocController(SimsDbContext db) : CrudController<MonHoc>(db, "Môn học") { }
public class PhongHocController(SimsDbContext db) : CrudController<PhongHoc>(db, "Phòng học") { }
public class LopHocController(SimsDbContext db) : CrudController<LopHoc>(db, "Lớp học") { }
public class LichHocController(SimsDbContext db) : CrudController<LichHoc>(db, "Lịch học") { }
public class DangKyHocController(SimsDbContext db, IGioiHanHocVuotService gioiHanHocVuotService) : CrudController<DangKyHoc>(db, "Đăng ký học")
{
    protected override async Task<bool> ValidateEntityAsync(DangKyHoc entity, IFormCollection form)
    {
        var valid = true;
        var lopHoc = await Db.LopHoc.AsNoTracking().SingleOrDefaultAsync(x => x.Id == entity.LopHocId);
        if (lopHoc == null)
        {
            ModelState.AddModelError(nameof(DangKyHoc.LopHocId), "Lop hoc khong ton tai.");
            valid = false;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(entity.HocKy)) entity.HocKy = lopHoc.HocKy;
            if (string.IsNullOrWhiteSpace(entity.NamHoc)) entity.NamHoc = lopHoc.NamHoc;

            var soDaDangKyLop = await Db.DangKyHoc.CountAsync(x => x.LopHocId == entity.LopHocId
                && x.Id != entity.Id
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
            if (soDaDangKyLop >= lopHoc.SoLuongToiDa)
            {
                ModelState.AddModelError(nameof(DangKyHoc.LopHocId), "Lop hoc da du so luong.");
                valid = false;
            }
        }

        if (!await Db.SinhVien.AnyAsync(x => x.Id == entity.SinhVienId))
        {
            ModelState.AddModelError(nameof(DangKyHoc.SinhVienId), "Sinh vien khong ton tai.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(entity.HocKy))
        {
            ModelState.AddModelError(nameof(DangKyHoc.HocKy), "Hoc ky khong duoc trong.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(entity.NamHoc))
        {
            ModelState.AddModelError(nameof(DangKyHoc.NamHoc), "Nam hoc khong duoc trong.");
            valid = false;
        }

        if (entity.LaHocVuot && entity.TrangThai is not (TrangThaiDangKy.DaHuy or TrangThaiDangKy.TuChoi))
        {
            var soDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(entity.SinhVienId, entity.HocKy, entity.NamHoc);
            var soToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(entity.HocKy, entity.NamHoc);
            var dangSuaBanGhiDangDuocTinh = entity.Id > 0 && await Db.DangKyHoc.AsNoTracking().AnyAsync(x => x.Id == entity.Id
                && x.LaHocVuot
                && x.SinhVienId == entity.SinhVienId
                && x.HocKy == entity.HocKy
                && x.NamHoc == entity.NamHoc
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);

            if (!dangSuaBanGhiDangDuocTinh && soDaDangKy >= soToiDa)
            {
                ModelState.AddModelError(nameof(DangKyHoc.LaHocVuot), await gioiHanHocVuotService.LayThongBaoGioiHanAsync(entity.SinhVienId, entity.HocKy, entity.NamHoc));
                valid = false;
            }
        }

        return valid;
    }
}
public class TaiKhoanController(SimsDbContext db) : CrudController<TaiKhoan>(db, "Tài khoản") { }
