# PROMPT 06 - GioiHanHocVuotToiDa5Mon

Bạn hãy xây dựng chức năng **GioiHanHocVuotToiDa5Mon** cho project ASP.NET Core MVC.

Bối cảnh:

* Project sử dụng ASP.NET Core MVC.
* Sử dụng Entity Framework Core + SQL Server.
* Giao diện dùng Bootstrap 5, JavaScript, CSS.
* Đây là hệ thống SIMS - He thong quan ly sinh vien.
* Database sử dụng tên bảng, tên cột, enum, status bằng tiếng Việt không dấu.
* Không sử dụng AI trong chức năng này.
* Chức năng này phải khớp với module `DangKyHoc`, `LopHoc`, `SinhVien`, `GoiYHocVuot`.

Mục tiêu:
Xây dựng chức năng kiểm soát sinh viên không được đăng ký quá 5 môn học vượt trong cùng một học kỳ và năm học.

Luật nghiệp vụ chính:

```text id="9ejgej"
Mot sinh vien khong duoc dang ky qua 5 mon hoc vuot trong cung HocKy va NamHoc.
```

Luật này phải được kiểm tra ở:

* Giao diện
* Controller
* Service
* Khi import dữ liệu từ file
* Khi sinh viên đăng ký từ gợi ý học vượt
* Khi Admin đăng ký học thay sinh viên

1. Database cần kiểm tra hoặc bổ sung

Bảng `DangKyHoc` cần có các cột:

* Id
* SinhVienId
* LopHocId
* NgayDangKy
* HocKy
* NamHoc
* LaHocVuot
* TrangThai

Nếu bảng `DangKyHoc` hiện tại chưa có các cột sau thì bổ sung:

```text id="e0ouaw"
HocKy
NamHoc
LaHocVuot
```

`TrangThai` gồm:

* `ChoDuyet`
* `DaDuyet`
* `TuChoi`
* `DaHuy`

Tạo thêm bảng `CauHinhHocVuot` nếu muốn Admin có thể đổi giới hạn.

Model `CauHinhHocVuot`:

* Id
* SoMonHocVuotToiDa
* ApDungTuNamHoc
* ApDungTuHocKy
* DangApDung
* NgayTao
* NgayCapNhat
* GhiChu

Mặc định:

* `SoMonHocVuotToiDa = 5`
* `DangApDung = true`

Nếu muốn MVP đơn giản, có thể không tạo bảng `CauHinhHocVuot`, mà để số 5 là constant trong service.

Khuyến nghị:

* Nên tạo `CauHinhHocVuot` để Admin có thể chỉnh giới hạn sau này.

2. Migration

Nếu cần bổ sung cột vào `DangKyHoc` hoặc tạo bảng `CauHinhHocVuot`, chạy migration:

```bash id="f1rqax"
dotnet ef migrations add AddGioiHanHocVuot
dotnet ef database update
```

Nếu dùng Package Manager Console:

```powershell id="xzw764"
Add-Migration AddGioiHanHocVuot
Update-Database
```

3. Logic tính số môn học vượt

Khi sinh viên đăng ký một lớp học vượt, hệ thống phải đếm số môn học vượt hiện tại trong cùng học kỳ và năm học.

Chỉ tính các bản ghi:

* `LaHocVuot = true`
* Cùng `SinhVienId`
* Cùng `HocKy`
* Cùng `NamHoc`
* `TrangThai` không phải `DaHuy`
* `TrangThai` không phải `TuChoi`

Ví dụ logic:

```csharp id="3el4bu"
var soMonHocVuot = _context.DangKyHoc
    .Count(x => x.SinhVienId == sinhVienId
             && x.HocKy == hocKy
             && x.NamHoc == namHoc
             && x.LaHocVuot == true
             && x.TrangThai != "DaHuy"
             && x.TrangThai != "TuChoi");

if (soMonHocVuot >= 5)
{
    return false;
}
```

Thông báo lỗi:

```text id="oa1gdr"
Sinh vien khong duoc dang ky qua 5 mon hoc vuot trong cung mot hoc ky.
```

4. Service cần tạo

Tạo folder:

```text id="mxq47j"
Services/HocVuot
```

Tạo interface:

```csharp id="c0qvur"
public interface IGioiHanHocVuotService
{
    Task<int> LaySoMonHocVuotDaDangKyAsync(int sinhVienId, string hocKy, string namHoc);
    Task<int> LaySoMonHocVuotToiDaAsync(string hocKy, string namHoc);
    Task<bool> CoTheDangKyHocVuotAsync(int sinhVienId, string hocKy, string namHoc);
    Task<string> LayThongBaoGioiHanAsync(int sinhVienId, string hocKy, string namHoc);
}
```

Tạo class:

```text id="lg74l7"
GioiHanHocVuotService
```

Service này phải được dùng ở:

* `DangKyHocController`
* `GoiYHocVuotController`
* `NhapDuLieuController`
* Bất kỳ nơi nào có thao tác tạo `DangKyHoc`

5. Controller Admin

Nếu có bảng `CauHinhHocVuot`, tạo controller:

```text id="3g38wl"
Controllers/Admin/CauHinhHocVuotController.cs
```

Admin có thể:

* Xem giới hạn hiện tại
* Cập nhật số môn học vượt tối đa
* Bật/tắt cấu hình đang áp dụng
* Xem lịch sử cấu hình nếu có thời gian

Tạo trang:

```text id="8y69ry"
/Admin/CauHinhHocVuot/Index
```

6. Controller đăng ký học

Trong controller hoặc service xử lý đăng ký học, bắt buộc kiểm tra:

```text id="mxiqoy"
Neu LaHocVuot = true thi goi IGioiHanHocVuotService.CoTheDangKyHocVuotAsync()
```

Nếu kết quả là false:

* Không cho lưu `DangKyHoc`
* Trả về lỗi cho giao diện
* Không tạo bản ghi đăng ký học
* Không cập nhật trạng thái gợi ý thành `DaDangKy`

7. Áp dụng trong chức năng gợi ý học vượt

Ở trang `/SinhVien/GoiYHocVuot`, khi sinh viên bấm “Đăng ký”:

* Kiểm tra lớp còn chỗ
* Kiểm tra không trùng lịch
* Kiểm tra môn tiên quyết
* Kiểm tra chưa quá 5 môn học vượt
* Nếu hợp lệ mới tạo `DangKyHoc`

Nếu đã đủ 5 môn:

* Disable nút “Đăng ký”
* Hiển thị badge:

```text id="kw63lg"
Da dat gioi han hoc vuot 5/5
```

8. Áp dụng trong import dữ liệu

Trong chức năng `NhapDuLieuTuFile`, khi import `DangKyHoc`:

Nếu dòng import có:

```text id="o57l5j"
LaHocVuot = true
```

Thì bắt buộc kiểm tra giới hạn 5 môn.

Nếu sinh viên đã đủ 5 môn học vượt:

* Dòng đó phải báo lỗi
* Không cho import dòng đó
* Hiển thị lỗi ở Preview

Thông báo lỗi:

```text id="xawf77"
Dong {soDong}: Sinh vien {MaSinhVien} da dang ky du 5 mon hoc vuot trong hoc ky {HocKy} nam hoc {NamHoc}.
```

9. Giao diện Sinh viên

Trên trang `/SinhVien/GoiYHocVuot` hoặc `/SinhVien/DangKyHoc`, hiển thị card:

```text id="qq791a"
Mon hoc vuot da dang ky: 3/5
Con co the dang ky them: 2 mon
```

Nếu đạt giới hạn:

```text id="5r3e5q"
Ban da dat gioi han 5 mon hoc vuot trong hoc ky nay.
```

Dùng progress bar Bootstrap:

```text id="rseplc"
0/5: xanh
1/5 - 3/5: xanh
4/5: vang
5/5: do
```

10. Giao diện Admin cấu hình học vượt

Tạo trang:

```text id="2i11hq"
/Admin/CauHinhHocVuot/Index
```

Giao diện gồm:

* Card hiển thị giới hạn hiện tại
* Form cập nhật giới hạn
* Dropdown học kỳ
* Input năm học
* Toggle đang áp dụng
* Nút “Lưu cấu hình”

Bảng lịch sử cấu hình:

* STT
* Số môn tối đa
* Học kỳ áp dụng
* Năm học áp dụng
* Trạng thái
* Ngày tạo
* Ngày cập nhật
* Thao tác

Nếu MVP đơn giản không làm bảng cấu hình thì tạo giao diện chỉ để hiển thị:

```text id="7gdajv"
Gioi han hoc vuot hien tai: 5 mon / hoc ky
```

11. Giao diện Admin xem sinh viên học vượt

Tạo trang:

```text id="6n8jzz"
/Admin/HocVuot/ThongKe
```

Hiển thị:

* Bộ lọc Khoa
* Bộ lọc Học kỳ
* Bộ lọc Năm học
* Bảng sinh viên có đăng ký học vượt

Các cột:

* Mã sinh viên
* Họ tên
* Khoa
* Số môn học vượt đã đăng ký
* Giới hạn
* Trạng thái
* Thao tác

Trạng thái:

* `BinhThuong`: dưới 5 môn
* `GanDatGioiHan`: 4/5 môn
* `DaDatGioiHan`: 5/5 môn

Dùng badge:

* Xanh: Bình thường
* Vàng: Gần đạt giới hạn
* Đỏ: Đã đạt giới hạn

12. CSS và JavaScript

Tạo CSS:

```text id="1czd9f"
wwwroot/css/hocvuot.css
```

Tạo JS:

```text id="f6wpfy"
wwwroot/js/hocvuot.js
```

JavaScript cần có:

* Hiển thị progress bar số môn học vượt
* SweetAlert2 khi sinh viên đăng ký học vượt
* SweetAlert2 khi sinh viên đã đạt giới hạn
* Disable nút đăng ký nếu đã đủ 5 môn
* Loading khi kiểm tra đăng ký

13. Menu

Thêm vào sidebar Admin:

```text id="8kqp2c"
Hoc vuot
- Cau hinh hoc vuot
- Thong ke hoc vuot
```

Thêm vào sidebar SinhVien:

```text id="n6kl46"
Goi y hoc vuot
```

14. Validation bắt buộc

Khi thêm hoặc cập nhật `DangKyHoc`:

* `SinhVienId` phải tồn tại
* `LopHocId` phải tồn tại
* `HocKy` không được trống
* `NamHoc` không được trống
* `LaHocVuot` phải xác định true/false
* Nếu `LaHocVuot = true`, không được vượt quá 5 môn
* Không tính các bản ghi `DaHuy`, `TuChoi`
* Không cho đăng ký trùng lớp
* Không cho đăng ký nếu lớp đủ số lượng
* Không cho đăng ký nếu trùng lịch

15. Kết quả cần bàn giao

* Hệ thống chặn sinh viên đăng ký quá 5 môn học vượt
* Luật này áp dụng ở đăng ký học thường
* Luật này áp dụng ở gợi ý học vượt
* Luật này áp dụng khi import `DangKyHoc` từ file
* Sinh viên nhìn thấy số môn học vượt đã đăng ký dạng 3/5, 4/5, 5/5
* Admin xem được thống kê sinh viên học vượt
* Nếu có bảng cấu hình, Admin chỉnh được số môn tối đa
* Giao diện dùng Bootstrap 5, JavaScript, CSS
* Không dùng AI
* Tên bảng, tên cột, enum, status trong database dùng tiếng Việt không dấu

16. MVP bắt buộc

Nếu không đủ thời gian, cần hoàn thành trước:

* Thêm cột `LaHocVuot`, `HocKy`, `NamHoc` vào `DangKyHoc` nếu chưa có
* Tạo `GioiHanHocVuotService`
* Chặn không quá 5 môn học vượt khi đăng ký
* Hiển thị số môn học vượt 0/5, 1/5, 2/5...
* Áp dụng kiểm tra trong import `DangKyHoc`
* Giao diện Bootstrap cơ bản cho sinh viên xem giới hạn

Sau đó mới mở rộng:

* Bảng `CauHinhHocVuot`
* Admin chỉnh giới hạn
* Thống kê học vượt
* Progress bar đẹp
* Biểu đồ thống kê


Yeu cau quan trong:
- Khong dung AI.
- Mot sinh vien khong duoc dang ky qua 5 mon hoc vuot trong cung HocKy va NamHoc.
- Phai co LaHocVuot trong DangKyHoc.
- Phai co HocKy trong DangKyHoc.
- Phai co NamHoc trong DangKyHoc.
- Phai kiem tra trong service.
- Phai kiem tra trong controller.
- Phai kiem tra khi import DangKyHoc tu file.
- Phai kiem tra khi sinh vien dang ky tu goi y hoc vuot.
