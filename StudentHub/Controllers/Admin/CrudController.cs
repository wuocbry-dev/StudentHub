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
        if (Validate(entity, form))
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
        if (Validate(entity, form) && await Save()) return RedirectSuccess("Cập nhật dữ liệu thành công.");
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
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectSuccess(string message) { TempData["Success"] = message; return RedirectToAction(nameof(Index)); }
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
    private static string HashPassword(TaiKhoan user, string password) => new PasswordHasher<TaiKhoan>().HashPassword(user, password);

    private static void BindForm(TEntity entity, IFormCollection form)
    {
        foreach (var p in typeof(TEntity).GetProperties().Where(IsEditable))
        {
            var raw = form[p.Name].ToString().Trim();
            if (p.PropertyType == typeof(string) && p.Name.StartsWith("Ma", StringComparison.Ordinal))
                raw = raw.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(raw)) { if (Nullable.GetUnderlyingType(p.PropertyType) != null || p.PropertyType == typeof(string)) p.SetValue(entity, null); continue; }
            try
            {
                var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
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
public class DangKyHocController(SimsDbContext db) : CrudController<DangKyHoc>(db, "Đăng ký học") { }
public class TaiKhoanController(SimsDbContext db) : CrudController<TaiKhoan>(db, "Tài khoản") { }
