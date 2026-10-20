using System.Globalization;
using QuanLySinhVien.Business.BLL;
using QuanLySinhVien.Data.Entity;

namespace QuanLySinhVien.View;

public class ConsoleView
{
    private readonly LopHocBUS _lopHoc = new();
    private readonly SinhVienBUS _sinhVien = new();

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n========== QUẢN LÝ SINH VIÊN (ADO.NET) ==========");
            Console.WriteLine("1. Danh sách lớp         2. Thêm lớp");
            Console.WriteLine("3. Sửa lớp               4. Xóa lớp");
            Console.WriteLine("5. Tìm lớp theo mã       6. Danh sách sinh viên");
            Console.WriteLine("7. Thêm sinh viên        8. Sửa sinh viên");
            Console.WriteLine("9. Xóa sinh viên        10. Tìm sinh viên theo mã");
            Console.WriteLine("11. Sinh viên theo lớp    0. Thoát");
            Console.Write("Chọn: ");
            string choice = Console.ReadLine() ?? string.Empty;
            if (choice == "0") return;

            // VIEW chịu trách nhiệm BẮT ngoại lệ từ Business và DAL.
            try
            {
                switch (choice)
                {
                    case "1": ShowClasses(_lopHoc.GetAllLopHoc()); break;
                    case "2": _lopHoc.AddLopHoc(ReadLopHoc()); Success(); break;
                    case "3": _lopHoc.UpdateLopHoc(ReadLopHoc()); Success(); break;
                    case "4": _lopHoc.DeleteLopHoc(Read("Mã lớp cần xóa")); Success(); break;
                    case "5":
                        var lop = _lopHoc.GetLopHocByMaLop(Read("Mã lớp"));
                        ShowClasses(lop == null ? new List<LopHoc>() : new List<LopHoc> { lop });
                        break;
                    case "6": ShowStudents(_sinhVien.GetAllSinhVien()); break;
                    case "7": _sinhVien.AddSinhVien(ReadSinhVien()); Success(); break;
                    case "8": _sinhVien.UpdateSinhVien(ReadSinhVien()); Success(); break;
                    case "9": _sinhVien.DeleteSinhVien(Read("Mã sinh viên cần xóa")); Success(); break;
                    case "10":
                        var sv = _sinhVien.GetSinhVienByMaSV(Read("Mã sinh viên"));
                        ShowStudents(sv == null ? new List<SinhVien>() : new List<SinhVien> { sv });
                        break;
                    case "11":
                        ShowStudents(_sinhVien.GetSinhViensByMaLop(Read("Mã lớp")));
                        break;
                    default: Console.WriteLine("Lựa chọn không hợp lệ."); break;
                }
            }
            catch (Exception ex)
            {
                // Bắt tập trung, để một thao tác lỗi không làm ứng dụng dừng.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("LỖI: " + ex.Message);
                Console.ResetColor();
            }
        }
    }

    private static LopHoc ReadLopHoc() => new()
    {
        MaLop = Read("Mã lớp"),
        TenLop = Read("Tên lớp")
    };

    private static SinhVien ReadSinhVien()
    {
        var sv = new SinhVien
        {
            MaSV = Read("Mã sinh viên"),
            MaLop = Read("Mã lớp"),
            HoTen = Read("Họ tên"),
            GioiTinh = Read("Giới tính"),
            NgaySinh = Read("Ngày sinh (dd/MM/yyyy)"),
            SoDienThoai = Read("Số điện thoại"),
            Email = Read("Email")
        };
        string input = Read("Điểm (0-10)").Replace(',', '.');
        if (!double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double diem))
            throw new ArgumentException("Điểm phải là một số hợp lệ.");
        sv.Diem = diem;
        return sv;
    }

    private static string Read(string prompt)
    {
        Console.Write(prompt + ": ");
        return (Console.ReadLine() ?? string.Empty).Trim();
    }

    private static void ShowClasses(List<LopHoc> classes)
    {
        if (classes.Count == 0) { Console.WriteLine("Không có lớp học."); return; }
        foreach (var item in classes)
            Console.WriteLine($"{item.MaLop,-12} | {item.TenLop}");
    }

    private static void ShowStudents(List<SinhVien> students)
    {
        if (students.Count == 0) { Console.WriteLine("Không có sinh viên."); return; }
        foreach (var sv in students)
        {
            Console.WriteLine($"{sv.MaSV} | {sv.HoTen} | Lớp: {sv.MaLop} | " +
                $"Giới tính: {sv.GioiTinh} | Ngày sinh: {sv.NgaySinh} | Điểm: {sv.Diem} | " +
                $"SĐT: {sv.SoDienThoai} | Email: {sv.Email}");
        }
    }

    private static void Success()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Thao tác thành công.");
        Console.ResetColor();
    }
}
