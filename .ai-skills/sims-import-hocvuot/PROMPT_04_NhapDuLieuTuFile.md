# PROMPT 04 - NhapDuLieuTuFile

Bạn hãy xây dựng chức năng **NhapDuLieuTuFile** cho project ASP.NET Core MVC.

Bối cảnh dự án:

* Project sử dụng ASP.NET Core MVC.
* Sử dụng Entity Framework Core + SQL Server.
* Giao diện dùng Bootstrap 5, JavaScript, CSS.
* Đây là hệ thống SIMS - He thong quan ly sinh vien.
* Database sử dụng tên bảng, tên cột, enum, status bằng tiếng Việt không dấu.
* Không sử dụng AI trong chức năng này.
* Hệ thống chỉ đọc dữ liệu từ file theo mẫu cố định.

Mục tiêu:
Xây dựng chức năng cho phép Admin hoặc Giảng viên nhập dữ liệu vào hệ thống bằng cách upload file `.xlsx`, `.txt`, `.docx`, thay vì phải nhập tay từng bản ghi.

Chức năng phải có:

* Upload file
* Đọc dữ liệu từ file
* Kiểm tra dữ liệu từng dòng
* Hiển thị preview trước khi lưu
* Hiển thị dòng hợp lệ và dòng lỗi
* Admin hoặc Giảng viên xác nhận thì mới lưu vào database
* Lưu lịch sử import
* Cho tải file mẫu
* Cho tải file lỗi nếu có dòng sai

Không được import thẳng dữ liệu vào database ngay sau khi upload.

Quy trình bắt buộc:

```text
Upload file
↓
Đọc dữ liệu theo mẫu
↓
Kiểm tra dữ liệu
↓
Lưu dữ liệu tạm
↓
Hiển thị Preview
↓
Admin hoặc Giảng viên xác nhận
↓
Lưu dữ liệu thật vào database
↓
Hiển thị kết quả import
```

1. Công nghệ cần dùng

Cài package đọc Excel và Word:

```bash
dotnet add package ClosedXML
dotnet add package DocumentFormat.OpenXml
```

Nếu dùng Package Manager Console trong Visual Studio:

```powershell
Install-Package ClosedXML
Install-Package DocumentFormat.OpenXml
```

Yêu cầu:

* `ClosedXML`: dùng đọc file Excel `.xlsx`.
* `DocumentFormat.OpenXml`: dùng đọc file Word `.docx`.
* File `.txt` dùng `System.IO`.
* Không cần hỗ trợ file `.doc` cũ.
* Không dùng AI.
* Không xử lý file Word hoặc TXT dạng văn bản tự do.

2. Các loại file được hỗ trợ

Hệ thống chỉ hỗ trợ:

```text
.xlsx
.txt
.docx
```

Quy định:

* File Excel `.xlsx` phải có dòng đầu tiên là header.
* File TXT `.txt` phải có dòng đầu tiên là header và dùng dấu `|` để ngăn cách cột.
* File Word `.docx` chỉ hỗ trợ dữ liệu nằm trong bảng.
* Bảng đầu tiên trong file Word được xem là bảng dữ liệu cần import.
* Dòng đầu tiên của bảng Word là header.
* Không xử lý file Word dạng đoạn văn tự do.

3. Phân quyền chức năng

Tạo khu vực Admin:

```text
/Admin/NhapDuLieu
```

Admin được phép import:

* Sinh viên
* Giảng viên
* Môn học
* Lớp học
* Đăng ký học
* Bảng điểm
* Điểm danh

Nếu có thời gian, tạo thêm khu vực Giảng viên:

```text
/GiangVien/NhapDuLieu
```

Giảng viên chỉ được import:

* Bảng điểm của lớp mình phụ trách
* Điểm danh của lớp mình phụ trách
* Danh sách sinh viên thuộc lớp mình phụ trách nếu hệ thống cho phép

Giảng viên không được import:

* Khoa
* Tài khoản Admin
* Giảng viên khác
* Môn học toàn hệ thống
* Lớp không thuộc quyền phụ trách

4. Database đã có sẵn hoặc sẽ có

Chức năng này cần khớp với các bảng chính sau:

```text
TaiKhoan
SinhVien
GiangVien
Khoa
MonHoc
PhongHoc
LopHoc
LichHoc
DangKyHoc
PhienDiemDanh
DiemDanh
BangDiem
CanhBaoSinhVien
```

Nếu một số bảng như `BangDiem`, `DiemDanh`, `PhienDiemDanh` chưa có thì cần phối hợp với phần Giảng viên Portal.

5. Database cần tạo thêm

Tạo model `LichSuNhapDuLieu`.

Các cột:

* Id
* TaiKhoanId
* TenFile
* LoaiFile
* LoaiDuLieu
* TongSoDong
* SoDongHopLe
* SoDongLoi
* TrangThai
* NgayNhap
* GhiChu

`LoaiFile` gồm:

* `Xlsx`
* `Txt`
* `Docx`

`LoaiDuLieu` gồm:

* `SinhVien`
* `GiangVien`
* `MonHoc`
* `LopHoc`
* `DangKyHoc`
* `BangDiem`
* `DiemDanh`

`TrangThai` gồm:

* `DangKiemTra`
* `ChoXacNhan`
* `DaNhap`
* `ThatBai`
* `DaHuy`

Tạo model `LoiNhapDuLieu`.

Các cột:

* Id
* LichSuNhapDuLieuId
* SoDong
* NoiDungDong
* NoiDungLoi
* NgayTao

Tạo model `DuLieuNhapTam`.

Mục đích:
Lưu dữ liệu đã đọc từ file trong bước preview, chưa lưu chính thức vào bảng thật.

Các cột:

* Id
* LichSuNhapDuLieuId
* SoDong
* LoaiDuLieu
* NoiDungJson
* HopLe
* NoiDungLoi
* NgayTao

Lưu ý:

* `NoiDungJson` dùng để lưu dữ liệu từng dòng dưới dạng JSON.
* Khi người dùng bấm “Xác nhận nhập dữ liệu”, hệ thống mới đọc từ `DuLieuNhapTam` rồi insert hoặc update vào bảng thật.
* Nếu import bị hủy thì đổi trạng thái `LichSuNhapDuLieu` thành `DaHuy`.

6. Migration

Vì chức năng này tạo thêm bảng mới nên bắt buộc chạy migration.

Lệnh CLI:

```bash
dotnet ef migrations add AddNhapDuLieuTuFile
dotnet ef database update
```

Nếu dùng Package Manager Console:

```powershell
Add-Migration AddNhapDuLieuTuFile
Update-Database
```

Lưu ý:

* Migration này chạy sau migration nền tảng `InitialCreateSIMSCore`.
* Nếu import bảng điểm hoặc điểm danh thì cần đảm bảo đã có migration phần Giảng viên:

  * `AddGiangVienDiemDanhVaBangDiem`

7. Controller cần tạo

Tạo controller:

```text
Controllers/Admin/NhapDuLieuController.cs
```

Các action cần có:

```text
Index()
KiemTraDuLieu(IFormFile file, string loaiDuLieu)
Preview(int id)
XacNhanNhap(int id, bool chiNhapDongHopLe = false)
HuyNhap(int id)
KetQua(int id)
LichSu()
ChiTiet(int id)
TaiFileMau(string loaiDuLieu, string loaiFile)
TaiFileLoi(int id)
```

Nếu làm thêm cho giảng viên, tạo:

```text
Controllers/GiangVien/NhapDuLieuController.cs
```

Nhưng controller của Giảng viên phải kiểm tra quyền:

* Giảng viên chỉ được import dữ liệu của lớp mình phụ trách.
* Không được import lớp của giảng viên khác.
* Không được import dữ liệu toàn hệ thống.

8. Service cần tạo

Tạo folder:

```text
Services/Import
```

Tạo interface chính:

```csharp
public interface IFileImportService
{
    Task<ImportPreviewResult> PreviewAsync(IFormFile file, string loaiDuLieu, int taiKhoanId);
    Task<ImportConfirmResult> ConfirmAsync(int lichSuNhapDuLieuId, int taiKhoanId, bool chiNhapDongHopLe = false);
}
```

Tạo các service con:

```text
ExcelImportReader
TxtImportReader
DocxImportReader
ImportValidationService
ImportSaveService
FileMauService
FileLoiService
```

Nhiệm vụ:

`ExcelImportReader`:

* Đọc file `.xlsx`.
* Lấy header từ dòng đầu tiên.
* Đọc từng dòng dữ liệu sau header.

`TxtImportReader`:

* Đọc file `.txt`.
* Tách cột bằng dấu `|`.
* Dòng đầu tiên là header.

`DocxImportReader`:

* Đọc file `.docx`.
* Lấy bảng đầu tiên trong Word.
* Dòng đầu tiên của bảng là header.

`ImportValidationService`:

* Kiểm tra dữ liệu từng dòng.
* Trả ra dòng hợp lệ hoặc dòng lỗi.

`ImportSaveService`:

* Lưu dữ liệu hợp lệ vào bảng thật.
* Chỉ chạy khi người dùng xác nhận import.

`FileMauService`:

* Tạo file mẫu `.xlsx`, `.txt`, `.docx`.

`FileLoiService`:

* Tạo file lỗi chứa các dòng không hợp lệ và lý do lỗi.

9. View cần tạo

Admin views:

```text
Views/Admin/NhapDuLieu/Index.cshtml
Views/Admin/NhapDuLieu/Preview.cshtml
Views/Admin/NhapDuLieu/KetQua.cshtml
Views/Admin/NhapDuLieu/LichSu.cshtml
Views/Admin/NhapDuLieu/ChiTiet.cshtml
```

Nếu làm thêm Giảng viên views:

```text
Views/GiangVien/NhapDuLieu/Index.cshtml
Views/GiangVien/NhapDuLieu/Preview.cshtml
Views/GiangVien/NhapDuLieu/KetQua.cshtml
Views/GiangVien/NhapDuLieu/LichSu.cshtml
```

10. Giao diện trang Index

Tạo trang:

```text
/Admin/NhapDuLieu/Index
```

Giao diện dùng Bootstrap 5.

Nội dung trang gồm:

Card 1: Hướng dẫn import

* Bước 1: Chọn loại dữ liệu cần nhập
* Bước 2: Tải file mẫu
* Bước 3: Điền dữ liệu vào file mẫu
* Bước 4: Upload file
* Bước 5: Kiểm tra dữ liệu
* Bước 6: Xác nhận import

Card 2: Form upload

* Dropdown chọn loại dữ liệu:

  * Sinh viên
  * Giảng viên
  * Môn học
  * Lớp học
  * Đăng ký học
  * Bảng điểm
  * Điểm danh
* Dropdown chọn loại file mẫu:

  * Excel `.xlsx`
  * TXT `.txt`
  * Word `.docx`
* Nút “Tải file mẫu”
* Input chọn file upload
* Hiển thị tên file đã chọn
* Nút “Kiểm tra dữ liệu”

Card 3: Lưu ý

* Không đổi tên header trong file mẫu
* Không để trống mã sinh viên, mã giảng viên, mã môn học, mã lớp
* Excel phải là `.xlsx`
* TXT phải dùng dấu `|`
* Word phải chứa bảng dữ liệu
* File tối đa 5MB hoặc 10MB
* Hệ thống không xử lý file sai mẫu

11. Giao diện trang Preview

Tạo trang:

```text
/Admin/NhapDuLieu/Preview/{id}
```

Trong đó `{id}` là `LichSuNhapDuLieuId`.

Trang Preview cần hiển thị thông tin:

* Tên file
* Loại file
* Loại dữ liệu
* Tổng số dòng
* Số dòng hợp lệ
* Số dòng lỗi
* Ngày nhập
* Người upload
* Trạng thái

Hiển thị thống kê dạng Bootstrap card:

* Tổng số dòng
* Dòng hợp lệ
* Dòng lỗi
* Tỷ lệ hợp lệ

Hiển thị bảng preview:

Các cột:

* STT
* Số dòng
* Nội dung dữ liệu
* Trạng thái
* Ghi chú lỗi

Trạng thái:

* Badge xanh: `HopLe`
* Badge đỏ: `Loi`

Màu dòng:

* Dòng hợp lệ: nền xanh nhạt
* Dòng lỗi: nền đỏ nhạt

Nút chức năng:

* “Xác nhận nhập dữ liệu”
* “Chỉ nhập dòng hợp lệ”
* “Tải file lỗi”
* “Hủy import”
* “Quay lại”

Quy tắc:

* Nếu không có dòng lỗi, cho phép bấm “Xác nhận nhập dữ liệu”.
* Nếu có dòng lỗi, mặc định không cho import toàn bộ.
* Có thể cho phép nút “Chỉ nhập dòng hợp lệ” nếu muốn.
* Khi bấm xác nhận phải hiện SweetAlert2 để hỏi lại.

12. Giao diện trang Kết quả

Tạo trang:

```text
/Admin/NhapDuLieu/KetQua/{id}
```

Hiển thị:

* Import thành công hay thất bại
* Tên file
* Loại dữ liệu
* Tổng số dòng
* Số dòng đã nhập thành công
* Số dòng lỗi
* Thời gian import
* Người import

Nút chức năng:

* “Xem lịch sử import”
* “Import file khác”
* “Xem chi tiết”
* “Tải file lỗi” nếu có lỗi

13. Giao diện trang Lịch sử import

Tạo trang:

```text
/Admin/NhapDuLieu/LichSu
```

Hiển thị danh sách các lần import.

Các cột:

* Id
* Tên file
* Loại file
* Loại dữ liệu
* Tổng số dòng
* Dòng hợp lệ
* Dòng lỗi
* Trạng thái
* Ngày nhập
* Người nhập
* Thao tác

Thao tác:

* Xem chi tiết
* Xem lỗi
* Tải file lỗi
* Xóa lịch sử nếu cần

Dùng DataTables.js nếu có thời gian:

* Tìm kiếm
* Sắp xếp
* Phân trang

14. CSS và JavaScript

Tạo CSS riêng:

```text
wwwroot/css/nhapdulieu.css
```

Style cần có:

* Card upload đẹp
* Khu vực chọn file rõ ràng
* Bảng preview dễ đọc
* Badge trạng thái
* Màu dòng hợp lệ/lỗi
* Responsive trên mobile/tablet

Tạo JS riêng:

```text
wwwroot/js/nhapdulieu.js
```

JavaScript cần xử lý:

* Kiểm tra người dùng đã chọn file chưa
* Kiểm tra đuôi file trước khi submit
* Chỉ cho phép `.xlsx`, `.txt`, `.docx`
* Hiển thị tên file đã chọn
* Hiển thị loading khi đang kiểm tra dữ liệu
* SweetAlert2 confirm trước khi import
* SweetAlert2 confirm trước khi hủy
* SweetAlert2 thông báo nếu file sai định dạng

15. Thêm menu vào layout Admin

Trong sidebar Admin thêm menu:

```text
Nhap du lieu
```

Icon gợi ý:

* `bi-file-earmark-arrow-up`
* `bi-upload`
* `fa-file-import`

Menu dẫn đến:

```text
/Admin/NhapDuLieu
```

Nếu có layout Giảng viên, thêm menu:

```text
Nhap du lieu lop hoc
```

Dẫn đến:

```text
/GiangVien/NhapDuLieu
```

16. Định dạng file Excel `.xlsx`

File Excel phải có dòng đầu tiên là header.

Mẫu import `SinhVien`:

```text
MaSinhVien | HoTen | Email | SoDienThoai | NgaySinh | GioiTinh | DiaChi | MaKhoa | TrangThai
```

Ví dụ:

```text
SV001 | Nguyen Van A | nguyenvana@gmail.com | 0900000001 | 2004-01-01 | Nam | Ha Noi | CNTT | DangHoc
SV002 | Tran Thi B | tranthib@gmail.com | 0900000002 | 2004-02-02 | Nu | Da Nang | CNTT | DangHoc
```

Mẫu import `GiangVien`:

```text
MaGiangVien | HoTen | Email | SoDienThoai | MaKhoa | HocVi
```

Mẫu import `MonHoc`:

```text
MaMonHoc | TenMonHoc | SoTinChi | MaKhoa | MoTa
```

Mẫu import `LopHoc`:

```text
MaLop | TenLop | MaMonHoc | MaGiangVien | HocKy | NamHoc | SoLuongToiDa | TrangThai
```

Mẫu import `DangKyHoc`:

```text
MaSinhVien | MaLop | HocKy | NamHoc | LaHocVuot | TrangThai
```

Mẫu import `BangDiem`:

```text
MaSinhVien | MaLop | DiemChuyenCan | DiemBaiTap | DiemGiuaKy | DiemCuoiKy
```

Mẫu import `DiemDanh`:

```text
MaSinhVien | MaLop | NgayDiemDanh | TrangThai | GhiChu
```

17. Định dạng file TXT `.txt`

File TXT phải dùng dấu `|` để ngăn cách cột.

Dòng đầu tiên là header.

Ví dụ import sinh viên:

```text
MaSinhVien|HoTen|Email|SoDienThoai|NgaySinh|GioiTinh|DiaChi|MaKhoa|TrangThai
SV001|Nguyen Van A|nguyenvana@gmail.com|0900000001|2004-01-01|Nam|Ha Noi|CNTT|DangHoc
SV002|Tran Thi B|tranthib@gmail.com|0900000002|2004-02-02|Nu|Da Nang|CNTT|DangHoc
```

Không xử lý TXT dạng văn bản tự do.

18. Định dạng file DOCX `.docx`

File Word chỉ hỗ trợ dữ liệu nằm trong bảng.

Quy định:

* Bảng đầu tiên trong Word là bảng dữ liệu cần import.
* Dòng đầu tiên là header.
* Các dòng tiếp theo là dữ liệu.
* Header phải đúng với file mẫu.

Ví dụ bảng Word import sinh viên:

```text
MaSinhVien | HoTen | Email | SoDienThoai | NgaySinh | GioiTinh | DiaChi | MaKhoa | TrangThai
```

Không xử lý Word dạng đoạn văn tự do.

19. Chức năng tải file mẫu

Tạo action:

```text
/Admin/NhapDuLieu/TaiFileMau?loaiDuLieu=SinhVien&loaiFile=Xlsx
```

Cho phép tải file mẫu theo loại dữ liệu và loại file.

Cần hỗ trợ:

* Mẫu Excel `.xlsx`
* Mẫu TXT `.txt`
* Mẫu Word `.docx`

Ưu tiên làm theo thứ tự:

1. Excel
2. TXT
3. DOCX

File mẫu cần có header đúng với hệ thống.

20. Validation khi import SinhVien

Kiểm tra bắt buộc:

* `MaSinhVien` không được trống
* `HoTen` không được trống
* `Email` không được trống
* `Email` đúng định dạng
* `MaSinhVien` không được trùng trong file
* `MaSinhVien` không được trùng trong database
* `Email` không được trùng trong database nếu hệ thống yêu cầu
* `MaKhoa` phải tồn tại trong bảng `Khoa`
* `NgaySinh` đúng định dạng `yyyy-MM-dd`
* `GioiTinh` chỉ nhận:

  * `Nam`
  * `Nu`
  * `Khac`
* `TrangThai` chỉ nhận:

  * `DangHoc`
  * `BaoLuu`
  * `DaTotNghiep`
  * `ThoiHoc`

Khi import SinhVien thành công:

* Tạo bản ghi `SinhVien`
* Nếu chưa có tài khoản, tự động tạo `TaiKhoan`
* `TenDangNhap` mặc định là `MaSinhVien`
* `MatKhau` mặc định là `123456`
* Lưu mật khẩu dạng hash, không lưu plain text
* `VaiTro` là `SinhVien`
* `TrangThai` tài khoản là `HoatDong`

21. Validation khi import GiangVien

Kiểm tra:

* `MaGiangVien` không được trống
* `HoTen` không được trống
* `Email` đúng định dạng
* `MaKhoa` phải tồn tại
* `MaGiangVien` không được trùng trong file
* `MaGiangVien` không được trùng trong database

Khi import GiangVien thành công:

* Tạo bản ghi `GiangVien`
* Nếu chưa có tài khoản, tự động tạo `TaiKhoan`
* `TenDangNhap` mặc định là `MaGiangVien`
* `MatKhau` mặc định là `123456`
* Lưu mật khẩu dạng hash, không lưu plain text
* `VaiTro` là `GiangVien`
* `TrangThai` tài khoản là `HoatDong`

22. Validation khi import MonHoc

Kiểm tra:

* `MaMonHoc` không được trống
* `TenMonHoc` không được trống
* `SoTinChi` phải lớn hơn 0
* `MaKhoa` phải tồn tại
* `MaMonHoc` không được trùng trong file
* `MaMonHoc` không được trùng trong database

23. Validation khi import LopHoc

Kiểm tra:

* `MaLop` không được trống
* `TenLop` không được trống
* `MaMonHoc` phải tồn tại trong bảng `MonHoc`
* `MaGiangVien` phải tồn tại trong bảng `GiangVien`
* `HocKy` không được trống
* `NamHoc` không được trống
* `SoLuongToiDa` phải lớn hơn 0
* `MaLop` không được trùng trong file
* `MaLop` không được trùng trong database
* `TrangThai` chỉ nhận:

  * `DangMo`
  * `DaDong`
  * `DaHuy`

24. Validation khi import DangKyHoc

Kiểm tra:

* `MaSinhVien` phải tồn tại
* `MaLop` phải tồn tại
* Sinh viên chưa đăng ký lớp đó trước đây
* Không cho đăng ký nếu lớp đã đủ số lượng tối đa
* Không cho đăng ký nếu trùng lịch học
* Nếu `LaHocVuot = true`, kiểm tra sinh viên không được đăng ký quá 5 môn học vượt trong cùng `HocKy` và `NamHoc`
* `TrangThai` chỉ nhận:

  * `ChoDuyet`
  * `DaDuyet`
  * `TuChoi`
  * `DaHuy`

Logic kiểm tra không quá 5 môn học vượt:

```csharp
var soMonHocVuot = _context.DangKyHoc
    .Count(x => x.SinhVienId == sinhVienId
             && x.HocKy == hocKy
             && x.NamHoc == namHoc
             && x.LaHocVuot == true
             && x.TrangThai != "DaHuy"
             && x.TrangThai != "TuChoi");

if (soMonHocVuot >= 5)
{
    // Bao loi: Sinh vien khong duoc dang ky qua 5 mon hoc vuot
}
```

25. Validation khi import BangDiem

Kiểm tra:

* `MaSinhVien` phải tồn tại
* `MaLop` phải tồn tại
* Sinh viên phải có trong bảng `DangKyHoc` của lớp đó
* Các điểm phải nằm trong khoảng 0 đến 10:

  * `DiemChuyenCan`
  * `DiemBaiTap`
  * `DiemGiuaKy`
  * `DiemCuoiKy`

Khi import điểm:

* Nếu sinh viên chưa có `BangDiem`, tạo mới
* Nếu đã có `BangDiem`, cập nhật điểm
* Tự động tính:

```text
DiemTongKet = DiemChuyenCan * 0.1 + DiemBaiTap * 0.2 + DiemGiuaKy * 0.3 + DiemCuoiKy * 0.4
```

Tự động quy đổi `DiemChu`:

* `A` nếu `DiemTongKet >= 8.5`
* `B` nếu `DiemTongKet >= 7.0`
* `C` nếu `DiemTongKet >= 5.5`
* `D` nếu `DiemTongKet >= 4.0`
* `F` nếu `DiemTongKet < 4.0`

Nếu người import là Giảng viên:

* Chỉ được import điểm cho lớp do giảng viên đó phụ trách.

26. Validation khi import DiemDanh

Kiểm tra:

* `MaSinhVien` phải tồn tại
* `MaLop` phải tồn tại
* Sinh viên phải thuộc lớp đó qua bảng `DangKyHoc`
* `NgayDiemDanh` đúng định dạng `yyyy-MM-dd`
* `TrangThai` chỉ nhận:

  * `CoMat`
  * `DiMuon`
  * `Vang`
  * `CoPhep`

Nếu người import là Giảng viên:

* Chỉ được import điểm danh cho lớp do giảng viên đó phụ trách.

Nếu đã có điểm danh cùng sinh viên, lớp, ngày:

* Có thể cập nhật dữ liệu cũ
* Hoặc báo lỗi tùy cách triển khai
* Mặc định nên cho cập nhật để tiện import lại file sửa lỗi

27. Bảo mật upload file

Bắt buộc kiểm tra:

* File không được null
* File không được rỗng
* File không quá 5MB hoặc 10MB
* Chỉ nhận extension:

  * `.xlsx`
  * `.txt`
  * `.docx`
* Không tin vào tên file gốc của người dùng
* Nếu cần lưu file, đổi tên file bằng GUID
* Không lưu file upload vào thư mục public nếu không cần
* Không execute bất kỳ nội dung nào từ file
* Không cho upload file có extension lạ

28. Logic xử lý trùng dữ liệu

Quy tắc mặc định:

* `SinhVien`: trùng `MaSinhVien` thì báo lỗi
* `GiangVien`: trùng `MaGiangVien` thì báo lỗi
* `MonHoc`: trùng `MaMonHoc` thì báo lỗi
* `LopHoc`: trùng `MaLop` thì báo lỗi
* `DangKyHoc`: nếu sinh viên đã đăng ký lớp đó thì báo lỗi
* `BangDiem`: nếu đã có thì cập nhật
* `DiemDanh`: nếu đã có cùng sinh viên, lớp, ngày thì cập nhật

29. Tải file lỗi

Tạo chức năng tải file lỗi:

```text
/Admin/NhapDuLieu/TaiFileLoi/{id}
```

File lỗi nên có các cột:

* SoDong
* NoiDungDong
* NoiDungLoi

Có thể xuất file lỗi dạng:

* `.xlsx`
* hoặc `.txt`

Ưu tiên `.xlsx` nếu đã dùng ClosedXML.

30. ViewModel cần tạo

Tạo các ViewModel cần thiết:

```text
NhapDuLieuIndexViewModel
ImportPreviewViewModel
ImportRowPreviewViewModel
ImportResultViewModel
LichSuNhapDuLieuViewModel
```

`ImportRowPreviewViewModel` nên có:

* SoDong
* NoiDungHienThi
* HopLe
* NoiDungLoi

31. Kết quả cần bàn giao

Sau khi hoàn thành, chức năng phải đáp ứng:

* Admin upload được file `.xlsx`
* Admin upload được file `.txt`
* Admin upload được file `.docx` dạng bảng
* Hệ thống đọc được dữ liệu theo mẫu
* Hệ thống kiểm tra lỗi từng dòng
* Hệ thống hiển thị preview trước khi lưu
* Admin xác nhận mới lưu vào database
* Có lịch sử import
* Có chi tiết lỗi import
* Có tải file mẫu
* Có tải file lỗi
* Import sinh viên tự tạo tài khoản sinh viên
* Import giảng viên tự tạo tài khoản giảng viên
* Import đăng ký học kiểm tra không quá 5 môn học vượt
* Import điểm tự tính điểm tổng kết và điểm chữ
* Nếu giảng viên import thì chỉ được thao tác trên lớp của mình
* Giao diện đẹp, rõ ràng, responsive, dùng Bootstrap 5, JavaScript, CSS
* Tên bảng, tên cột, enum, status trong database đều dùng tiếng Việt không dấu
* Không sử dụng AI

32. Gợi ý thứ tự làm

Làm theo thứ tự sau:

```text
1. Tạo model LichSuNhapDuLieu, LoiNhapDuLieu, DuLieuNhapTam
2. Add migration và update database
3. Tạo service đọc file Excel
4. Tạo service đọc file TXT
5. Tạo service đọc file DOCX dạng bảng
6. Tạo service validation dữ liệu
7. Tạo service lưu dữ liệu thật
8. Tạo controller Admin/NhapDuLieu
9. Tạo view Index
10. Tạo view Preview
11. Tạo chức năng Confirm import
12. Tạo trang KetQua
13. Tạo trang LichSu
14. Tạo trang ChiTiet
15. Tạo chức năng tải file mẫu
16. Tạo chức năng tải file lỗi
17. Thêm CSS nhapdulieu.css
18. Thêm JS nhapdulieu.js
19. Thêm menu vào layout Admin
20. Test import SinhVien trước
21. Sau đó mở rộng import GiangVien, MonHoc, LopHoc, DangKyHoc, BangDiem, DiemDanh
```

33. Phiên bản MVP bắt buộc

Nếu không đủ thời gian làm toàn bộ, bắt buộc hoàn thành MVP sau:

* Import Excel `.xlsx` cho SinhVien
* Có tải file mẫu Excel
* Có kiểm tra dữ liệu
* Có preview dữ liệu
* Có báo lỗi từng dòng
* Có xác nhận rồi mới lưu database
* Có tự tạo tài khoản sinh viên
* Có lưu lịch sử import
* Có giao diện Bootstrap rõ ràng

Sau khi MVP ổn định mới mở rộng:

* Import TXT
* Import DOCX dạng bảng
* Import GiangVien
* Import MonHoc
* Import LopHoc
* Import DangKyHoc
* Import BangDiem
* Import DiemDanh


Yeu cau quan trong:
- Khong dung AI.
- Chi doc file theo mau co dinh.
- Ho tro .xlsx, .txt, .docx.
- Phai co preview truoc khi luu database.
- Phai co validation tung dong.
- Phai co lich su import.
- Phai co tai file mau.
- Phai co tai file loi.
