# Bài tập ADO.NET ngày 17/10/2026

Mẫu project C# .NET 8, SQL Server, ADO.NET, phân tầng theo **View → Business → DataAccess**.

## Có gì trong project

- `QuanLySinhVien.Data`: `DataHelper`, `LopHocDAO`, `SinhVienDAO`, hai entity.
- `QuanLySinhVien.Business`: `LopHocBUS`, `SinhVienBUS`; ném ngoại lệ đối với dữ liệu sai; các lỗi DAL tự truyền lên.
- `QuanLySinhVien.View`: console View có menu CRUD và `try-catch`.
- `sql/01_QLSV_setup.sql`: tạo `QLSV`, bảng `LopHoc`, `SinhVien`, vài dòng dữ liệu mẫu nếu chưa tồn tại.

> Vì chưa được cung cấp Entity/LopHoc và cấu trúc bảng LopHoc thực tế, demo giả định bảng LopHoc chỉ có `MaLop`, `TenLop`. SinhVien.NgaySinh giữ kiểu `string` như dữ liệu code được gửi.

## Chạy trên Visual Studio (Windows)

1. Mở **SQL Server Management Studio (SSMS)** và Connect bằng Windows Authentication đến SQL Server instance đang chạy. Tên ví dụ: `LAPTOP-6SN65NCS\HUECTMDB`.
2. Nếu CSDL **QLSV đã có đủ bảng và dữ liệu**, không nhất thiết chạy script, nhưng kiểm tra cột cho khớp. Nếu chưa có, mở `sql/01_QLSV_setup.sql` trong SSMS, chọn **Execute (F5)** để tạo.
3. Trong Visual Studio 2022, **File → Open → Project/Solution**, chọn `QuanLySinhVien_ADONET_17102026.sln`.
4. Đảm bảo Visual Studio có **.NET 8 SDK** (nếu chưa có thì cài workload .NET desktop development hoặc SDK tương ứng).
5. Trong `QuanLySinhVien.Data/DAL/DataHelper.cs`, sửa giá trị `DefaultConnectionString` đúng tên SQL Server instance và database của bạn.
6. **Right click solution → Restore NuGet Packages** (hoặc Build sẽ tự restore), **Build → Build Solution** (`Ctrl+Shift+B`). Khi được yêu cầu, cho phép tải gói `Microsoft.Data.SqlClient`.
7. Right click `QuanLySinhVien.View` → **Set as Startup Project**.
8. Bấm `Ctrl+F5`, chọn `1` để xem lớp, `6` xem sinh viên, `7` thêm sinh viên, `8` sửa, `9` xóa.
9. Vào SSMS chạy `SELECT * FROM QLSV.dbo.SinhVien;` để xác nhận thao tác đã ghi xuống database.

## Test ngoại lệ theo yêu cầu

- Nhập menu `7`, dùng điểm 12: Business ném `ArgumentOutOfRangeException`, View bắt và hiện `LỖI: ...`.
- Thêm sinh viên đã tồn tại `MaSV`: SQL Server ném `SqlException` vi phạm khóa chính → DAL/BUS chuyển lên View.
- Nhập lớp không tồn tại trong lúc thêm sinh viên → SQL Server lỗi khóa ngoại (nếu bảng có FK).
- Xóa một lớp đang có sinh viên → SQL Server lỗi FK → View thông báo; lớp chưa bị xóa.
- Xóa sinh viên không có trong CSDL → DAO ném `KeyNotFoundException` → View thông báo.

## Nếu gắn vào project có sẵn của bạn

- Đảm bảo đã cài NuGet `Microsoft.Data.SqlClient`. Không dùng `MySql.Data.MySqlClient` hay `Mysqlx.Cursor` khi kết nối SQL Server.
- Giữ các Entity `SinhVien` và `LopHoc` hiện có; **không tạo trùng class**. Nếu tên/kiểu thuộc tính khác ví dụ thì sửa Map/param/SQL tương ứng.
- Dùng `DataHelper.cs`, `SinhVienDAO.cs`, `LopHocDAO.cs` từ folder Data/DAL; sửa namespace theo project.
- Nếu project đã có `SinhVienDAL` thì hoặc đổi tên thành `SinhVienDAO` và cập nhật nơi gọi, hoặc giữ tên cũ; tránh tạo ra hai class xử lý cùng mục đích mà không thống nhất.
- Ở Business gọi DAO, làm kiểm tra đầu vào và giữ nguyên cách ngoại lệ được truyền lên; ở WinForms View đặt `try-catch` trong click handler và `MessageBox.Show(ex.Message, "Lỗi")`.
- Dữ liệu thực có thể có thêm cột bắt buộc/kiểu khác: chỉnh các SQL theo bảng thực tế trước khi chạy.

## Đọc code như thế nào

- `DataHelper.GetDataTable`: SQL SELECT trả `DataTable`, tự giải phóng kết nối.
- `DataHelper.ExecuteNonQuery`: INSERT/UPDATE/DELETE, trả số hàng bị tác động.
- `SqlParameter`: truyền tham số an toàn hơn cách nối chuỗi SQL.
- `DAO`: CRUD trực tiếp SQL Server.
- `BUS`: validate, gọi DAO; không nuốt exception.
- `ConsoleView.Run`: `try-catch` ở tầng View.

**Chưa thực hiện git add / git commit.** Theo yêu cầu, để phần này làm sau. Gợi ý nội dung commit về sau: `Bài tập về ADO.Net ngày 17/10/2026`.

**Giới hạn kiểm thử:** bộ mã được chuẩn bị và rà soát cấu trúc, chưa kiểm thử chạy kết nối SQL Server thực tế vì môi trường tạo file không có SQL Server/.NET SDK.
