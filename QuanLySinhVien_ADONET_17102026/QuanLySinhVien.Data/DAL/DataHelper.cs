using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySinhVien.Data.DAL;

public sealed class DataHelper
{
    // SỬA Server cho khớp tên instance SQL Server của máy bạn.
    private const string DefaultConnectionString =
        @"Server=LAPTOP-6SN65NCS\HUECTMDB;Database=QLSV;Integrated Security=True;TrustServerCertificate=True;";

    private readonly string _connectionString;

    public DataHelper() : this(DefaultConnectionString) { }

    public DataHelper(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Chuỗi kết nối không được để trống.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public DataTable GetDataTable(string sql, params SqlParameter[] parameters)
    {
        using var con = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(cmd);
        var table = new DataTable();
        // DataAdapter.Fill sẽ tự mở và đóng kết nối nếu đang đóng.
        adapter.Fill(table);
        return table;
    }

    public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using var con = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        con.Open();
        return cmd.ExecuteNonQuery();
    }
}
