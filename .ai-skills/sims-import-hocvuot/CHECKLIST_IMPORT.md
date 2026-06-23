# Checklist: NhapDuLieuTuFile

## Database
- [ ] Da tao bang LichSuNhapDuLieu
- [ ] Da tao bang LoiNhapDuLieu
- [ ] Da tao bang DuLieuNhapTam
- [ ] Da them DbSet trong SimsDbContext
- [ ] Da tao migration AddNhapDuLieuTuFile
- [ ] Da chay Update-Database

## Package
- [ ] Da cai ClosedXML
- [ ] Da cai DocumentFormat.OpenXml

## Service
- [ ] Da tao IFileImportService
- [ ] Da tao ExcelImportReader
- [ ] Da tao TxtImportReader
- [ ] Da tao DocxImportReader
- [ ] Da tao ImportValidationService
- [ ] Da tao ImportSaveService
- [ ] Da tao FileMauService
- [ ] Da tao FileLoiService

## Controller
- [ ] Da tao Admin/NhapDuLieuController
- [ ] Co action Index
- [ ] Co action KiemTraDuLieu
- [ ] Co action Preview
- [ ] Co action XacNhanNhap
- [ ] Co action HuyNhap
- [ ] Co action KetQua
- [ ] Co action LichSu
- [ ] Co action ChiTiet
- [ ] Co action TaiFileMau
- [ ] Co action TaiFileLoi

## View
- [ ] Da tao Index.cshtml
- [ ] Da tao Preview.cshtml
- [ ] Da tao KetQua.cshtml
- [ ] Da tao LichSu.cshtml
- [ ] Da tao ChiTiet.cshtml

## UI
- [ ] Co form upload file
- [ ] Co dropdown loai du lieu
- [ ] Co dropdown loai file mau
- [ ] Co nut tai file mau
- [ ] Co nut kiem tra du lieu
- [ ] Co bang preview
- [ ] Co badge HopLe/Loi
- [ ] Co nut xac nhan import
- [ ] Co nut huy import
- [ ] Co nut tai file loi
- [ ] Co file wwwroot/css/nhapdulieu.css
- [ ] Co file wwwroot/js/nhapdulieu.js

## Validation
- [ ] Kiem tra file null
- [ ] Kiem tra file rong
- [ ] Kiem tra dung luong file
- [ ] Chi cho .xlsx, .txt, .docx
- [ ] Kiem tra header dung mau
- [ ] Kiem tra loi tung dong
- [ ] Kiem tra trung du lieu trong file
- [ ] Kiem tra trung du lieu trong database

## Import SinhVien
- [ ] Import duoc SinhVien tu Excel
- [ ] Import duoc SinhVien tu TXT
- [ ] Import duoc SinhVien tu DOCX dang bang
- [ ] Tu tao TaiKhoan cho SinhVien
- [ ] MatKhau mac dinh 123456 nhung phai hash
- [ ] VaiTro la SinhVien
- [ ] TrangThai tai khoan la HoatDong

## Tich hop Hoc Vuot
- [ ] Import DangKyHoc co kiem tra LaHocVuot
- [ ] Import DangKyHoc co kiem tra khong qua 5 mon hoc vuot
- [ ] Dong loi hoc vuot hien thi trong Preview

## Khong duoc lam
- [ ] Khong import thang sau khi upload
- [ ] Khong dung AI
- [ ] Khong xu ly TXT tu do
- [ ] Khong xu ly DOCX doan van tu do
