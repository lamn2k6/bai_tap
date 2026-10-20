using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using QuanLySinhVien.Data.Entity;

namespace QuanLySinhVien.Data.DAL;

public class SinhVienDAO
{
    private readonly DataHelper _db = new();
    private const string Columns =
        "MaSV, MaLop, HoTen, GioiTinh, NgaySinh, Diem, SoDienThoai, Email";

    public List<SinhVien> GetAllSinhVien()
    {
        var dt = _db.GetDataTable($"SELECT {Columns} FROM dbo.SinhVien ORDER BY MaSV");
        return dt.Rows.Cast<DataRow>().Select(Map).ToList();
    }

    public SinhVien? GetSinhVienByMaSV(string maSV)
    {
        var dt = _db.GetDataTable(
            $"SELECT {Columns} FROM dbo.SinhVien WHERE MaSV = @MaSV",
            new SqlParameter("@MaSV", SqlDbType.VarChar, 20) { Value = maSV });
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    public List<SinhVien> GetSinhViensByMaLop(string maLop)
    {
        var dt = _db.GetDataTable(
            $"SELECT {Columns} FROM dbo.SinhVien WHERE MaLop = @MaLop ORDER BY MaSV",
            new SqlParameter("@MaLop", SqlDbType.VarChar, 20) { Value = maLop });
        return dt.Rows.Cast<DataRow>().Select(Map).ToList();
    }

    public void AddSinhVien(SinhVien sv)
    {
        _db.ExecuteNonQuery(
            """
            INSERT INTO dbo.SinhVien
                (MaSV, MaLop, HoTen, GioiTinh, NgaySinh, Diem, SoDienThoai, Email)
            VALUES
                (@MaSV, @MaLop, @HoTen, @GioiTinh, @NgaySinh, @Diem, @SoDienThoai, @Email)
            """,
            BuildParameters(sv));
    }

    public void UpdateSinhVien(SinhVien sv)
    {
        int rows = _db.ExecuteNonQuery(
            """
            UPDATE dbo.SinhVien
            SET MaLop = @MaLop, HoTen = @HoTen, GioiTinh = @GioiTinh,
                NgaySinh = @NgaySinh, Diem = @Diem,
                SoDienThoai = @SoDienThoai, Email = @Email
            WHERE MaSV = @MaSV
            """,
            BuildParameters(sv));
        if (rows == 0)
            throw new KeyNotFoundException($"Không tìm thấy sinh viên có mã {sv.MaSV}.");
    }

    public void DeleteSinhVien(string maSV)
    {
        int rows = _db.ExecuteNonQuery(
            "DELETE FROM dbo.SinhVien WHERE MaSV = @MaSV",
            new SqlParameter("@MaSV", SqlDbType.VarChar, 20) { Value = maSV });
        if (rows == 0)
            throw new KeyNotFoundException($"Không tìm thấy sinh viên có mã {maSV}.");
    }

    private static SqlParameter[] BuildParameters(SinhVien sv)
    {
        DateTime ngaySinh = DateTime.ParseExact(
            sv.NgaySinh.Trim(),
            new[] { "yyyy-MM-dd", "dd/MM/yyyy" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);

        return new[]
        {
            new SqlParameter("@MaSV", SqlDbType.VarChar, 20) { Value = sv.MaSV },
            new SqlParameter("@MaLop", SqlDbType.VarChar, 20) { Value = sv.MaLop },
            new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = sv.HoTen },
            new SqlParameter("@GioiTinh", SqlDbType.NVarChar, 10) { Value = sv.GioiTinh },
            new SqlParameter("@NgaySinh", SqlDbType.Date) { Value = ngaySinh },
            new SqlParameter("@Diem", SqlDbType.Float) { Value = sv.Diem },
            new SqlParameter("@SoDienThoai", SqlDbType.VarChar, 20) { Value = sv.SoDienThoai },
            new SqlParameter("@Email", SqlDbType.VarChar, 150) { Value = sv.Email }
        };
    }

    private static SinhVien Map(DataRow row) => new()
    {
        MaSV = row["MaSV"].ToString() ?? string.Empty,
        MaLop = row["MaLop"].ToString() ?? string.Empty,
        HoTen = row["HoTen"].ToString() ?? string.Empty,
        GioiTinh = row["GioiTinh"].ToString() ?? string.Empty,
        NgaySinh = Convert.ToDateTime(row["NgaySinh"], CultureInfo.InvariantCulture)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        Diem = Convert.ToDouble(row["Diem"], CultureInfo.InvariantCulture),
        SoDienThoai = row["SoDienThoai"].ToString() ?? string.Empty,
        Email = row["Email"].ToString() ?? string.Empty
    };
}
