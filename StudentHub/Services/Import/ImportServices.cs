using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.Services.HocVuot;

namespace StudentHub.Services.Import;

public record ImportRawRow(int SoDong, Dictionary<string, string> Values);
public record ImportReadResult(List<string> Headers, List<ImportRawRow> Rows);
public record ImportValidationRow(int SoDong, Dictionary<string, string> Values, bool HopLe, string? NoiDungLoi);
public record ImportPreviewResult(int LichSuNhapDuLieuId, int TongSoDong, int SoDongHopLe, int SoDongLoi);
public record ImportConfirmResult(int LichSuNhapDuLieuId, int SoDongDaNhap, int SoDongLoi);
public record FileDownloadResult(byte[] Content, string ContentType, string FileName);

public interface IFileImportService
{
    Task<ImportPreviewResult> PreviewAsync(IFormFile file, LoaiDuLieuNhap loaiDuLieu, int taiKhoanId);
    Task<ImportConfirmResult> ConfirmAsync(int lichSuNhapDuLieuId, int taiKhoanId, bool chiNhapDongHopLe = false);
    Task HuyAsync(int lichSuNhapDuLieuId, int taiKhoanId);
}

public class ExcelImportReader
{
    public async Task<ImportReadResult> ReadAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var range = worksheet.RangeUsed() ?? throw new InvalidOperationException("File Excel khong co du lieu.");
        var headerRow = range.FirstRowUsed();
        var headers = headerRow.Cells().Select(x => x.GetString().Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var rows = new List<ImportRawRow>();
        foreach (var row in range.RowsUsed().Skip(1))
        {
            var values = headers.Select((header, index) => new { header, value = row.Cell(index + 1).GetString().Trim() })
                .ToDictionary(x => x.header, x => x.value);
            if (values.Values.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(new ImportRawRow(row.RowNumber(), values));
        }
        return new ImportReadResult(headers, rows);
    }
}

public class TxtImportReader
{
    public async Task<ImportReadResult> ReadAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
        var lines = new List<string>();
        while (await reader.ReadLineAsync() is { } line) lines.Add(line);
        if (lines.Count == 0) throw new InvalidOperationException("File TXT khong co du lieu.");
        var headers = Split(lines[0]);
        var rows = new List<ImportRawRow>();
        for (var i = 1; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = Split(lines[i]);
            var values = headers.Select((header, index) => new { header, value = index < cells.Count ? cells[index] : "" })
                .ToDictionary(x => x.header, x => x.value);
            rows.Add(new ImportRawRow(i + 1, values));
        }
        return new ImportReadResult(headers, rows);
    }

    private static List<string> Split(string line) => line.Split('|').Select(x => x.Trim()).ToList();
}

public class DocxImportReader
{
    public Task<ImportReadResult> ReadAsync(IFormFile file)
    {
        using var document = WordprocessingDocument.Open(file.OpenReadStream(), false);
        var mainPart = document.MainDocumentPart
            ?? throw new InvalidOperationException("File DOCX khong co noi dung.");
        var wordDocument = mainPart.Document
            ?? throw new InvalidOperationException("File DOCX khong co noi dung.");
        var body = wordDocument.Body
            ?? throw new InvalidOperationException("File DOCX khong co noi dung.");
        var table = body.Elements<Table>().FirstOrDefault()
            ?? throw new InvalidOperationException("File DOCX phai co bang du lieu dau tien.");
        var tableRows = table.Elements<TableRow>().ToList();
        if (tableRows.Count == 0) throw new InvalidOperationException("Bang trong file DOCX khong co du lieu.");
        var headers = ReadCells(tableRows[0]);
        var rows = new List<ImportRawRow>();
        for (var i = 1; i < tableRows.Count; i++)
        {
            var cells = ReadCells(tableRows[i]);
            if (cells.All(string.IsNullOrWhiteSpace)) continue;
            var values = headers.Select((header, index) => new { header, value = index < cells.Count ? cells[index] : "" })
                .ToDictionary(x => x.header, x => x.value);
            rows.Add(new ImportRawRow(i + 1, values));
        }
        return Task.FromResult(new ImportReadResult(headers, rows));
    }

    private static List<string> ReadCells(TableRow row) => row.Elements<TableCell>()
        .Select(cell => string.Concat(cell.Descendants<Text>().Select(t => t.Text)).Trim())
        .ToList();
}

public class ImportValidationService(SimsDbContext db, IGioiHanHocVuotService gioiHanHocVuotService)
{
    private static readonly Dictionary<LoaiDuLieuNhap, string[]> Headers = new()
    {
        [LoaiDuLieuNhap.SinhVien] = ["MaSinhVien", "HoTen", "Email", "SoDienThoai", "NgaySinh", "GioiTinh", "DiaChi", "MaKhoa", "TrangThai"],
        [LoaiDuLieuNhap.GiangVien] = ["MaGiangVien", "HoTen", "Email", "SoDienThoai", "MaKhoa", "HocVi"],
        [LoaiDuLieuNhap.MonHoc] = ["MaMonHoc", "TenMonHoc", "SoTinChi", "MaKhoa", "MoTa"],
        [LoaiDuLieuNhap.LopHoc] = ["MaLop", "TenLop", "MaMonHoc", "MaGiangVien", "HocKy", "NamHoc", "SoLuongToiDa", "TrangThai"],
        [LoaiDuLieuNhap.DangKyHoc] = ["MaSinhVien", "MaLop", "HocKy", "NamHoc", "LaHocVuot", "TrangThai"],
        [LoaiDuLieuNhap.BangDiem] = ["MaSinhVien", "MaLop", "DiemChuyenCan", "DiemBaiTap", "DiemGiuaKy", "DiemCuoiKy"],
        [LoaiDuLieuNhap.DiemDanh] = ["MaSinhVien", "MaLop", "NgayDiemDanh", "TrangThai", "GhiChu"]
    };

    public IReadOnlyList<string> GetHeaders(LoaiDuLieuNhap loaiDuLieu) => Headers[loaiDuLieu];

    public async Task<List<ImportValidationRow>> ValidateAsync(LoaiDuLieuNhap loaiDuLieu, ImportReadResult readResult)
    {
        var expected = Headers[loaiDuLieu];
        if (!expected.SequenceEqual(readResult.Headers, StringComparer.OrdinalIgnoreCase))
        {
            var message = $"Header khong dung mau. Can: {string.Join("|", expected)}";
            return readResult.Rows.Select(x => new ImportValidationRow(x.SoDong, x.Values, false, message)).ToList();
        }

        return loaiDuLieu switch
        {
            LoaiDuLieuNhap.SinhVien => await ValidateSinhVienAsync(readResult.Rows),
            LoaiDuLieuNhap.DangKyHoc => await ValidateDangKyHocAsync(readResult.Rows),
            LoaiDuLieuNhap.BangDiem => await ValidateBangDiemAsync(readResult.Rows),
            _ => readResult.Rows.Select(x => new ImportValidationRow(x.SoDong, x.Values, false, "Loai du lieu nay chua duoc ho tro trong MVP.")).ToList()
        };
    }

    private async Task<List<ImportValidationRow>> ValidateSinhVienAsync(List<ImportRawRow> rows)
    {
        var result = new List<ImportValidationRow>();
        var maTrongFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        var maSinhVienDb = await db.SinhVien.AsNoTracking().Select(x => x.MaSinhVien).ToListAsync();
        var emailDb = await db.SinhVien.AsNoTracking().Select(x => x.Email).ToListAsync();
        var maKhoaDb = await db.Khoa.AsNoTracking().Select(x => x.MaKhoa).ToListAsync();

        foreach (var row in rows)
        {
            var errors = new List<string>();
            var ma = Value(row, "MaSinhVien").ToUpperInvariant();
            var email = Value(row, "Email");
            var maKhoa = Value(row, "MaKhoa").ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(ma)) errors.Add("MaSinhVien khong duoc trong.");
            if (string.IsNullOrWhiteSpace(Value(row, "HoTen"))) errors.Add("HoTen khong duoc trong.");
            if (string.IsNullOrWhiteSpace(email)) errors.Add("Email khong duoc trong.");
            else if (!emailRegex.IsMatch(email)) errors.Add("Email khong dung dinh dang.");
            if (!string.IsNullOrWhiteSpace(ma) && !maTrongFile.Add(ma)) errors.Add("MaSinhVien bi trung trong file.");
            if (maSinhVienDb.Contains(ma, StringComparer.OrdinalIgnoreCase)) errors.Add("MaSinhVien da ton tai trong database.");
            if (emailDb.Contains(email, StringComparer.OrdinalIgnoreCase)) errors.Add("Email da ton tai trong database.");
            if (!maKhoaDb.Contains(maKhoa, StringComparer.OrdinalIgnoreCase)) errors.Add("MaKhoa khong ton tai.");
            if (!DateTime.TryParseExact(Value(row, "NgaySinh"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                errors.Add("NgaySinh phai dung dinh dang yyyy-MM-dd.");
            if (!Enum.TryParse<GioiTinh>(Value(row, "GioiTinh"), true, out _)) errors.Add("GioiTinh chi nhan Nam, Nu, Khac.");
            if (!Enum.TryParse<TrangThaiSinhVien>(Value(row, "TrangThai"), true, out _)) errors.Add("TrangThai chi nhan DangHoc, BaoLuu, DaTotNghiep, ThoiHoc.");

            result.Add(new ImportValidationRow(row.SoDong, row.Values, errors.Count == 0, string.Join(" ", errors)));
        }

        return result;
    }

    private async Task<List<ImportValidationRow>> ValidateDangKyHocAsync(List<ImportRawRow> rows)
    {
        var result = new List<ImportValidationRow>();
        var sinhViens = await db.SinhVien.AsNoTracking().ToDictionaryAsync(x => x.MaSinhVien);
        var lopHocs = await db.LopHoc.AsNoTracking().ToDictionaryAsync(x => x.MaLop);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hocVuotHopLeTrongFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var errors = new List<string>();
            var maSinhVien = Value(row, "MaSinhVien").ToUpperInvariant();
            var maLop = Value(row, "MaLop").ToUpperInvariant();
            var hocKy = Value(row, "HocKy");
            var namHoc = Value(row, "NamHoc");
            var key = $"{maSinhVien}|{maLop}";

            if (!sinhViens.TryGetValue(maSinhVien, out var sinhVien)) errors.Add("MaSinhVien khong ton tai.");
            if (!lopHocs.TryGetValue(maLop, out var lopHoc)) errors.Add("MaLop khong ton tai.");
            if (!seen.Add(key)) errors.Add("Dang ky bi trung trong file.");
            if (!Enum.TryParse<TrangThaiDangKy>(Value(row, "TrangThai"), true, out _)) errors.Add("TrangThai khong hop le.");
            if (!bool.TryParse(Value(row, "LaHocVuot"), out var laHocVuot)) errors.Add("LaHocVuot phai la true hoac false.");

            if (sinhVien != null && lopHoc != null)
            {
                if (await db.DangKyHoc.AsNoTracking().AnyAsync(x => x.SinhVienId == sinhVien.Id && x.LopHocId == lopHoc.Id))
                    errors.Add("Sinh vien da dang ky lop nay.");
                var soLuong = await db.DangKyHoc.AsNoTracking().CountAsync(x => x.LopHocId == lopHoc.Id && x.TrangThai != TrangThaiDangKy.DaHuy && x.TrangThai != TrangThaiDangKy.TuChoi);
                if (soLuong >= lopHoc.SoLuongToiDa) errors.Add("Lop hoc da du so luong.");
                if (laHocVuot)
                {
                    var coTheTheoDatabase = await gioiHanHocVuotService.CoTheDangKyHocVuotAsync(sinhVien.Id, hocKy, namHoc);
                    var soDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVien.Id, hocKy, namHoc);
                    var soToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(hocKy, namHoc);
                    var hocVuotKey = $"{sinhVien.Id}|{hocKy}|{namHoc}";
                    var soHopLeTrongFile = hocVuotHopLeTrongFile.GetValueOrDefault(hocVuotKey);
                    if (!coTheTheoDatabase || soDaDangKy + soHopLeTrongFile >= soToiDa)
                    {
                        errors.Add($"Sinh viên {maSinhVien} đã đạt giới hạn {soToiDa} môn học vượt trong học kỳ {hocKy} năm học {namHoc}.");
                    }
                }
            }

            result.Add(new ImportValidationRow(row.SoDong, row.Values, errors.Count == 0, string.Join(" ", errors)));
            if (errors.Count == 0 && sinhVien != null && laHocVuot)
            {
                var hocVuotKey = $"{sinhVien.Id}|{hocKy}|{namHoc}";
                hocVuotHopLeTrongFile[hocVuotKey] = hocVuotHopLeTrongFile.GetValueOrDefault(hocVuotKey) + 1;
            }
        }
        return result;
    }

    private async Task<List<ImportValidationRow>> ValidateBangDiemAsync(List<ImportRawRow> rows)
    {
        var result = new List<ImportValidationRow>();
        var sinhViens = await db.SinhVien.AsNoTracking().ToDictionaryAsync(x => x.MaSinhVien);
        var lopHocs = await db.LopHoc.AsNoTracking().ToDictionaryAsync(x => x.MaLop);
        foreach (var row in rows)
        {
            var errors = new List<string>();
            var maSinhVien = Value(row, "MaSinhVien").ToUpperInvariant();
            var maLop = Value(row, "MaLop").ToUpperInvariant();
            if (!sinhViens.TryGetValue(maSinhVien, out var sinhVien)) errors.Add("MaSinhVien khong ton tai.");
            if (!lopHocs.TryGetValue(maLop, out var lopHoc)) errors.Add("MaLop khong ton tai.");
            if (sinhVien != null && lopHoc != null && !await db.DangKyHoc.AsNoTracking().AnyAsync(x => x.SinhVienId == sinhVien.Id && x.LopHocId == lopHoc.Id))
                errors.Add("Sinh vien chua dang ky lop nay.");
            foreach (var field in new[] { "DiemChuyenCan", "DiemBaiTap", "DiemGiuaKy", "DiemCuoiKy" })
            {
                if (!decimal.TryParse(Value(row, field), NumberStyles.Number, CultureInfo.InvariantCulture, out var diem) || diem < 0 || diem > 10)
                    errors.Add($"{field} phai tu 0 den 10.");
            }
            result.Add(new ImportValidationRow(row.SoDong, row.Values, errors.Count == 0, string.Join(" ", errors)));
        }
        return result;
    }

    private static string Value(ImportRawRow row, string key) => row.Values.TryGetValue(key, out var value) ? value.Trim() : "";
}

public class ImportSaveService(SimsDbContext db, IGioiHanHocVuotService gioiHanHocVuotService)
{
    public async Task<int> SaveAsync(LichSuNhapDuLieu lichSu, List<DuLieuNhapTam> rows)
    {
        return lichSu.LoaiDuLieu switch
        {
            LoaiDuLieuNhap.SinhVien => await SaveSinhVienAsync(rows),
            LoaiDuLieuNhap.DangKyHoc => await SaveDangKyHocAsync(rows),
            LoaiDuLieuNhap.BangDiem => await SaveBangDiemAsync(rows),
            _ => throw new InvalidOperationException("Loai du lieu nay chua duoc ho tro luu trong MVP.")
        };
    }

    private async Task<int> SaveSinhVienAsync(List<DuLieuNhapTam> rows)
    {
        var imported = 0;
        foreach (var row in rows)
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(row.NoiDungJson) ?? [];
            var maKhoa = values["MaKhoa"].Trim().ToUpperInvariant();
            var khoa = await db.Khoa.SingleAsync(x => x.MaKhoa == maKhoa);
            var taiKhoan = await db.TaiKhoan.SingleOrDefaultAsync(x => x.TenDangNhap == values["MaSinhVien"].Trim().ToUpper());
            if (taiKhoan == null)
            {
                taiKhoan = new TaiKhoan
                {
                    TenDangNhap = values["MaSinhVien"].Trim().ToUpperInvariant(),
                    HoTen = values["HoTen"].Trim(),
                    Email = values["Email"].Trim(),
                    VaiTro = VaiTro.SinhVien,
                    TrangThai = TrangThaiTaiKhoan.HoatDong
                };
                taiKhoan.MatKhauHash = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, "123456");
                db.TaiKhoan.Add(taiKhoan);
                await db.SaveChangesAsync();
            }

            db.SinhVien.Add(new SinhVien
            {
                MaSinhVien = values["MaSinhVien"].Trim().ToUpperInvariant(),
                HoTen = values["HoTen"].Trim(),
                Email = values["Email"].Trim(),
                SoDienThoai = EmptyToNull(values["SoDienThoai"]),
                NgaySinh = DateTime.ParseExact(values["NgaySinh"], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                GioiTinh = Enum.Parse<GioiTinh>(values["GioiTinh"], true),
                DiaChi = EmptyToNull(values["DiaChi"]),
                KhoaId = khoa.Id,
                TaiKhoanId = taiKhoan.Id,
                TrangThai = Enum.Parse<TrangThaiSinhVien>(values["TrangThai"], true)
            });
            imported++;
        }
        await db.SaveChangesAsync();
        return imported;
    }

    private async Task<int> SaveDangKyHocAsync(List<DuLieuNhapTam> rows)
    {
        var imported = 0;
        var hocVuotHopLeTrongLanLuu = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(row.NoiDungJson) ?? [];
            var sinhVien = await db.SinhVien.SingleAsync(x => x.MaSinhVien == values["MaSinhVien"].Trim().ToUpper());
            var lopHoc = await db.LopHoc.SingleAsync(x => x.MaLop == values["MaLop"].Trim().ToUpper());
            var laHocVuot = bool.TryParse(values["LaHocVuot"], out var hv) && hv;
            var hocKy = values["HocKy"].Trim();
            var namHoc = values["NamHoc"].Trim();
            if (laHocVuot)
            {
                var coTheTheoDatabase = await gioiHanHocVuotService.CoTheDangKyHocVuotAsync(sinhVien.Id, hocKy, namHoc);
                var soDaDangKy = await gioiHanHocVuotService.LaySoMonHocVuotDaDangKyAsync(sinhVien.Id, hocKy, namHoc);
                var soToiDa = await gioiHanHocVuotService.LaySoMonHocVuotToiDaAsync(hocKy, namHoc);
                var hocVuotKey = $"{sinhVien.Id}|{hocKy}|{namHoc}";
                var soHopLeTrongLanLuu = hocVuotHopLeTrongLanLuu.GetValueOrDefault(hocVuotKey);
                if (!coTheTheoDatabase || soDaDangKy + soHopLeTrongLanLuu >= soToiDa)
                    throw new InvalidOperationException($"Sinh viên {sinhVien.MaSinhVien} đã đạt giới hạn {soToiDa} môn học vượt trong học kỳ {hocKy} năm học {namHoc}.");
            }

            db.DangKyHoc.Add(new DangKyHoc
            {
                SinhVienId = sinhVien.Id,
                LopHocId = lopHoc.Id,
                HocKy = hocKy,
                NamHoc = namHoc,
                LaHocVuot = laHocVuot,
                TrangThai = Enum.Parse<TrangThaiDangKy>(values["TrangThai"], true)
            });
            if (laHocVuot)
            {
                var hocVuotKey = $"{sinhVien.Id}|{hocKy}|{namHoc}";
                hocVuotHopLeTrongLanLuu[hocVuotKey] = hocVuotHopLeTrongLanLuu.GetValueOrDefault(hocVuotKey) + 1;
            }
            imported++;
        }
        await db.SaveChangesAsync();
        return imported;
    }

    private async Task<int> SaveBangDiemAsync(List<DuLieuNhapTam> rows)
    {
        var imported = 0;
        foreach (var row in rows)
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(row.NoiDungJson) ?? [];
            var sinhVien = await db.SinhVien.SingleAsync(x => x.MaSinhVien == values["MaSinhVien"].Trim().ToUpper());
            var lopHoc = await db.LopHoc.SingleAsync(x => x.MaLop == values["MaLop"].Trim().ToUpper());
            var diemChuyenCan = ParseDiem(values["DiemChuyenCan"]);
            var diemBaiTap = ParseDiem(values["DiemBaiTap"]);
            var diemGiuaKy = ParseDiem(values["DiemGiuaKy"]);
            var diemCuoiKy = ParseDiem(values["DiemCuoiKy"]);
            var tongKet = Math.Round(diemChuyenCan * 0.1m + diemBaiTap * 0.2m + diemGiuaKy * 0.3m + diemCuoiKy * 0.4m, 2);
            var bangDiem = await db.BangDiem.SingleOrDefaultAsync(x => x.SinhVienId == sinhVien.Id && x.LopHocId == lopHoc.Id);
            if (bangDiem == null)
            {
                bangDiem = new BangDiem { SinhVienId = sinhVien.Id, LopHocId = lopHoc.Id };
                db.BangDiem.Add(bangDiem);
            }
            bangDiem.DiemChuyenCan = diemChuyenCan;
            bangDiem.DiemBaiTap = diemBaiTap;
            bangDiem.DiemGiuaKy = diemGiuaKy;
            bangDiem.DiemCuoiKy = diemCuoiKy;
            bangDiem.DiemTongKet = tongKet;
            bangDiem.DiemChu = tongKet >= 8.5m ? "A" : tongKet >= 7m ? "B" : tongKet >= 5.5m ? "C" : tongKet >= 4m ? "D" : "F";
            bangDiem.NgayCapNhat = DateTime.Now;
            imported++;
        }
        await db.SaveChangesAsync();
        return imported;
    }

    private static decimal ParseDiem(string value) => decimal.Parse(value, NumberStyles.Number, CultureInfo.InvariantCulture);

    private static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public class FileMauService(ImportValidationService validation)
{
    public FileDownloadResult Create(LoaiDuLieuNhap loaiDuLieu, LoaiFileNhap loaiFile)
    {
        var headers = validation.GetHeaders(loaiDuLieu).ToList();
        return loaiFile switch
        {
            LoaiFileNhap.Xlsx => CreateXlsx(loaiDuLieu, headers),
            LoaiFileNhap.Txt => CreateTxt(loaiDuLieu, headers),
            LoaiFileNhap.Docx => CreateDocx(loaiDuLieu, headers),
            _ => throw new InvalidOperationException("Loai file khong hop le.")
        };
    }

    private static FileDownloadResult CreateXlsx(LoaiDuLieuNhap loaiDuLieu, List<string> headers)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(loaiDuLieu.ToString());
        for (var i = 0; i < headers.Count; i++) sheet.Cell(1, i + 1).Value = headers[i];
        sheet.Row(1).Style.Font.Bold = true;
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new FileDownloadResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"mau-{loaiDuLieu}.xlsx");
    }

    private static FileDownloadResult CreateTxt(LoaiDuLieuNhap loaiDuLieu, List<string> headers)
    {
        var bytes = Encoding.UTF8.GetBytes(string.Join("|", headers) + Environment.NewLine);
        return new FileDownloadResult(bytes, "text/plain; charset=utf-8", $"mau-{loaiDuLieu}.txt");
    }

    private static FileDownloadResult CreateDocx(LoaiDuLieuNhap loaiDuLieu, List<string> headers)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var main = document.AddMainDocumentPart();
            main.Document = new Document(new Body());
            var table = new Table();
            table.Append(new TableRow(headers.Select(h => new TableCell(new Paragraph(new Run(new Text(h)))))));
            main.Document.Body!.Append(table);
            main.Document.Save();
        }
        return new FileDownloadResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"mau-{loaiDuLieu}.docx");
    }
}

public class FileLoiService(SimsDbContext db)
{
    public async Task<FileDownloadResult> CreateAsync(int lichSuNhapDuLieuId)
    {
        var errors = await db.LoiNhapDuLieu.AsNoTracking()
            .Where(x => x.LichSuNhapDuLieuId == lichSuNhapDuLieuId)
            .OrderBy(x => x.SoDong)
            .ToListAsync();
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Loi");
        sheet.Cell(1, 1).Value = "SoDong";
        sheet.Cell(1, 2).Value = "NoiDungDong";
        sheet.Cell(1, 3).Value = "NoiDungLoi";
        for (var i = 0; i < errors.Count; i++)
        {
            sheet.Cell(i + 2, 1).Value = errors[i].SoDong;
            sheet.Cell(i + 2, 2).Value = errors[i].NoiDungDong;
            sheet.Cell(i + 2, 3).Value = errors[i].NoiDungLoi;
        }
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new FileDownloadResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"loi-import-{lichSuNhapDuLieuId}.xlsx");
    }
}

public class FileImportService(
    SimsDbContext db,
    ExcelImportReader excelReader,
    TxtImportReader txtReader,
    DocxImportReader docxReader,
    ImportValidationService validation,
    ImportSaveService saveService) : IFileImportService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<ImportPreviewResult> PreviewAsync(IFormFile file, LoaiDuLieuNhap loaiDuLieu, int taiKhoanId)
    {
        ValidateUpload(file);
        var loaiFile = ParseFileType(file.FileName);
        var readResult = loaiFile switch
        {
            LoaiFileNhap.Xlsx => await excelReader.ReadAsync(file),
            LoaiFileNhap.Txt => await txtReader.ReadAsync(file),
            LoaiFileNhap.Docx => await docxReader.ReadAsync(file),
            _ => throw new InvalidOperationException("Loai file khong hop le.")
        };
        var rows = await validation.ValidateAsync(loaiDuLieu, readResult);
        var lichSu = new LichSuNhapDuLieu
        {
            TaiKhoanId = taiKhoanId,
            TenFile = Path.GetFileName(file.FileName),
            LoaiFile = loaiFile,
            LoaiDuLieu = loaiDuLieu,
            TongSoDong = rows.Count,
            SoDongHopLe = rows.Count(x => x.HopLe),
            SoDongLoi = rows.Count(x => !x.HopLe),
            TrangThai = TrangThaiNhapDuLieu.ChoXacNhan,
            GhiChu = "Da kiem tra du lieu, cho xac nhan import."
        };
        db.LichSuNhapDuLieu.Add(lichSu);
        await db.SaveChangesAsync();

        foreach (var row in rows)
        {
            var json = JsonSerializer.Serialize(row.Values);
            db.DuLieuNhapTam.Add(new DuLieuNhapTam
            {
                LichSuNhapDuLieuId = lichSu.Id,
                SoDong = row.SoDong,
                LoaiDuLieu = loaiDuLieu,
                NoiDungJson = json,
                HopLe = row.HopLe,
                NoiDungLoi = row.NoiDungLoi
            });
            if (!row.HopLe)
            {
                db.LoiNhapDuLieu.Add(new LoiNhapDuLieu
                {
                    LichSuNhapDuLieuId = lichSu.Id,
                    SoDong = row.SoDong,
                    NoiDungDong = json,
                    NoiDungLoi = row.NoiDungLoi ?? "Du lieu khong hop le."
                });
            }
        }
        await db.SaveChangesAsync();
        return new ImportPreviewResult(lichSu.Id, lichSu.TongSoDong, lichSu.SoDongHopLe, lichSu.SoDongLoi);
    }

    public async Task<ImportConfirmResult> ConfirmAsync(int lichSuNhapDuLieuId, int taiKhoanId, bool chiNhapDongHopLe = false)
    {
        var lichSu = await db.LichSuNhapDuLieu.SingleAsync(x => x.Id == lichSuNhapDuLieuId && x.TaiKhoanId == taiKhoanId);
        if (lichSu.TrangThai != TrangThaiNhapDuLieu.ChoXacNhan) throw new InvalidOperationException("Lan import khong o trang thai cho xac nhan.");
        if (lichSu.SoDongLoi > 0 && !chiNhapDongHopLe) throw new InvalidOperationException("Con dong loi, chi co the nhap dong hop le hoac tai file loi de sua.");
        var rows = await db.DuLieuNhapTam.Where(x => x.LichSuNhapDuLieuId == lichSuNhapDuLieuId && x.HopLe).OrderBy(x => x.SoDong).ToListAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var imported = await saveService.SaveAsync(lichSu, rows);
            lichSu.SoDongDaNhap = imported;
            lichSu.TrangThai = TrangThaiNhapDuLieu.DaNhap;
            lichSu.GhiChu = $"Da import thanh cong {imported} dong.";
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ImportConfirmResult(lichSu.Id, imported, lichSu.SoDongLoi);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            lichSu.TrangThai = TrangThaiNhapDuLieu.ThatBai;
            lichSu.GhiChu = ex.Message;
            await db.SaveChangesAsync();
            throw;
        }
    }

    public async Task HuyAsync(int lichSuNhapDuLieuId, int taiKhoanId)
    {
        var lichSu = await db.LichSuNhapDuLieu.SingleAsync(x => x.Id == lichSuNhapDuLieuId && x.TaiKhoanId == taiKhoanId);
        lichSu.TrangThai = TrangThaiNhapDuLieu.DaHuy;
        lichSu.GhiChu = "Nguoi dung da huy import.";
        await db.SaveChangesAsync();
    }

    private static void ValidateUpload(IFormFile file)
    {
        if (file == null) throw new InvalidOperationException("Vui long chon file.");
        if (file.Length == 0) throw new InvalidOperationException("File rong.");
        if (file.Length > MaxFileSize) throw new InvalidOperationException("File vuot qua dung luong toi da 10MB.");
        _ = ParseFileType(file.FileName);
    }

    private static LoaiFileNhap ParseFileType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".xlsx" => LoaiFileNhap.Xlsx,
            ".txt" => LoaiFileNhap.Txt,
            ".docx" => LoaiFileNhap.Docx,
            _ => throw new InvalidOperationException("Chi ho tro file .xlsx, .txt, .docx.")
        };
    }
}
