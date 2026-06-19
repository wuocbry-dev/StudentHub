using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;

namespace StudentHub.Controllers.GiangVienPortal;

[Authorize(Roles = "GiangVien")]
public abstract class GiangVienPortalController(SimsDbContext db) : Controller
{
    protected SimsDbContext Db { get; } = db;

    protected async Task<int?> GetGiangVienId()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId)) return null;
        return await Db.GiangVien.Where(x => x.TaiKhoanId == taiKhoanId).Select(x => (int?)x.Id).SingleOrDefaultAsync();
    }

    protected async Task<bool> SoHuuLop(int lopHocId, int giangVienId) =>
        await Db.LopHoc.AnyAsync(x => x.Id == lopHocId && x.GiangVienId == giangVienId);
}
