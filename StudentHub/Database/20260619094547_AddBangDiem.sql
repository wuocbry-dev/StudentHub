BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619094547_AddGiangVienDiemDanhVaBangDiem'
)
BEGIN
    CREATE TABLE [BangDiem] (
        [Id] int NOT NULL IDENTITY,
        [SinhVienId] int NOT NULL,
        [LopHocId] int NOT NULL,
        [DiemChuyenCan] decimal(4,2) NULL,
        [DiemBaiTap] decimal(4,2) NULL,
        [DiemGiuaKy] decimal(4,2) NULL,
        [DiemCuoiKy] decimal(4,2) NULL,
        [DiemTongKet] decimal(4,2) NULL,
        [DiemChu] nvarchar(1) NULL,
        [NgayCapNhat] datetime2 NOT NULL,
        CONSTRAINT [PK_BangDiem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BangDiem_LopHoc_LopHocId] FOREIGN KEY ([LopHocId]) REFERENCES [LopHoc] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BangDiem_SinhVien_SinhVienId] FOREIGN KEY ([SinhVienId]) REFERENCES [SinhVien] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619094547_AddGiangVienDiemDanhVaBangDiem'
)
BEGIN
    CREATE INDEX [IX_BangDiem_LopHocId] ON [BangDiem] ([LopHocId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619094547_AddGiangVienDiemDanhVaBangDiem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BangDiem_SinhVienId_LopHocId] ON [BangDiem] ([SinhVienId], [LopHocId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619094547_AddGiangVienDiemDanhVaBangDiem'
)
BEGIN
    INSERT INTO BangDiem (SinhVienId, LopHocId, DiemChuyenCan, DiemGiuaKy, DiemCuoiKy, DiemTongKet, NgayCapNhat)
    SELECT dk.SinhVienId, dk.LopHocId, d.DiemChuyenCan, d.DiemGiuaKy, d.DiemCuoiKy, d.DiemTongKet, d.NgayCapNhat
    FROM Diem d
    INNER JOIN DangKyHoc dk ON dk.Id = d.DangKyHocId;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619094547_AddGiangVienDiemDanhVaBangDiem'
)
BEGIN
    DROP TABLE [Diem];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619094547_AddGiangVienDiemDanhVaBangDiem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619094547_AddGiangVienDiemDanhVaBangDiem', N'8.0.3');
END;
GO

COMMIT;
GO

