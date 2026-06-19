using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers;

public class TaiKhoanController(SimsDbContext db) : Controller
{
    [AllowAnonymous]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new DangNhapViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.TaiKhoan.SingleOrDefaultAsync(x => x.TenDangNhap == model.TenDangNhap);
        if (user == null || user.TrangThai != TrangThaiTaiKhoan.HoatDong ||
            new PasswordHasher<TaiKhoan>().VerifyHashedPassword(user, user.MatKhauHash, model.MatKhau) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.HoTen), new Claim(ClaimTypes.Role, user.VaiTro.ToString()) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = model.GhiNho });
        HttpContext.Session.SetInt32("TaiKhoanId", user.Id);
        HttpContext.Session.SetString("HoTen", user.HoTen);
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : user.VaiTro == VaiTro.Admin ? "/Admin/Dashboard" : "/");
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(DangNhap));
    }

    public IActionResult TuChoiTruyCap() => View();
}
