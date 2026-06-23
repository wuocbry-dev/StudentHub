using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public class GoiYHocVuotService(SimsDbContext db, IGioiHanHocVuotService gioiHanHocVuotService) : IGoiYHocVuotService
{
    public async Task<List<GoiYHocVuotViewModel>> TaoGoiYChoSinhVienAsync(int sinhVienId, string hocKy, string namHoc)
    {
        hocKy = ChuanHoa(hocKy);
        namHoc = ChuanHoa(namHoc);

        if (!await gioiHanHocVuotService.CoTheDangKyHocVuotAsync(sinhVienId, hocKy, namHoc))
            return [];

        var sinhVienTonTai = await db.SinhVien.AnyAsync(x => x.Id == sinhVienId && x.TrangThai == TrangThaiSinhVien.DangHoc);
        if (!sinhVienTonTai) return [];

        var gpa = await TinhGPAAsync(sinhVienId);
        if (gpa < 5m) return [];

        var monDaHocDat = await LayMonDaHocDatIdsAsync(sinhVienId);
        var monDaDangKy = await LayMonDaDangKyIdsAsync(sinhVienId);

        var existing = await db.GoiYHocVuot
            .Where(x => x.SinhVienId == sinhVienId
                && x.HocKyGoiY == hocKy
                && x.NamHocGoiY == namHoc
                && x.TrangThai != TrangThaiGoiY.DaDangKy)
            .ToListAsync();
        db.GoiYHocVuot.RemoveRange(existing);

        var lopHoc = await db.LopHoc.AsNoTracking()
            .Include(x => x.MonHoc).ThenInclude(x => x!.Khoa)
            .Include(x => x.GiangVien)
            .Where(x => x.HocKy == hocKy
                && x.NamHoc == namHoc
                && (x.TrangThai == TrangThaiLopHoc.SapMo || x.TrangThai == TrangThaiLopHoc.DangHoc))
            .OrderBy(x => x.MonHoc!.MaMonHoc)
            .ThenBy(x => x.MaLop)
            .ToListAsync();

        var goiYMoi = new List<GoiYHocVuot>();
        foreach (var lop in lopHoc)
        {
            if (lop.MonHoc == null) continue;
            if (monDaHocDat.Contains(lop.MonHocId)) continue;
            if (monDaDangKy.Contains(lop.MonHocId)) continue;
            if (!await KiemTraMonTienQuyetAsync(sinhVienId, lop.MonHocId)) continue;
            if (!await LopConChoAsync(lop.Id)) continue;
            if (await KiemTraTrungLichAsync(sinhVienId, lop.Id)) continue;

            var diemPhuHop = await TinhDiemPhuHopAsync(sinhVienId, lop, gpa);
            var mucDo = XepMucDo(diemPhuHop);
            if (mucDo == MucDoGoiY.KhongPhuHop) continue;

            goiYMoi.Add(new GoiYHocVuot
            {
                SinhVienId = sinhVienId,
                MonHocId = lop.MonHocId,
                LopHocId = lop.Id,
                HocKyGoiY = hocKy,
                NamHocGoiY = namHoc,
                DiemPhuHop = diemPhuHop,
                MucDoGoiY = mucDo,
                TrangThai = TrangThaiGoiY.Moi,
                LyDoGoiY = await TaoLyDoGoiYAsync(sinhVienId, lop, gpa, diemPhuHop)
            });
        }

        if (goiYMoi.Count > 0) db.GoiYHocVuot.AddRange(goiYMoi);
        await db.SaveChangesAsync();

        return await LayGoiYCuaSinhVienAsync(sinhVienId);
    }

    public async Task<List<GoiYHocVuotViewModel>> LayGoiYCuaSinhVienAsync(int sinhVienId)
    {
        var goiY = await db.GoiYHocVuot.AsNoTracking()
            .Include(x => x.SinhVien).ThenInclude(x => x!.Khoa)
            .Include(x => x.MonHoc)
            .Include(x => x.LopHoc).ThenInclude(x => x!.GiangVien)
            .Where(x => x.SinhVienId == sinhVienId
                && x.TrangThai != TrangThaiGoiY.DaBoQua
                && x.TrangThai != TrangThaiGoiY.HetHan)
            .OrderByDescending(x => x.DiemPhuHop)
            .ThenByDescending(x => x.NgayTao)
            .ToListAsync();

        return await ToViewModelsAsync(goiY);
    }

    public async Task CapNhatTrangThaiGoiYAsync(int goiYHocVuotId, string trangThai)
    {
        var goiY = await db.GoiYHocVuot.FindAsync(goiYHocVuotId);
        if (goiY == null) return;
        if (!Enum.TryParse<TrangThaiGoiY>(trangThai, out var value)) return;
        goiY.TrangThai = value;
        goiY.NgayCapNhat = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<bool> KiemTraMonTienQuyetAsync(int sinhVienId, int monHocId)
    {
        var batBuocIds = await db.MonHocTienQuyet.AsNoTracking()
            .Where(x => x.MonHocId == monHocId && x.BatBuoc)
            .Select(x => x.MonHocTienQuyetId)
            .ToListAsync();

        if (batBuocIds.Count == 0) return true;

        var monDaHocDat = await LayMonDaHocDatIdsAsync(sinhVienId);
        return batBuocIds.All(monDaHocDat.Contains);
    }

    public async Task<bool> KiemTraTrungLichAsync(int sinhVienId, int lopHocId)
    {
        var lichLopMoi = await db.LichHoc.AsNoTracking()
            .Where(x => x.LopHocId == lopHocId)
            .ToListAsync();
        if (lichLopMoi.Count == 0) return false;

        var lopDaDangKyIds = db.DangKyHoc
            .Where(x => x.SinhVienId == sinhVienId
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi)
            .Select(x => x.LopHocId);

        var lichDangKy = await db.LichHoc.AsNoTracking()
            .Where(x => lopDaDangKyIds.Contains(x.LopHocId))
            .ToListAsync();

        return lichLopMoi.Any(a => lichDangKy.Any(b => TrungLich(a, b)));
    }

    public async Task<decimal> TinhGPAAsync(int sinhVienId)
    {
        var diem = await db.BangDiem.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId && x.DiemTongKet.HasValue)
            .Select(x => new { x.DiemTongKet, SoTinChi = x.LopHoc!.MonHoc!.SoTinChi })
            .ToListAsync();

        var tongTinChi = diem.Sum(x => x.SoTinChi);
        return tongTinChi == 0 ? 0 : Math.Round(diem.Sum(x => x.DiemTongKet!.Value * x.SoTinChi) / tongTinChi, 2);
    }

    public async Task<(bool ThanhCong, string ThongBao)> DangKyTuGoiYAsync(int sinhVienId, int goiYHocVuotId)
    {
        var goiY = await db.GoiYHocVuot
            .Include(x => x.LopHoc)
            .SingleOrDefaultAsync(x => x.Id == goiYHocVuotId && x.SinhVienId == sinhVienId);
        if (goiY == null) return (false, "Không tìm thấy gợi ý học vượt.");
        if (goiY.TrangThai is TrangThaiGoiY.DaBoQua or TrangThaiGoiY.HetHan)
            return (false, "Gợi ý học vượt không còn hiệu lực.");
        if (goiY.TrangThai == TrangThaiGoiY.DaDangKy)
            return (false, "Bạn đã đăng ký từ gợi ý này.");
        goiY.HocKyGoiY = ChuanHoa(goiY.HocKyGoiY);
        goiY.NamHocGoiY = ChuanHoa(goiY.NamHocGoiY);

        if (await DaDangKyMonHoacLopAsync(sinhVienId, goiY.MonHocId, goiY.LopHocId))
        {
            await CapNhatGoiYDaDangKyAsync(sinhVienId, goiY.MonHocId, goiY.LopHocId, goiY.HocKyGoiY, goiY.NamHocGoiY);
            return (false, "Ban da dang ky lop hoac mon hoc nay.");
        }

        if (!await gioiHanHocVuotService.CoTheDangKyHocVuotAsync(sinhVienId, goiY.HocKyGoiY, goiY.NamHocGoiY))
            return (false, await gioiHanHocVuotService.LayThongBaoGioiHanAsync(sinhVienId, goiY.HocKyGoiY, goiY.NamHocGoiY));
        if (!await LopConChoAsync(goiY.LopHocId))
            return (false, "Lớp học đã đủ số lượng.");
        if (await KiemTraTrungLichAsync(sinhVienId, goiY.LopHocId))
            return (false, "Lớp học bị trùng lịch với lớp đã đăng ký.");
        if (!await KiemTraMonTienQuyetAsync(sinhVienId, goiY.MonHocId))
            return (false, "Bạn chưa đạt môn tiên quyết bắt buộc.");

        var daDangKy = await db.DangKyHoc.AnyAsync(x => x.SinhVienId == sinhVienId
            && x.LopHocId == goiY.LopHocId
            && x.TrangThai != TrangThaiDangKy.DaHuy
            && x.TrangThai != TrangThaiDangKy.TuChoi);
        if (daDangKy) return (false, "Bạn đã đăng ký lớp học này.");

        db.DangKyHoc.Add(new DangKyHoc
        {
            SinhVienId = sinhVienId,
            LopHocId = goiY.LopHocId,
            HocKy = goiY.HocKyGoiY,
            NamHoc = goiY.NamHocGoiY,
            LaHocVuot = true,
            TrangThai = TrangThaiDangKy.ChoDuyet
        });

        goiY.TrangThai = TrangThaiGoiY.DaDangKy;
        goiY.NgayCapNhat = DateTime.Now;
        var goiYCungMonHoacLop = await db.GoiYHocVuot
            .Where(x => x.SinhVienId == sinhVienId
                && x.Id != goiY.Id
                && (x.LopHocId == goiY.LopHocId || x.MonHocId == goiY.MonHocId)
                && x.HocKyGoiY.Trim() == goiY.HocKyGoiY
                && x.NamHocGoiY.Trim() == goiY.NamHocGoiY
                && x.TrangThai != TrangThaiGoiY.DaBoQua
                && x.TrangThai != TrangThaiGoiY.HetHan)
            .ToListAsync();
        foreach (var item in goiYCungMonHoacLop)
        {
            item.TrangThai = TrangThaiGoiY.DaDangKy;
            item.NgayCapNhat = DateTime.Now;
        }
        await db.SaveChangesAsync();
        return (true, "Đăng ký học vượt thành công. Trạng thái đăng ký đang chờ duyệt.");
    }

    private async Task<bool> DaDangKyMonHoacLopAsync(int sinhVienId, int monHocId, int lopHocId)
    {
        return await db.DangKyHoc.AsNoTracking()
            .AnyAsync(x => x.SinhVienId == sinhVienId
                && (x.LopHocId == lopHocId || x.LopHoc!.MonHocId == monHocId)
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
    }

    private async Task CapNhatGoiYDaDangKyAsync(int sinhVienId, int monHocId, int lopHocId, string hocKy, string namHoc)
    {
        var goiYDaDangKy = await db.GoiYHocVuot
            .Where(x => x.SinhVienId == sinhVienId
                && (x.LopHocId == lopHocId || x.MonHocId == monHocId)
                && x.HocKyGoiY.Trim() == hocKy
                && x.NamHocGoiY.Trim() == namHoc
                && x.TrangThai != TrangThaiGoiY.DaBoQua
                && x.TrangThai != TrangThaiGoiY.HetHan)
            .ToListAsync();

        foreach (var item in goiYDaDangKy)
        {
            item.TrangThai = TrangThaiGoiY.DaDangKy;
            item.NgayCapNhat = DateTime.Now;
        }

        if (goiYDaDangKy.Count > 0) await db.SaveChangesAsync();
    }

    private async Task<HashSet<int>> LayMonDaHocDatIdsAsync(int sinhVienId)
    {
        var ids = await db.BangDiem.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId
                && x.DiemTongKet >= 4m
                && x.DiemChu != "F")
            .Select(x => x.LopHoc!.MonHocId)
            .Distinct()
            .ToListAsync();
        return ids.ToHashSet();
    }

    private async Task<HashSet<int>> LayMonDaDangKyIdsAsync(int sinhVienId)
    {
        var ids = await db.DangKyHoc.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi)
            .Select(x => x.LopHoc!.MonHocId)
            .Distinct()
            .ToListAsync();
        return ids.ToHashSet();
    }

    private async Task<bool> LopConChoAsync(int lopHocId)
    {
        var lop = await db.LopHoc.AsNoTracking().SingleOrDefaultAsync(x => x.Id == lopHocId);
        if (lop == null) return false;
        var soDaDangKy = await db.DangKyHoc.AsNoTracking()
            .CountAsync(x => x.LopHocId == lopHocId
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
        return soDaDangKy < lop.SoLuongToiDa;
    }

    private async Task<decimal> TinhDiemPhuHopAsync(int sinhVienId, LopHoc lop, decimal gpa)
    {
        var diemGpa = Math.Min(30m, gpa / 10m * 30m);
        var diemTienQuyet = await TinhDiemTienQuyetAsync(sinhVienId, lop.MonHocId);
        var diemLich = await KiemTraTrungLichAsync(sinhVienId, lop.Id) ? 0m : 20m;
        var diemLop = await TinhDiemLopConChoAsync(lop);
        var diemDoKho = lop.MonHoc?.SoTinChi <= 3 ? 10m : lop.MonHoc?.SoTinChi == 4 ? 7m : 5m;
        return Math.Round(Math.Min(100m, diemGpa + diemTienQuyet + diemLich + diemLop + diemDoKho), 2);
    }

    private async Task<decimal> TinhDiemTienQuyetAsync(int sinhVienId, int monHocId)
    {
        var tienQuyet = await db.MonHocTienQuyet.AsNoTracking()
            .Where(x => x.MonHocId == monHocId)
            .ToListAsync();
        if (tienQuyet.Count == 0) return 30m;

        var monDaHocDat = await LayMonDaHocDatIdsAsync(sinhVienId);
        if (tienQuyet.Any(x => x.BatBuoc && !monDaHocDat.Contains(x.MonHocTienQuyetId))) return 0m;

        var soDat = tienQuyet.Count(x => monDaHocDat.Contains(x.MonHocTienQuyetId));
        return 20m + Math.Round(10m * soDat / tienQuyet.Count, 2);
    }

    private async Task<decimal> TinhDiemLopConChoAsync(LopHoc lop)
    {
        var soDaDangKy = await db.DangKyHoc.AsNoTracking()
            .CountAsync(x => x.LopHocId == lop.Id
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
        var conTrong = Math.Max(0, lop.SoLuongToiDa - soDaDangKy);
        return lop.SoLuongToiDa <= 0 ? 0 : Math.Round(10m * conTrong / lop.SoLuongToiDa, 2);
    }

    private async Task<string> TaoLyDoGoiYAsync(int sinhVienId, LopHoc lop, decimal gpa, decimal diemPhuHop)
    {
        var tienQuyetDat = await KiemTraMonTienQuyetAsync(sinhVienId, lop.MonHocId);
        var mucGpa = gpa >= 8m ? "GPA cao" : gpa >= 6.5m ? "GPA phù hợp" : "Cần cân nhắc học lực";
        var tienQuyet = tienQuyetDat ? "đã đạt môn tiên quyết bắt buộc" : "thiếu môn tiên quyết";
        return $"{mucGpa}, {tienQuyet}, lớp còn chỗ, không trùng lịch. Điểm phù hợp {diemPhuHop:0.##}/100.";
    }

    private async Task<List<GoiYHocVuotViewModel>> ToViewModelsAsync(List<GoiYHocVuot> goiY)
    {
        var ids = goiY.Select(x => x.LopHocId).ToList();
        var lich = await db.LichHoc.AsNoTracking()
            .Where(x => ids.Contains(x.LopHocId))
            .Include(x => x.PhongHoc)
            .ToListAsync();

        var sinhVienIds = goiY.Select(x => x.SinhVienId).Distinct().ToList();
        var dangKyHienTai = await db.DangKyHoc.AsNoTracking()
            .Where(x => sinhVienIds.Contains(x.SinhVienId)
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi)
            .Select(x => new { x.SinhVienId, x.LopHocId, MonHocId = x.LopHoc!.MonHocId })
            .ToListAsync();

        var result = new List<GoiYHocVuotViewModel>();
        foreach (var item in goiY)
        {
            var lichText = string.Join("; ", lich.Where(x => x.LopHocId == item.LopHocId)
                .OrderBy(x => x.ThuTrongTuan)
                .ThenBy(x => x.GioBatDau)
                .Select(x => $"Thứ {x.ThuTrongTuan}, {x.GioBatDau:hh\\:mm}-{x.GioKetThuc:hh\\:mm}, {x.PhongHoc?.MaPhong ?? "Chưa xếp phòng"}"));
            var soDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(item.SinhVienId, item.HocKyGoiY, item.NamHocGoiY);
            var soToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(item.HocKyGoiY, item.NamHocGoiY);
            var daDatGioiHan = soDaDangKy >= soToiDa;
            var daDangKy = item.TrangThai == TrangThaiGoiY.DaDangKy
                || dangKyHienTai.Any(x => x.SinhVienId == item.SinhVienId
                    && (x.LopHocId == item.LopHocId || x.MonHocId == item.MonHocId));
            var coTheDangKy = !daDangKy && !daDatGioiHan;
            var lyDoKhongTheDangKy = daDangKy
                ? "Da dang ky"
                : daDatGioiHan ? "Da dat gioi han" : "";

            result.Add(new GoiYHocVuotViewModel
            {
                Id = item.Id,
                SinhVienId = item.SinhVienId,
                MaSinhVien = item.SinhVien?.MaSinhVien ?? "",
                HoTenSinhVien = item.SinhVien?.HoTen ?? "",
                TenKhoa = item.SinhVien?.Khoa?.TenKhoa ?? "",
                MonHocId = item.MonHocId,
                MaMonHoc = item.MonHoc?.MaMonHoc ?? "",
                TenMonHoc = item.MonHoc?.TenMonHoc ?? "",
                SoTinChi = item.MonHoc?.SoTinChi ?? 0,
                LopHocId = item.LopHocId,
                MaLop = item.LopHoc?.MaLop ?? "",
                TenLop = item.LopHoc?.TenLop ?? "",
                GiangVien = item.LopHoc?.GiangVien?.HoTen ?? "Chưa phân công",
                LichHoc = string.IsNullOrWhiteSpace(lichText) ? "Chưa xếp lịch" : lichText,
                HocKyGoiY = item.HocKyGoiY,
                NamHocGoiY = item.NamHocGoiY,
                DiemPhuHop = item.DiemPhuHop,
                LyDoGoiY = item.LyDoGoiY,
                MucDoGoiY = item.MucDoGoiY,
                TrangThai = item.TrangThai,
                NgayTao = item.NgayTao,
                DaDatGioiHan = daDatGioiHan,
                DaDangKy = daDangKy,
                CoTheDangKy = coTheDangKy,
                LyDoKhongTheDangKy = lyDoKhongTheDangKy
            });
        }

        return result;
    }

    private static MucDoGoiY XepMucDo(decimal diemPhuHop)
    {
        if (diemPhuHop >= 85m) return MucDoGoiY.RatPhuHop;
        if (diemPhuHop >= 70m) return MucDoGoiY.PhuHop;
        if (diemPhuHop >= 50m) return MucDoGoiY.CanCanNhac;
        return MucDoGoiY.KhongPhuHop;
    }

    private static bool TrungLich(LichHoc a, LichHoc b)
    {
        return a.ThuTrongTuan == b.ThuTrongTuan
            && a.NgayBatDau.Date <= b.NgayKetThuc.Date
            && b.NgayBatDau.Date <= a.NgayKetThuc.Date
            && a.GioBatDau < b.GioKetThuc
            && b.GioBatDau < a.GioKetThuc;
    }

    private static string ChuanHoa(string value) => value.Trim();
}
