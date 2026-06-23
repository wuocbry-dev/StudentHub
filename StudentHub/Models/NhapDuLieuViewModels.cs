using System.Text.Json;

namespace StudentHub.Models;

public class NhapDuLieuIndexViewModel
{
    public List<LoaiDuLieuOption> LoaiDuLieuOptions { get; set; } = [];
    public List<LoaiFileOption> LoaiFileOptions { get; set; } = [];
    public List<LichSuNhapDuLieu> LichSuGanDay { get; set; } = [];
}

public record LoaiDuLieuOption(LoaiDuLieuNhap Value, string Label);
public record LoaiFileOption(LoaiFileNhap Value, string Label, string Extension);

public class ImportPreviewViewModel
{
    public LichSuNhapDuLieu LichSu { get; set; } = new();
    public List<ImportRowPreviewViewModel> Rows { get; set; } = [];
    public int TyLeHopLe => LichSu.TongSoDong == 0 ? 0 : (int)Math.Round(LichSu.SoDongHopLe * 100m / LichSu.TongSoDong);
}

public class ImportRowPreviewViewModel
{
    public int SoDong { get; set; }
    public string NoiDungHienThi { get; set; } = "";
    public bool HopLe { get; set; }
    public string? NoiDungLoi { get; set; }

    public Dictionary<string, string> NoiDung => JsonSerializer.Deserialize<Dictionary<string, string>>(NoiDungHienThi) ?? [];
}

public class ImportResultViewModel
{
    public LichSuNhapDuLieu LichSu { get; set; } = new();
    public bool ThanhCong => LichSu.TrangThai == TrangThaiNhapDuLieu.DaNhap;
}

public class LichSuNhapDuLieuViewModel
{
    public List<LichSuNhapDuLieu> Items { get; set; } = [];
}
