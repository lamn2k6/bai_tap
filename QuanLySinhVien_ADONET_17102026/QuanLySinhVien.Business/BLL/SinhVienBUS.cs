using System.Globalization;
using QuanLySinhVien.Data.DAL;
using QuanLySinhVien.Data.Entity;

namespace QuanLySinhVien.Business.BLL;

public class SinhVienBUS
{
    private readonly SinhVienDAO _dao = new();

    public List<SinhVien> GetAllSinhVien() => _dao.GetAllSinhVien();

    public SinhVien? GetSinhVienByMaSV(string maSV)
    {
        ValidateMaSV(maSV);
        return _dao.GetSinhVienByMaSV(maSV);
    }

    public List<SinhVien> GetSinhViensByMaLop(string maLop)
    {
        if (string.IsNullOrWhiteSpace(maLop))
            throw new ArgumentException("Mã lớp không được để trống.");
        return _dao.GetSinhViensByMaLop(maLop);
    }

    public void AddSinhVien(SinhVien sv)
    {
        Validate(sv);
        _dao.AddSinhVien(sv);
    }

    public void UpdateSinhVien(SinhVien sv)
    {
        Validate(sv);
        _dao.UpdateSinhVien(sv);
    }

    public void DeleteSinhVien(string maSV)
    {
        ValidateMaSV(maSV);
        _dao.DeleteSinhVien(maSV);
    }

    private static void ValidateMaSV(string maSV)
    {
        if (string.IsNullOrWhiteSpace(maSV))
            throw new ArgumentException("Mã sinh viên không được để trống.");
    }

    private static void Validate(SinhVien sv)
    {
        ArgumentNullException.ThrowIfNull(sv);
        ValidateMaSV(sv.MaSV);
        if (string.IsNullOrWhiteSpace(sv.HoTen))
            throw new ArgumentException("Họ tên không được để trống.");
        if (string.IsNullOrWhiteSpace(sv.MaLop))
            throw new ArgumentException("Mã lớp không được để trống.");
        if (!DateTime.TryParseExact(
                sv.NgaySinh,
                new[] { "yyyy-MM-dd", "dd/MM/yyyy" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
            throw new ArgumentException("Ngày sinh phải có dạng dd/MM/yyyy hoặc yyyy-MM-dd.");
        if (!double.IsFinite(sv.Diem) || sv.Diem < 0 || sv.Diem > 10)
            throw new ArgumentOutOfRangeException(nameof(sv.Diem), "Điểm phải từ 0 đến 10.");
    }
}
