using System.Data;
using Microsoft.Data.SqlClient;
using QuanLySinhVien.Data.Entity;

namespace QuanLySinhVien.Data.DAL;

public class LopHocDAO
{
    private readonly DataHelper _db = new();

    public List<LopHoc> GetAllLopHoc()
    {
        var dt = _db.GetDataTable("SELECT MaLop, TenLop FROM dbo.LopHoc ORDER BY MaLop");
        return dt.Rows.Cast<DataRow>().Select(Map).ToList();
    }

    public LopHoc? GetLopHocByMaLop(string maLop)
    {
        var dt = _db.GetDataTable(
            "SELECT MaLop, TenLop FROM dbo.LopHoc WHERE MaLop = @MaLop",
            new SqlParameter("@MaLop", SqlDbType.VarChar, 20) { Value = maLop });
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    public void AddLopHoc(LopHoc lop)
    {
        _db.ExecuteNonQuery(
            "INSERT INTO dbo.LopHoc (MaLop, TenLop) VALUES (@MaLop, @TenLop)",
            new SqlParameter("@MaLop", SqlDbType.VarChar, 20) { Value = lop.MaLop },
            new SqlParameter("@TenLop", SqlDbType.NVarChar, 100) { Value = lop.TenLop });
    }

    public void UpdateLopHoc(LopHoc lop)
    {
        int rows = _db.ExecuteNonQuery(
            "UPDATE dbo.LopHoc SET TenLop = @TenLop WHERE MaLop = @MaLop",
            new SqlParameter("@TenLop", SqlDbType.NVarChar, 100) { Value = lop.TenLop },
            new SqlParameter("@MaLop", SqlDbType.VarChar, 20) { Value = lop.MaLop });
        if (rows == 0)
            throw new KeyNotFoundException($"Không tìm thấy lớp có mã {lop.MaLop}.");
    }

    public void DeleteLopHoc(string maLop)
    {
        int rows = _db.ExecuteNonQuery(
            "DELETE FROM dbo.LopHoc WHERE MaLop = @MaLop",
            new SqlParameter("@MaLop", SqlDbType.VarChar, 20) { Value = maLop });
        if (rows == 0)
            throw new KeyNotFoundException($"Không tìm thấy lớp có mã {maLop}.");
        // Nếu lớp đang chứa sinh viên, khóa ngoại sẽ sinh SqlException.
        // Không catch ở đây để View xử lý.
    }

    private static LopHoc Map(DataRow row) => new()
    {
        MaLop = row["MaLop"].ToString() ?? string.Empty,
        TenLop = row["TenLop"].ToString() ?? string.Empty
    };
}
