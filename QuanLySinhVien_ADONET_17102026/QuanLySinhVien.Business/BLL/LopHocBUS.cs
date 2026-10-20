using QuanLySinhVien.Data.DAL;
using QuanLySinhVien.Data.Entity;

namespace QuanLySinhVien.Business.BLL;

public class LopHocBUS
{
    private readonly LopHocDAO _dao = new();

    public List<LopHoc> GetAllLopHoc() => _dao.GetAllLopHoc();

    public LopHoc? GetLopHocByMaLop(string maLop)
    {
        ValidateMaLop(maLop);
        return _dao.GetLopHocByMaLop(maLop);
    }

    public void AddLopHoc(LopHoc lop)
    {
        Validate(lop);
        _dao.AddLopHoc(lop);
    }

    public void UpdateLopHoc(LopHoc lop)
    {
        Validate(lop);
        _dao.UpdateLopHoc(lop);
    }

    public void DeleteLopHoc(string maLop)
    {
        ValidateMaLop(maLop);
        _dao.DeleteLopHoc(maLop);
    }

    private static void ValidateMaLop(string maLop)
    {
        if (string.IsNullOrWhiteSpace(maLop))
            throw new ArgumentException("Mã lớp không được để trống.");
    }

    private static void Validate(LopHoc lop)
    {
        ArgumentNullException.ThrowIfNull(lop);
        ValidateMaLop(lop.MaLop);
        if (string.IsNullOrWhiteSpace(lop.TenLop))
            throw new ArgumentException("Tên lớp không được để trống.");
    }
}
