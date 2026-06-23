using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Services.HocVuot;

public class DangKyHocVuotService(
    SimsDbContext db,
    IGioiHanHocVuotService gioiHanHocVuotService,
    IGoiYHocVuotService goiYHocVuotService) : IDangKyHocVuotService
{
    private const decimal GpaToiThieu = 6.5m;

    public async Task<List<LopHocVuotDangKyViewModel>> LayLopHocVuotCoTheDangKyAsync(int sinhVienId, string hocKy, string namHoc)
    {
        hocKy = ChuanHoa(hocKy);
        namHoc = ChuanHoa(namHoc);

        var sinhVien = await db.SinhVien.AsNoTracking().SingleOrDefaultAsync(x => x.Id == sinhVienId);
        if (sinhVien == null) return [];

        var lopHoc = await db.LopHoc.AsNoTracking()
            .Include(x => x.MonHoc)
            .Include(x => x.GiangVien)
            .Where(x => x.MonHoc != null
                && x.MonHoc.KhoaId == sinhVien.KhoaId
                && x.HocKy.Trim() == hocKy
                && x.NamHoc.Trim() == namHoc
                && (x.TrangThai == TrangThaiLopHoc.SapMo || x.TrangThai == TrangThaiLopHoc.DangHoc))
            .OrderBy(x => x.MonHoc!.MaMonHoc)
            .ThenBy(x => x.MaLop)
            .ToListAsync();

        if (lopHoc.Count == 0) return [];

        var lopIds = lopHoc.Select(x => x.Id).ToList();
        var lichHoc = await db.LichHoc.AsNoTracking()
            .Where(x => lopIds.Contains(x.LopHocId))
            .Include(x => x.PhongHoc)
            .ToListAsync();

        var result = new List<LopHocVuotDangKyViewModel>();
        foreach (var lop in lopHoc)
        {
            var danhGia = await DanhGiaLopHocVuotAsync(sinhVien, lop, hocKy, namHoc, lichHoc.Where(x => x.LopHocId == lop.Id).ToList());
            result.Add(danhGia);
        }

        return result;
    }

    public async Task<KetQuaDangKyHocVuotViewModel> DangKyHocVuotAsync(int sinhVienId, int lopHocId, string hocKy, string namHoc)
    {
        hocKy = ChuanHoa(hocKy);
        namHoc = ChuanHoa(namHoc);

        var soToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(hocKy, namHoc);
        var kiemTra = await KiemTraDieuKienHocVuotAsync(sinhVienId, lopHocId, hocKy, namHoc);
        if (!kiemTra.HopLe)
        {
            return new KetQuaDangKyHocVuotViewModel
            {
                ThanhCong = false,
                ThongBao = kiemTra.ThongBao,
                SoMonHocVuotDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVienId, hocKy, namHoc),
                SoMonHocVuotToiDa = soToiDa
            };
        }

        var lop = await db.LopHoc.Include(x => x.MonHoc).SingleAsync(x => x.Id == lopHocId);
        db.DangKyHoc.Add(new DangKyHoc
        {
            SinhVienId = sinhVienId,
            LopHocId = lopHocId,
            HocKy = hocKy,
            NamHoc = namHoc,
            LaHocVuot = true,
            TrangThai = TrangThaiDangKy.ChoDuyet,
            NgayDangKy = DateTime.Now
        });

        var goiY = await db.GoiYHocVuot
            .Where(x => x.SinhVienId == sinhVienId
                && (x.LopHocId == lopHocId || x.MonHocId == lop.MonHocId)
                && x.HocKyGoiY.Trim() == hocKy
                && x.NamHocGoiY.Trim() == namHoc
                && x.TrangThai != TrangThaiGoiY.DaBoQua
                && x.TrangThai != TrangThaiGoiY.HetHan)
            .ToListAsync();
        foreach (var item in goiY)
        {
            item.TrangThai = TrangThaiGoiY.DaDangKy;
            item.NgayCapNhat = DateTime.Now;
        }

        await db.SaveChangesAsync();

        return new KetQuaDangKyHocVuotViewModel
        {
            ThanhCong = true,
            ThongBao = "Đăng ký học vượt thành công. Trạng thái đăng ký đang chờ duyệt.",
            SoMonHocVuotDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVienId, hocKy, namHoc),
            SoMonHocVuotToiDa = soToiDa
        };
    }

    public async Task<KetQuaKiemTraHocVuotViewModel> KiemTraDieuKienHocVuotAsync(int sinhVienId, int lopHocId, string hocKy, string namHoc)
    {
        hocKy = ChuanHoa(hocKy);
        namHoc = ChuanHoa(namHoc);

        var sinhVien = await db.SinhVien.AsNoTracking().SingleOrDefaultAsync(x => x.Id == sinhVienId);
        if (sinhVien == null) return Loi("Không tìm thấy sinh viên.");

        var lop = await db.LopHoc.AsNoTracking()
            .Include(x => x.MonHoc)
            .SingleOrDefaultAsync(x => x.Id == lopHocId);
        if (lop?.MonHoc == null) return Loi("Không tìm thấy lớp học.");

        var danhSachLoi = await LayLoiDieuKienAsync(sinhVien, lop, hocKy, namHoc);
        return new KetQuaKiemTraHocVuotViewModel
        {
            HopLe = danhSachLoi.Count == 0,
            DanhSachLoi = danhSachLoi,
            ThongBao = danhSachLoi.Count == 0 ? "Đủ điều kiện đăng ký học vượt." : string.Join(" ", danhSachLoi)
        };
    }

    private async Task<LopHocVuotDangKyViewModel> DanhGiaLopHocVuotAsync(SinhVien sinhVien, LopHoc lop, string hocKy, string namHoc, List<LichHoc> lichLop)
    {
        var gpa = await goiYHocVuotService.TinhGPAAsync(sinhVien.Id);
        var soLuongDaDangKy = await DemSoLuongDangKyLopAsync(lop.Id);
        var monTienQuyetThieu = await LayMonTienQuyetThieuAsync(sinhVien.Id, lop.MonHocId);
        var daDangKy = await DaDangKyMonHoacLopAsync(sinhVien.Id, lop.Id, lop.MonHocId, hocKy, namHoc);
        var biTrungLich = await BiTrungLichAsync(sinhVien.Id, lop.Id);
        var soDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVien.Id, hocKy, namHoc);
        var soToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(hocKy, namHoc);
        var lopDaDay = soLuongDaDangKy >= lop.SoLuongToiDa;
        var daDuGioiHan = soDaDangKy >= soToiDa;

        var lyDo = new List<string>();
        if (lop.MonHoc?.KhoaId != sinhVien.KhoaId) lyDo.Add("Lớp không thuộc khoa của bạn.");
        if (lop.HocKy.Trim() != hocKy || lop.NamHoc.Trim() != namHoc) lyDo.Add("Lớp không thuộc học kỳ/năm học đã chọn.");
        if (lop.TrangThai is not (TrangThaiLopHoc.SapMo or TrangThaiLopHoc.DangHoc)) lyDo.Add("Lớp học không còn mở.");
        if (gpa < GpaToiThieu) lyDo.Add("GPA hiện tại chưa đủ điều kiện học vượt.");
        if (monTienQuyetThieu.Count > 0) lyDo.Add($"Bạn chưa hoàn thành môn tiên quyết: {string.Join(", ", monTienQuyetThieu)}.");
        if (daDangKy) lyDo.Add("Bạn đã đăng ký môn/lớp này.");
        if (daDuGioiHan) lyDo.Add($"Bạn đã đạt giới hạn {soToiDa} môn học vượt trong học kỳ này.");
        if (lopDaDay) lyDo.Add("Lớp học đã đủ số lượng.");
        if (biTrungLich) lyDo.Add("Lớp học bị trùng lịch với lớp đã đăng ký.");

        var diemPhuHop = TinhDiemPhuHop(gpa, monTienQuyetThieu.Count == 0, !biTrungLich, !lopDaDay);
        return new LopHocVuotDangKyViewModel
        {
            LopHocId = lop.Id,
            MaLop = lop.MaLop,
            TenLop = lop.TenLop,
            MonHocId = lop.MonHocId,
            MaMonHoc = lop.MonHoc?.MaMonHoc ?? "",
            TenMonHoc = lop.MonHoc?.TenMonHoc ?? "",
            SoTinChi = lop.MonHoc?.SoTinChi ?? 0,
            GiangVien = lop.GiangVien?.HoTen ?? "Chưa phân công",
            PhongHoc = string.Join(", ", lichLop.Select(x => x.PhongHoc?.MaPhong).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()),
            LichHocText = FormatLichHoc(lichLop),
            HocKy = hocKy,
            NamHoc = namHoc,
            SoLuongToiDa = lop.SoLuongToiDa,
            SoLuongDaDangKy = soLuongDaDangKy,
            SoChoConLai = Math.Max(0, lop.SoLuongToiDa - soLuongDaDangKy),
            GpaHienTai = gpa,
            GpaToiThieu = GpaToiThieu,
            DaHocMonTienQuyet = monTienQuyetThieu.Count == 0,
            DanhSachMonTienQuyetThieu = monTienQuyetThieu,
            DaDangKy = daDangKy,
            BiTrungLich = biTrungLich,
            DaDuGioiHanHocVuot = daDuGioiHan,
            LopDaDay = lopDaDay,
            CoTheDangKy = lyDo.Count == 0,
            LyDoKhongTheDangKy = string.Join(" ", lyDo),
            DiemPhuHop = diemPhuHop,
            MucDoPhuHop = XepMucDo(diemPhuHop)
        };
    }

    private async Task<List<string>> LayLoiDieuKienAsync(SinhVien sinhVien, LopHoc lop, string hocKy, string namHoc)
    {
        var lichLop = await db.LichHoc.AsNoTracking().Where(x => x.LopHocId == lop.Id).Include(x => x.PhongHoc).ToListAsync();
        var danhGia = await DanhGiaLopHocVuotAsync(sinhVien, lop, hocKy, namHoc, lichLop);
        return string.IsNullOrWhiteSpace(danhGia.LyDoKhongTheDangKy)
            ? []
            : danhGia.LyDoKhongTheDangKy.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.EndsWith('.') ? x : $"{x}.")
                .ToList();
    }

    private async Task<List<string>> LayMonTienQuyetThieuAsync(int sinhVienId, int monHocId)
    {
        var tienQuyet = await db.MonHocTienQuyet.AsNoTracking()
            .Where(x => x.MonHocId == monHocId && x.BatBuoc)
            .Include(x => x.MonHocTienQuyetCuaMon)
            .ToListAsync();
        if (tienQuyet.Count == 0) return [];

        var monDaDat = await db.BangDiem.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId
                && x.DiemTongKet >= 4m
                && x.DiemChu != "F")
            .Select(x => x.LopHoc!.MonHocId)
            .Distinct()
            .ToListAsync();

        return tienQuyet
            .Where(x => !monDaDat.Contains(x.MonHocTienQuyetId))
            .Select(x => x.MonHocTienQuyetCuaMon == null
                ? $"Môn #{x.MonHocTienQuyetId}"
                : $"{x.MonHocTienQuyetCuaMon.MaMonHoc} - {x.MonHocTienQuyetCuaMon.TenMonHoc}")
            .ToList();
    }

    private Task<int> DemSoLuongDangKyLopAsync(int lopHocId)
    {
        return db.DangKyHoc.AsNoTracking()
            .CountAsync(x => x.LopHocId == lopHocId
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
    }

    private Task<bool> DaDangKyMonHoacLopAsync(int sinhVienId, int lopHocId, int monHocId, string hocKy, string namHoc)
    {
        return db.DangKyHoc.AsNoTracking()
            .AnyAsync(x => x.SinhVienId == sinhVienId
                && (x.LopHocId == lopHocId || x.LopHoc!.MonHocId == monHocId)
                && x.HocKy.Trim() == hocKy
                && x.NamHoc.Trim() == namHoc
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi);
    }

    private async Task<bool> BiTrungLichAsync(int sinhVienId, int lopHocId)
    {
        var lichLopMoi = await db.LichHoc.AsNoTracking()
            .Where(x => x.LopHocId == lopHocId)
            .ToListAsync();
        if (lichLopMoi.Count == 0) return false;

        var lopDaDangKyIds = db.DangKyHoc.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId
                && x.TrangThai != TrangThaiDangKy.DaHuy
                && x.TrangThai != TrangThaiDangKy.TuChoi)
            .Select(x => x.LopHocId);

        var lichDangKy = await db.LichHoc.AsNoTracking()
            .Where(x => lopDaDangKyIds.Contains(x.LopHocId))
            .ToListAsync();

        return lichLopMoi.Any(a => lichDangKy.Any(b => TrungLich(a, b)));
    }

    private static KetQuaKiemTraHocVuotViewModel Loi(string thongBao) => new()
    {
        HopLe = false,
        ThongBao = thongBao,
        DanhSachLoi = [thongBao]
    };

    private static decimal TinhDiemPhuHop(decimal gpa, bool datTienQuyet, bool khongTrungLich, bool lopConCho)
    {
        var diem = Math.Min(30m, gpa / 10m * 30m);
        diem += datTienQuyet ? 30m : 0m;
        diem += khongTrungLich ? 20m : 0m;
        diem += lopConCho ? 10m : 0m;
        diem += gpa >= 8m ? 10m : gpa >= GpaToiThieu ? 7m : 0m;
        return Math.Round(Math.Min(100m, diem), 2);
    }

    private static string XepMucDo(decimal diem)
    {
        if (diem >= 85m) return "Rất phù hợp";
        if (diem >= 70m) return "Phù hợp";
        if (diem >= 50m) return "Cần cân nhắc";
        return "Không phù hợp";
    }

    private static string FormatLichHoc(List<LichHoc> lich)
    {
        if (lich.Count == 0) return "Chưa xếp lịch";
        return string.Join("; ", lich.OrderBy(x => x.ThuTrongTuan).ThenBy(x => x.GioBatDau)
            .Select(x => $"Thứ {x.ThuTrongTuan}, {x.GioBatDau:hh\\:mm}-{x.GioKetThuc:hh\\:mm}, {x.PhongHoc?.MaPhong ?? "Chưa xếp phòng"}"));
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
