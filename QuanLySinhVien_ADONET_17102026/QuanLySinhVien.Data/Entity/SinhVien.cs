namespace QuanLySinhVien.Data.Entity;

public class SinhVien
{
    public string MaSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    // Giữ kiểu string giống Entity của đoạn code được cung cấp.
    // Khi ghi SQL, DAL chuyển chuỗi thành DATE an toàn.
    public string NgaySinh { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string MaLop { get; set; } = string.Empty;
    public double Diem { get; set; }
    public string Email { get; set; } = string.Empty;
}
