using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace t5
{
    // Data Access Layer (DAO): thao tác bảng lop bằng ADO.NET.
    // - Truy vấn tham số hóa, kiểu dữ liệu tường minh, bất đồng bộ.
    // - Mọi phương thức ném ngoại lệ ra ngoài cho tầng View xử lý.
    public class LopHocDAO
    {
        private const string SelectColumns = "malop, tenlop";
        private readonly string _connectionString;

        public LopHocDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<LopHoc>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var list = new List<LopHoc>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT {SelectColumns} FROM lop ORDER BY malop;";
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                var oMaLop = reader.GetOrdinal("malop");
                var oTenLop = reader.GetOrdinal("tenlop");
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    list.Add(Map(reader, oMaLop, oTenLop));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đọc danh sách lớp học từ cơ sở dữ liệu.", ex);
            }
            return list;
        }

        public async Task<LopHoc?> GetByIdAsync(string maLop, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT {SelectColumns} FROM lop WHERE malop = @malop;";
                cmd.Parameters.Add("@malop", MySqlDbType.VarChar, 50).Value = maLop;
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    return Map(reader, reader.GetOrdinal("malop"), reader.GetOrdinal("tenlop"));
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc lớp học có mã '{maLop}'.", ex);
            }
        }

        public async Task InsertAsync(LopHoc lh, CancellationToken cancellationToken = default)
        {
            if (lh == null) throw new ArgumentNullException(nameof(lh));
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO lop (malop, tenlop) VALUES (@malop, @tenlop);";
                cmd.Parameters.Add("@malop", MySqlDbType.VarChar, 50).Value = lh.MaLop;
                cmd.Parameters.Add("@tenlop", MySqlDbType.VarChar, 200).Value = lh.TenLop;
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm lớp học '{lh.MaLop}'.", ex);
            }
        }

        public async Task UpdateAsync(LopHoc lh, CancellationToken cancellationToken = default)
        {
            if (lh == null) throw new ArgumentNullException(nameof(lh));
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE lop SET tenlop = @tenlop WHERE malop = @malop;";
                cmd.Parameters.Add("@malop", MySqlDbType.VarChar, 50).Value = lh.MaLop;
                cmd.Parameters.Add("@tenlop", MySqlDbType.VarChar, 200).Value = lh.TenLop;
                if (await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
                {
                    throw new Exception($"Không tìm thấy lớp học có mã '{lh.MaLop}' để cập nhật.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật lớp học '{lh.MaLop}'.", ex);
            }
        }

        public async Task DeleteAsync(string maLop, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM lop WHERE malop = @malop;";
                cmd.Parameters.Add("@malop", MySqlDbType.VarChar, 50).Value = maLop;
                if (await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
                {
                    throw new Exception($"Không tìm thấy lớp học có mã '{maLop}' để xóa.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa lớp học '{maLop}'.", ex);
            }
        }

        private static LopHoc Map(MySqlDataReader reader, int oMaLop, int oTenLop)
        {
            return new LopHoc
            {
                MaLop = reader.IsDBNull(oMaLop) ? string.Empty : reader.GetString(oMaLop),
                TenLop = reader.IsDBNull(oTenLop) ? string.Empty : reader.GetString(oTenLop)
            };
        }
    }
}
