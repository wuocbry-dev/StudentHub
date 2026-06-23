# AGENTS.md

Day la project SIMS - He thong quan ly sinh vien.

Khi Codex/AI lam viec trong project nay, phai uu tien doc cau truc hien co cua project truoc khi sua code.

Project su dung:
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Bootstrap 5
- JavaScript
- CSS

## Vi tri skill

File AGENTS.md nay dang nam trong thu muc `StudentHub`.

Thu muc skill nam o ngoai thu muc `StudentHub`, vi vay tat ca duong dan den skill phai dung dang:

- `../.ai-skills/sims-import-hocvuot/...`

## Bat buoc khi lam 3 module

Khi lam bat ky module nao trong 3 module duoi day, bat buoc doc file:

- `../.ai-skills/sims-import-hocvuot/SKILL.md`

3 module gom:

1. NhapDuLieuTuFile
2. GoiYMonHocVuot
3. GioiHanHocVuotToiDa5Mon

## Module Nhap du lieu tu file

Khi lam module nhap du lieu tu dong tu file Excel / TXT / DOCX, bat buoc doc:

- `../.ai-skills/sims-import-hocvuot/PROMPT_04_NhapDuLieuTuFile.md`
- `../.ai-skills/sims-import-hocvuot/CHECKLIST_IMPORT.md`

Yeu cau chinh cua module nay:

- Khong su dung AI trong logic ung dung.
- Chi doc file theo mau co dinh.
- Ho tro file `.xlsx`, `.txt`, `.docx`.
- File Excel phai co header dung mau.
- File TXT phai co header dung mau va dung dau `|` de ngan cach cot.
- File DOCX chi ho tro du lieu nam trong bang.
- Khong xu ly file Word hoac TXT dang van ban tu do.
- Khong import thang vao database sau khi upload.
- Bat buoc co buoc preview truoc khi luu database.
- Bat buoc validation tung dong.
- Bat buoc co lich su import.
- Bat buoc co tai file mau.
- Bat buoc co tai file loi neu co dong sai.
- Import `DangKyHoc` phai kiem tra gioi han hoc vuot toi da 5 mon.
- Import `BangDiem` phai tu tinh `DiemTongKet` va `DiemChu`.
- Import `SinhVien` phai tu tao `TaiKhoan` voi `VaiTro = SinhVien`.
- Import `GiangVien` phai tu tao `TaiKhoan` voi `VaiTro = GiangVien`.

Migration du kien:

```bash
dotnet ef migrations add AddNhapDuLieuTuFile
dotnet ef database update
```

Neu dung Package Manager Console:

```powershell
Add-Migration AddNhapDuLieuTuFile
Update-Database
```

## Module Goi y mon hoc vuot

Khi lam module goi y mon hoc vuot, bat buoc doc:

- `../.ai-skills/sims-import-hocvuot/PROMPT_05_GoiYMonHocVuot.md`
- `../.ai-skills/sims-import-hocvuot/CHECKLIST_GOI_Y_HOC_VUOT.md`

Yeu cau chinh cua module nay:

- Khong su dung AI trong logic ung dung.
- Goi y mon hoc vuot bang rule-based logic.
- Phai co bang `MonHocTienQuyet`.
- Phai co bang `GoiYHocVuot`.
- Phai kiem tra GPA.
- Phai kiem tra mon tien quyet.
- Phai kiem tra mon da hoc.
- Phai kiem tra mon da dang ky.
- Phai kiem tra lop con cho.
- Phai kiem tra trung lich.
- Khong goi y mon da hoc.
- Khong goi y mon da dang ky.
- Khong goi y neu thieu mon tien quyet bat buoc.
- Khong goi y lop bi trung lich.
- Khong goi y lop da day.
- SinhVien phai xem duoc danh sach goi y hoc vuot.
- Admin phai quan ly duoc mon hoc tien quyet.
- Admin phai xem duoc danh sach goi y hoc vuot.

Migration du kien:

```bash
dotnet ef migrations add AddGoiYMonHocVuot
dotnet ef database update
```

Neu dung Package Manager Console:

```powershell
Add-Migration AddGoiYMonHocVuot
Update-Database
```

## Module Khong qua 5 mon hoc vuot

Khi lam module gioi han khong qua 5 mon hoc vuot, bat buoc doc:

- `../.ai-skills/sims-import-hocvuot/PROMPT_06_GioiHanHocVuot.md`
- `../.ai-skills/sims-import-hocvuot/CHECKLIST_GIOI_HAN_HOC_VUOT.md`

Yeu cau chinh cua module nay:

- Khong su dung AI trong logic ung dung.
- Mot sinh vien khong duoc dang ky qua 5 mon hoc vuot trong cung `HocKy` va `NamHoc`.
- Bang `DangKyHoc` phai co cot `HocKy`.
- Bang `DangKyHoc` phai co cot `NamHoc`.
- Bang `DangKyHoc` phai co cot `LaHocVuot`.
- Khong tinh cac `DangKyHoc` co `TrangThai = DaHuy`.
- Khong tinh cac `DangKyHoc` co `TrangThai = TuChoi`.
- Neu da co 5 mon hoc vuot thi khong cho dang ky them.
- Logic gioi han hoc vuot phai nam trong `GioiHanHocVuotService`.
- Phai ap dung khi SinhVien dang ky hoc vuot.
- Phai ap dung khi Admin dang ky thay SinhVien.
- Phai ap dung khi SinhVien dang ky tu `GoiYHocVuot`.
- Phai ap dung khi import `DangKyHoc` tu file.
- Giao dien SinhVien phai hien thi so mon hoc vuot dang `0/5`, `1/5`, `5/5`.
- Neu da du `5/5` thi disable nut dang ky.

Migration du kien:

```bash
dotnet ef migrations add AddGioiHanHocVuot
dotnet ef database update
```

Neu dung Package Manager Console:

```powershell
Add-Migration AddGioiHanHocVuot
Update-Database
```

## Quy tac database

Database bat buoc dung tieng Viet khong dau cho:

- Ten bang
- Ten cot
- Enum
- Status
- Role neu luu trong database

Vi du ten dung:

- `TaiKhoan`
- `SinhVien`
- `GiangVien`
- `Khoa`
- `MonHoc`
- `PhongHoc`
- `LopHoc`
- `LichHoc`
- `DangKyHoc`
- `BangDiem`
- `DiemDanh`
- `PhienDiemDanh`
- `LichSuNhapDuLieu`
- `LoiNhapDuLieu`
- `DuLieuNhapTam`
- `GoiYHocVuot`
- `MonHocTienQuyet`
- `CauHinhHocVuot`

Vi du ten sai, khong duoc dung trong database:

- `Student`
- `Lecturer`
- `Course`
- `Class`
- `Schedule`
- `Enrollment`
- `Grade`
- `Attendance`
- `ImportHistory`
- `Recommendation`
- `Prerequisite`

## Quy tac service layer

- Cac chuc nang phai co service layer.
- Business rule phai dat trong service.
- Controller chi dieu phoi request, goi service va tra ve view/result.
- Khong nhet het logic nghiep vu vao controller.
- Validation quan trong phai chay o server-side.
- JavaScript chi dung de ho tro UX, khong duoc la noi duy nhat validate nghiep vu.

## Quy tac giao dien

Cac module co giao dien phai dung:

- Bootstrap 5
- JavaScript
- CSS
- SweetAlert2 neu can confirm/alert
- DataTables.js neu can bang du lieu co sap xep, tim kiem, phan trang
- Bootstrap Icons hoac Font Awesome neu can icon

File CSS/JS rieng goi y:

- `wwwroot/css/nhapdulieu.css`
- `wwwroot/js/nhapdulieu.js`
- `wwwroot/css/hocvuot.css`
- `wwwroot/js/hocvuot.js`

## Quy tac migration

Neu thay doi database thi bat buoc:

1. Tao hoac sua model.
2. Them `DbSet` vao `SimsDbContext` neu co bang moi.
3. Tao migration.
4. Nhac nguoi dung chay update database.

Lenh CLI:

```bash
dotnet ef migrations add TenMigration
dotnet ef database update
```

Lenh Package Manager Console:

```powershell
Add-Migration TenMigration
Update-Database
```

Khong duoc sua database truc tiep bang tay neu khong can thiet.

## Thu tu uu tien khi code

Khong nen lam ca 3 module cung luc neu khong can thiet.

Thu tu khuyen nghi:

1. `GioiHanHocVuotToiDa5Mon`
2. `GoiYMonHocVuot`
3. `NhapDuLieuTuFile`

Ly do:

- `NhapDuLieuTuFile` khi import `DangKyHoc` can goi lai logic gioi han hoc vuot.
- `GoiYHocVuot` khi sinh vien dang ky can kiem tra gioi han hoc vuot.
- `GioiHanHocVuotToiDa5Mon` la rule nen tang nen can lam truoc.

## Bat buoc truoc khi code

Truoc khi sua code, Codex phai:

1. Xac dinh module dang lam la:
   - `NhapDuLieuTuFile`
   - `GoiYMonHocVuot`
   - `GioiHanHocVuotToiDa5Mon`
2. Doc file:
   - `../.ai-skills/sims-import-hocvuot/SKILL.md`
3. Doc prompt cua module dang lam.
4. Doc checklist cua module dang lam.
5. Kiem tra cau truc code hien co:
   - `Controllers`
   - `Models`
   - `Views`
   - `Data`
   - `Migrations`
   - `wwwroot`
   - `Program.cs`
   - `appsettings.json`
6. Tom tat ngan gon ke hoach thuc hien.
7. Sau do moi bat dau code.

Khong duoc code ngay neu chua doc prompt va checklist tuong ung.

## Bat buoc sau khi code

Sau khi code xong, Codex phai bao cao:

1. Da tao/sua nhung file nao.
2. Da them model nao.
3. Da them service nao.
4. Da them controller nao.
5. Da them view nao.
6. Da them CSS/JS nao.
7. Da tao migration nao.
8. Can chay lenh database nao.
9. Checklist nao da hoan thanh.
10. Checklist nao chua hoan thanh.
11. Loi hoac phan nao can nguoi dung kiem tra lai.

Khong duoc bao hoan thanh neu chua doi chieu checklist.

## Khong duoc lam

- Khong su dung AI trong logic ung dung cua 3 module nay.
- Khong bo qua service layer.
- Khong chi validate bang JavaScript.
- Khong import du lieu thang vao database ngay sau khi upload file.
- Khong xu ly file Word/TXT dang van ban tu do.
- Khong bo qua migration khi co thay doi database.
- Khong bo qua checklist tuong ung.
- Khong bao hoan thanh neu con checklist bat buoc chua lam.
