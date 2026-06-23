# PROMPT 05 - GoiYMonHocVuot

Ban hay xay dung chuc nang **GoiYMonHocVuot** cho project ASP.NET Core MVC.

## Boi canh

- Project su dung ASP.NET Core MVC.
- Su dung Entity Framework Core + SQL Server.
- Giao dien dung Bootstrap 5, JavaScript, CSS.
- Day la he thong SIMS - He thong quan ly sinh vien.
- Database su dung ten bang, ten cot, enum, status bang tieng Viet khong dau.
- Khong su dung AI trong chuc nang nay.
- He thong goi y mon hoc vuot dua tren rule-based logic, tuc la dua vao luat nghiep vu ro rang.

## Muc tieu

Xay dung chuc nang goi y mon hoc vuot cho sinh vien dua tren:

- Tien do hoc tap
- GPA
- Diem so
- Mon tien quyet
- Lich hoc
- Lop con cho
- So luong mon hoc vuot da dang ky

Chuc nang nay giup sinh vien biet nen hoc vuot mon nao truoc, mon nao sau, theo lo trinh tu de den nang cao.

## 1. Database da co san hoac se co

Chuc nang nay can khop voi cac bang:

```text
TaiKhoan
SinhVien
Khoa
MonHoc
LopHoc
LichHoc
DangKyHoc
BangDiem
```

Neu bang `BangDiem` chua co thi can phoi hop voi phan Giang vien Portal.

## 2. Database can tao them

Tao model `MonHocTienQuyet`.

Data columns:

```text
Id
MonHocId
MonHocTienQuyetId
BatBuoc
MucDo
GhiChu
NgayTao
```

Trong do:

- `MonHocId`: mon hoc muon dang ky.
- `MonHocTienQuyetId`: mon can hoc truoc.
- `BatBuoc`: true/false.
- `MucDo`: muc do lien quan cua mon tien quyet.

`MucDo` gom:

```text
BatBuoc
KhuyenNghi
NenHocTruoc
```

Tao model `GoiYHocVuot`.

Data columns:

```text
Id
SinhVienId
MonHocId
LopHocId
HocKyGoiY
NamHocGoiY
DiemPhuHop
LyDoGoiY
MucDoGoiY
TrangThai
NgayTao
NgayCapNhat
```

`MucDoGoiY` gom:

```text
RatPhuHop
PhuHop
CanCanNhac
KhongPhuHop
```

`TrangThai` gom:

```text
Moi
DaXem
DaDangKy
DaBoQua
HetHan
```

## 3. Migration

Vi chuc nang nay tao them bang moi nen bat buoc chay migration.

Lenh CLI:

```bash
dotnet ef migrations add AddGoiYMonHocVuot
dotnet ef database update
```

Neu dung Package Manager Console:

```powershell
Add-Migration AddGoiYMonHocVuot
Update-Database
```

## 4. Logic goi y mon hoc vuot

He thong chi goi y mon hoc vuot neu thoa man cac dieu kien:

- Sinh vien chua hoc mon do.
- Sinh vien chua dang ky lop cua mon do.
- Sinh vien da dat cac mon tien quyet bat buoc.
- Mon hoc co lop dang mo.
- Lop hoc chua du so luong.
- Khong trung lich voi cac lop sinh vien da dang ky.
- Sinh vien chua dang ky qua gioi han mon hoc vuot.
- Sinh vien co hoc luc phu hop.

## 5. Quy tac kiem tra hoc luc

Dua vao diem trung binh hoac GPA hien tai.

Goi y quy tac:

```text
GPA >= 8.0:
- Co the goi y mon nang cao
- MucDoGoiY = RatPhuHop

GPA >= 6.5 va GPA < 8.0:
- Goi y mon trung binh hoac mon dung tien do
- MucDoGoiY = PhuHop

GPA >= 5.0 va GPA < 6.5:
- Chi goi y mon nhe hoac mon nen tang
- MucDoGoiY = CanCanNhac

GPA < 5.0:
- Khong nen goi y hoc vuot
- MucDoGoiY = KhongPhuHop
```

## 6. Quy tac tinh DiemPhuHop

Tao service tinh diem phu hop tu 0 den 100.

Goi y cong thuc:

```text
DiemPhuHop = DiemGPA + DiemTienQuyet + DiemKhongTrungLich + DiemLopConCho + DiemDoKho
```

Trong do:

- `DiemGPA`: toi da 30 diem.
- `DiemTienQuyet`: toi da 30 diem.
- `DiemKhongTrungLich`: toi da 20 diem.
- `DiemLopConCho`: toi da 10 diem.
- `DiemDoKho`: toi da 10 diem.

Quy doi:

```text
DiemPhuHop >= 85: RatPhuHop
DiemPhuHop >= 70: PhuHop
DiemPhuHop >= 50: CanCanNhac
DiemPhuHop < 50: KhongPhuHop
```

## 7. Logic mon tien quyet

Vi du:

Muon hoc `LapTrinhWeb`, sinh vien nen hoc truoc:

```text
NhapMonLapTrinh
CoSoDuLieu
LapTrinhHuongDoiTuong
```

Neu mon tien quyet `BatBuoc = true` ma sinh vien chua dat thi khong duoc goi y.

Neu mon tien quyet `BatBuoc = false` thi van co the goi y nhung giam diem phu hop.

## 8. Logic kiem tra mon da hoc

Mot mon duoc xem la da hoc dat neu:

- Sinh vien co `BangDiem` cua mon do.
- `DiemTongKet >= 4.0`.
- `DiemChu` khac `F`.

Neu chua co diem hoac diem duoi 4.0 thi xem nhu chua dat.

## 9. Logic kiem tra trung lich

Khi goi y mot `LopHoc`, phai kiem tra voi cac lop sinh vien da dang ky.

Khong goi y neu:

- Cung `ThuTrongTuan`
- Thoi gian hoc giao nhau
- Ngay hoc nam trong cung khoang thoi gian

Vi du:

```text
Lop A: Thu 2, 07:00 - 09:00
Lop B: Thu 2, 08:00 - 10:00
```

Ket qua: bi trung lich, khong goi y.

## 10. Service can tao

Tao folder:

```text
Services/HocVuot
```

Tao interface:

```csharp
public interface IGoiYHocVuotService
{
    Task<List<GoiYHocVuotViewModel>> TaoGoiYChoSinhVienAsync(int sinhVienId, string hocKy, string namHoc);
    Task<List<GoiYHocVuotViewModel>> LayGoiYCuaSinhVienAsync(int sinhVienId);
    Task CapNhatTrangThaiGoiYAsync(int goiYHocVuotId, string trangThai);
    Task<bool> KiemTraMonTienQuyetAsync(int sinhVienId, int monHocId);
    Task<bool> KiemTraTrungLichAsync(int sinhVienId, int lopHocId);
    Task<decimal> TinhGPAAsync(int sinhVienId);
}
```

Tao class:

```text
GoiYHocVuotService
MonHocTienQuyetService
```

Neu da co `GioiHanHocVuotService`, phai dung lai service do de kiem tra sinh vien chua vuot qua 5 mon hoc vuot.

## 11. Controller Admin

Tao controller:

```text
Controllers/Admin/MonHocTienQuyetController.cs
```

Admin co the:

- Xem danh sach mon tien quyet
- Them mon tien quyet
- Sua mon tien quyet
- Xoa mon tien quyet
- Tim kiem theo mon hoc
- Quan ly mon nao can hoc truoc mon nao

Tao controller:

```text
Controllers/Admin/GoiYHocVuotController.cs
```

Admin co the:

- Xem danh sach goi y hoc vuot cua sinh vien
- Loc theo khoa
- Loc theo sinh vien
- Loc theo hoc ky
- Loc theo nam hoc
- Bam nut tao lai goi y cho toan bo sinh vien
- Xem ly do he thong goi y

## 12. Controller SinhVien

Tao controller hoac action trong khu vuc SinhVien:

```text
/SinhVien/GoiYHocVuot
```

Sinh vien co the:

- Xem danh sach mon duoc goi y hoc vuot
- Xem diem phu hop
- Xem ly do goi y
- Xem mon tien quyet da dat/chua dat
- Bam "Dang ky lop nay" neu lop con cho
- Bam "Bo qua goi y"
- Bam "Da xem"

## 13. Giao dien Admin quan ly mon tien quyet

Tao trang:

```text
/Admin/MonHocTienQuyet/Index
```

Giao dien gom:

- Bang danh sach mon tien quyet
- Nut them moi
- Modal them/sua
- Tim kiem theo ma mon hoc hoac ten mon hoc
- Badge hien thi `BatBuoc`, `KhuyenNghi`, `NenHocTruoc`

Columns:

```text
Mon hoc
Mon tien quyet
Bat buoc
Muc do
Ghi chu
Thao tac
```

## 14. Giao dien Admin goi y hoc vuot

Tao trang:

```text
/Admin/GoiYHocVuot/Index
```

Giao dien gom:

- Bo loc Khoa
- Bo loc Hoc ky
- Bo loc Nam hoc
- Bo loc Muc do goi y
- Nut "Tao lai goi y"
- Bang danh sach goi y

Columns:

```text
Ma sinh vien
Ho ten
Mon hoc duoc goi y
Lop hoc
Diem phu hop
Muc do goi y
Ly do
Trang thai
Ngay tao
Thao tac
```

Dung Bootstrap 5, DataTables.js, SweetAlert2.

## 15. Giao dien Sinh vien xem goi y hoc vuot

Tao trang:

```text
/SinhVien/GoiYHocVuot
```

Giao dien gom:

- Card thong ke:
  - GPA hien tai
  - So mon da hoc dat
  - So mon hoc vuot da dang ky
  - So goi y phu hop
- Danh sach mon duoc goi y dang card

Moi card hien thi:

- Ten mon hoc
- Ma mon hoc
- So tin chi
- Lop hoc de xuat
- Giang vien
- Lich hoc
- Diem phu hop
- Muc do goi y
- Ly do goi y
- Nut "Dang ky"
- Nut "Bo qua"

Mau goi y:

```text
RatPhuHop: xanh
PhuHop: xanh duong
CanCanNhac: vang
KhongPhuHop: do hoac khong hien thi
```

Neu sinh vien da du 5 mon hoc vuot thi disable nut dang ky.

## 16. CSS va JavaScript

Tao CSS:

```text
wwwroot/css/hocvuot.css
```

Tao JS:

```text
wwwroot/js/hocvuot.js
```

JavaScript can co:

- SweetAlert2 confirm khi sinh vien dang ky mon hoc vuot
- SweetAlert2 confirm khi sinh vien bo qua goi y
- Loading khi Admin bam tao lai goi y
- Hien thi progress bar cho `DiemPhuHop`
- Validate bo loc truoc khi submit neu can

## 17. Menu

Them vao sidebar Admin:

```text
Hoc vuot
- Mon hoc tien quyet
- Goi y hoc vuot
```

Them vao sidebar SinhVien:

```text
Goi y hoc vuot
```

## 18. Ket qua can ban giao

- Admin quan ly duoc mon hoc tien quyet.
- Admin tao lai goi y hoc vuot cho sinh vien.
- Sinh vien xem duoc danh sach mon hoc vuot duoc goi y.
- Goi y dua tren GPA, mon tien quyet, lich hoc, lop con cho.
- Khong goi y mon da hoc hoac da dang ky.
- Khong goi y mon bi trung lich.
- Khong goi y neu sinh vien da du 5 mon hoc vuot.
- Khong dung AI.
- Giao dien dep, responsive, dung Bootstrap 5, JavaScript, CSS.
- Ten bang, ten cot, enum, status trong database dung tieng Viet khong dau.

## 19. MVP bat buoc

Neu khong du thoi gian, can hoan thanh truoc:

- Bang `MonHocTienQuyet`
- Bang `GoiYHocVuot`
- Admin quan ly mon tien quyet
- Sinh vien xem goi y hoc vuot
- Logic kiem tra mon tien quyet
- Logic khong goi y mon da hoc
- Logic khong goi y mon da dang ky
- Logic khong goi y neu da du 5 mon hoc vuot
- Giao dien Bootstrap co ban

Sau do moi mo rong:

- Tinh diem phu hop chi tiet
- Kiem tra trung lich nang cao
- Admin tao lai goi y hang loat
- Progress bar va bieu do
