# Checklist: GioiHanHocVuotToiDa5Mon

## Database
- [ ] Bang DangKyHoc co cot HocKy
- [ ] Bang DangKyHoc co cot NamHoc
- [ ] Bang DangKyHoc co cot LaHocVuot
- [ ] TrangThai co ChoDuyet, DaDuyet, TuChoi, DaHuy
- [ ] Neu co bang CauHinhHocVuot thi da tao model
- [ ] Da them DbSet neu co bang moi
- [ ] Da tao migration AddGioiHanHocVuot
- [ ] Da chay Update-Database

## Service
- [ ] Da tao IGioiHanHocVuotService
- [ ] Da tao GioiHanHocVuotService
- [ ] Co ham LaySoMonHocVuotDaDangKyAsync
- [ ] Co ham LaySoMonHocVuotToiDaAsync
- [ ] Co ham CoTheDangKyHocVuotAsync
- [ ] Co ham LayThongBaoGioiHanAsync

## Logic
- [ ] Chi dem LaHocVuot = true
- [ ] Chi dem cung SinhVienId
- [ ] Chi dem cung HocKy
- [ ] Chi dem cung NamHoc
- [ ] Khong dem TrangThai = DaHuy
- [ ] Khong dem TrangThai = TuChoi
- [ ] Neu >= 5 thi khong cho dang ky them
- [ ] Thong bao loi ro rang

## Tich hop
- [ ] Tich hop vao DangKyHocController
- [ ] Tich hop vao GoiYHocVuotController
- [ ] Tich hop vao NhapDuLieuController khi import DangKyHoc
- [ ] Tich hop vao Student Portal neu sinh vien tu dang ky
- [ ] Tich hop vao Admin neu Admin dang ky thay sinh vien

## UI SinhVien
- [ ] Hien thi so mon hoc vuot dang 0/5, 1/5...
- [ ] Co progress bar
- [ ] Neu 4/5 thi canh bao mau vang
- [ ] Neu 5/5 thi canh bao mau do
- [ ] Disable nut dang ky neu du 5 mon
- [ ] Co SweetAlert2 khi vuot gioi han

## Import
- [ ] Import DangKyHoc kiem tra LaHocVuot
- [ ] Neu sinh vien du 5 mon thi dong import bi loi
- [ ] Loi hien thi trong Preview
- [ ] Khong luu dong loi vao database

## Khong duoc lam
- [ ] Khong chi kiem tra bang JavaScript
- [ ] Khong bo qua service
- [ ] Khong hard-code rai rac nhieu noi
- [ ] Khong dung AI
