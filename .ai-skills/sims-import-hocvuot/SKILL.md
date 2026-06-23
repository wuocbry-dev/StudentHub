---
name: sims-import-hocvuot
description: Huong dan Codex/AI lam dung cac module SIMS ve nhap du lieu tu file Excel/TXT/DOCX, goi y mon hoc vuot, va gioi han toi da 5 mon hoc vuot trong project ASP.NET Core MVC. Use when working on SIMS import data, hoc vuot recommendation, hoc vuot limit, related instructions, prompts, checklists, EF Core models, services, controllers, views, validation, Bootstrap UI, migrations, or database naming rules for these modules.
---

# SIMS Import Va Hoc Vuot Skill

## Muc tieu

Skill nay dung de huong dan Codex/AI xay dung dung 3 module:

1. Nhap du lieu tu dong tu file Excel / TXT / DOCX
2. Goi y mon hoc vuot
3. Gioi han khong qua 5 mon hoc vuot

Project su dung:
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Bootstrap 5
- JavaScript
- CSS

Khong su dung AI trong logic cua 3 module nay.

## Quy tac database

Database phai dung tieng Viet khong dau cho:
- Ten bang
- Ten cot
- Enum
- Status
- Role neu luu trong database

Vi du dung:
- TaiKhoan
- SinhVien
- GiangVien
- Khoa
- MonHoc
- LopHoc
- LichHoc
- DangKyHoc
- BangDiem
- DiemDanh
- LichSuNhapDuLieu
- GoiYHocVuot
- MonHocTienQuyet
- CauHinhHocVuot

Vi du sai:
- Student
- Lecturer
- Course
- Enrollment
- Attendance
- Grade

## Quy tac giao dien

Tat ca chuc nang phai co giao dien bang:
- Bootstrap 5
- JavaScript
- CSS rieng neu can

Cac file CSS/JS goi y:
- wwwroot/css/nhapdulieu.css
- wwwroot/js/nhapdulieu.js
- wwwroot/css/hocvuot.css
- wwwroot/js/hocvuot.js

Co the dung them:
- SweetAlert2
- DataTables.js
- Bootstrap Icons hoac Font Awesome
- Chart.js neu can thong ke

## Quy tac service layer

- Moi module phai co service layer.
- Business rule phai dat trong service.
- Controller chi dieu phoi request, goi service va tra view/result.
- Validation quan trong phai chay o server-side.

## Quy tac migration

Neu tao bang moi hoac them cot moi thi bat buoc co migration.

Cac migration du kien:

```bash
dotnet ef migrations add AddNhapDuLieuTuFile
dotnet ef database update

dotnet ef migrations add AddGoiYMonHocVuot
dotnet ef database update

dotnet ef migrations add AddGioiHanHocVuot
dotnet ef database update
```

## Module 1: Nhap du lieu tu file

Doc chi tiet trong:

- `PROMPT_04_NhapDuLieuTuFile.md`
- `CHECKLIST_IMPORT.md`

Bat buoc co:

- Upload file
- Doc file `.xlsx`, `.txt`, `.docx`
- Khong xu ly file tu do
- File phai theo mau co dinh
- Co tai file mau
- Co preview truoc khi luu
- Co kiem tra loi tung dong
- Co lich su import
- Co tai file loi
- Co xac nhan roi moi luu database

Khong duoc:

- Import thang vao database ngay khi upload
- Su dung AI de suy doan du lieu
- Xu ly file Word dang doan van tu do

## Module 2: Goi y mon hoc vuot

Doc chi tiet trong:

- `PROMPT_05_GoiYMonHocVuot.md`
- `CHECKLIST_GOI_Y_HOC_VUOT.md`

Bat buoc co:

- Bang MonHocTienQuyet
- Bang GoiYHocVuot
- Admin quan ly mon tien quyet
- Sinh vien xem goi y hoc vuot
- Logic dua tren GPA, mon tien quyet, lich hoc, lop con cho
- Khong goi y mon da hoc
- Khong goi y mon da dang ky
- Khong goi y lop bi trung lich
- Khong su dung AI

## Module 3: Khong qua 5 mon hoc vuot

Doc chi tiet trong:

- `PROMPT_06_GioiHanHocVuot.md`
- `CHECKLIST_GIOI_HAN_HOC_VUOT.md`

Bat buoc co:

- DangKyHoc co HocKy, NamHoc, LaHocVuot
- Service kiem tra gioi han hoc vuot
- Khong cho sinh vien dang ky qua 5 mon hoc vuot trong cung HocKy va NamHoc
- Ap dung trong dang ky hoc
- Ap dung trong goi y hoc vuot
- Ap dung trong import DangKyHoc tu file
- Co giao dien hien thi so mon hoc vuot dang 3/5, 4/5, 5/5

## Thu tu lam khuyen nghi

1. GioiHanHocVuotToiDa5Mon
2. GoiYMonHocVuot
3. NhapDuLieuTuFile

## Checklist truoc khi hoan thanh

Truoc khi bao hoan thanh, bat buoc kiem tra:

- CHECKLIST_IMPORT.md
- CHECKLIST_GOI_Y_HOC_VUOT.md
- CHECKLIST_GIOI_HAN_HOC_VUOT.md

Khong duoc bao hoan thanh neu con checklist bat buoc chua lam.
