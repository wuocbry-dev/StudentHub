using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Services;

public class CanhBaoHocTapService(SimsDbContext db)
{
    public async Task<List<CanhBaoSinhVien>> DongBoVaLayAsync(int sinhVienId)
    {
        var dangKy = await db.DangKyHoc.AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId && x.TrangThai != TrangThaiDangKy.DaHuy)
            .Select(x => new
            {
                x.LopHocId, x.LopHoc!.MaLop, x.LopHoc.TrangThai,
                TenMonHoc = x.LopHoc.MonHoc!.TenMonHoc
            }).ToListAsync();
        var lopIds = dangKy.Select(x => x.LopHocId).ToList();
        var deXuat = new Dictionary<(int LopHocId, LoaiCanhBao Loai), (string NoiDung, MucDo MucDo)>();

        try
        {
            var bangDiem = await db.BangDiem.AsNoTracking()
                .Where(x => x.SinhVienId == sinhVienId && lopIds.Contains(x.LopHocId)).ToListAsync();
            foreach (var lop in dangKy)
            {
                var diem = bangDiem.FirstOrDefault(x => x.LopHocId == lop.LopHocId);
                if (diem?.DiemTongKet < 4)
                    deXuat[(lop.LopHocId, LoaiCanhBao.DiemThap)] =
                        ($"Điểm tổng kết môn {lop.TenMonHoc} hiện là {diem.DiemTongKet:0.00}, dưới mức đạt 4.0.", MucDo.NguyHiem);
                if (diem?.DiemCuoiKy == null && (lop.TrangThai == TrangThaiLopHoc.DaKetThuc
                    || diem != null && (diem.DiemChuyenCan.HasValue || diem.DiemBaiTap.HasValue || diem.DiemGiuaKy.HasValue)))
                    deXuat[(lop.LopHocId, LoaiCanhBao.ThieuDiem)] =
                        ($"Môn {lop.TenMonHoc} chưa có điểm cuối kỳ. Vui lòng theo dõi cập nhật từ giảng viên.", MucDo.CanhBao);
            }
        }
        catch (DbException)
        {
            // BangDiem is optional in databases that have not merged the lecturer module.
        }

        try
        {
            var diemDanh = await db.DiemDanh.AsNoTracking()
                .Where(x => x.SinhVienId == sinhVienId && lopIds.Contains(x.PhienDiemDanh!.LopHocId))
                .Select(x => new { x.PhienDiemDanh!.LopHocId, x.TrangThai }).ToListAsync();
            foreach (var lop in dangKy)
            {
                var ds = diemDanh.Where(x => x.LopHocId == lop.LopHocId).ToList();
                if (ds.Count == 0) continue;
                var vang = ds.Count(x => x.TrangThai == TrangThaiDiemDanh.Vang);
                var thamGia = ds.Count(x => x.TrangThai is TrangThaiDiemDanh.CoMat or TrangThaiDiemDanh.DiMuon);
                var tyLe = thamGia * 100d / ds.Count;
                if (tyLe < 70 || vang >= 3)
                {
                    var mucDo = tyLe < 50 || vang >= 5 ? MucDo.NguyHiem : MucDo.CanhBao;
                    deXuat[(lop.LopHocId, LoaiCanhBao.ChuyenCanThap)] =
                        ($"Chuyên cần môn {lop.TenMonHoc} là {tyLe:0.0}% và đã vắng {vang}/{ds.Count} buổi.", mucDo);
                }
            }
        }
        catch (DbException)
        {
            // DiemDanh is optional in databases that have not merged the lecturer module.
        }

        var lichHoc = await db.LichHoc.AsNoTracking().Where(x => lopIds.Contains(x.LopHocId)).ToListAsync();
        for (var i = 0; i < lichHoc.Count; i++)
        for (var j = i + 1; j < lichHoc.Count; j++)
        {
            var a = lichHoc[i];
            var b = lichHoc[j];
            var trungNgay = a.ThuTrongTuan == b.ThuTrongTuan && a.NgayBatDau.Date <= b.NgayKetThuc.Date
                && b.NgayBatDau.Date <= a.NgayKetThuc.Date;
            var trungGio = a.GioBatDau < b.GioKetThuc && b.GioBatDau < a.GioKetThuc;
            if (!trungNgay || !trungGio) continue;
            var lopA = dangKy.First(x => x.LopHocId == a.LopHocId);
            var lopB = dangKy.First(x => x.LopHocId == b.LopHocId);
            deXuat[(lopA.LopHocId, LoaiCanhBao.TrungLichHoc)] =
                ($"Lịch môn {lopA.TenMonHoc} ({lopA.MaLop}) trùng với {lopB.TenMonHoc} ({lopB.MaLop}).", MucDo.CanhBao);
            deXuat[(lopB.LopHocId, LoaiCanhBao.TrungLichHoc)] =
                ($"Lịch môn {lopB.TenMonHoc} ({lopB.MaLop}) trùng với {lopA.TenMonHoc} ({lopA.MaLop}).", MucDo.CanhBao);
        }

        var ganDay = DateTime.Now.AddDays(-7);
        var canhBaoCu = await db.CanhBaoSinhVien
            .Where(x => x.SinhVienId == sinhVienId && (!x.DaDoc || x.NgayTao >= ganDay)).ToListAsync();
        foreach (var item in deXuat)
        {
            if (canhBaoCu.Any(x => x.LopHocId == item.Key.LopHocId && x.LoaiCanhBao == item.Key.Loai)) continue;
            db.CanhBaoSinhVien.Add(new CanhBaoSinhVien
            {
                SinhVienId = sinhVienId, LopHocId = item.Key.LopHocId, LoaiCanhBao = item.Key.Loai,
                NoiDung = item.Value.NoiDung, MucDo = item.Value.MucDo
            });
        }
        await db.SaveChangesAsync();

        return await db.CanhBaoSinhVien.AsNoTracking().Include(x => x.LopHoc).ThenInclude(x => x!.MonHoc)
            .Where(x => x.SinhVienId == sinhVienId).OrderBy(x => x.DaDoc).ThenByDescending(x => x.NgayTao).ToListAsync();
    }
}
