using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace t5
{
    // Data Access Layer (DAO): thao tác bảng students bằng ADO.NET.
    // - Truy vấn tham số hóa (chống SQL injection) với kiểu dữ liệu tường minh.
    // - Bất đồng bộ (async) để không chặn UI.
    // - Mọi phương thức ném ngoại lệ ra ngoài cho tầng View xử lý.
    public class SinhVienDAO
    {
        private const string SelectColumns = "id, full_name, dob, gender, email, phone, score, lop, status";
        private readonly string _connectionString;

        public SinhVienDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<SinhVien>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var list = new List<SinhVien>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT {SelectColumns} FROM students;";
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                var columns = new Columns(reader);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    list.Add(Map(reader, columns));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đọc danh sách sinh viên từ cơ sở dữ liệu.", ex);
            }
            return list;
        }

        public async Task<List<SinhVien>> GetByLopAsync(string lop, CancellationToken cancellationToken = default)
        {
            var list = new List<SinhVien>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT {SelectColumns} FROM students WHERE lop = @lop;";
                cmd.Parameters.Add("@lop", MySqlDbType.VarChar, 200).Value = (object?)lop ?? DBNull.Value;
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                var columns = new Columns(reader);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    list.Add(Map(reader, columns));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc danh sách sinh viên của lớp '{lop}'.", ex);
            }
            return list;
        }

        public async Task<SinhVien?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT {SelectColumns} FROM students WHERE id = @id;";
                cmd.Parameters.Add("@id", MySqlDbType.VarChar, 50).Value = id;
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    return Map(reader, new Columns(reader));
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc sinh viên có mã '{id}'.", ex);
            }
        }

        public async Task InsertAsync(SinhVien sv, CancellationToken cancellationToken = default)
        {
            if (sv == null) throw new ArgumentNullException(nameof(sv));
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO students (id, full_name, dob, gender, email, phone, score, lop, status)
                                    VALUES (@id, @full_name, @dob, @gender, @email, @phone, @score, @lop, @status);";
                AddParameters(cmd, sv);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm sinh viên '{sv.Id}'.", ex);
            }
        }

        public async Task UpdateAsync(SinhVien sv, CancellationToken cancellationToken = default)
        {
            if (sv == null) throw new ArgumentNullException(nameof(sv));
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE students SET full_name = @full_name, dob = @dob, gender = @gender,
                                    email = @email, phone = @phone, score = @score, lop = @lop, status = @status
                                    WHERE id = @id;";
                AddParameters(cmd, sv);
                if (await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
                {
                    throw new Exception($"Không tìm thấy sinh viên có mã '{sv.Id}' để cập nhật.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật sinh viên '{sv.Id}'.", ex);
            }
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM students WHERE id = @id;";
                cmd.Parameters.Add("@id", MySqlDbType.VarChar, 50).Value = id;
                if (await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
                {
                    throw new Exception($"Không tìm thấy sinh viên có mã '{id}' để xóa.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa sinh viên '{id}'.", ex);
            }
        }

        private static void AddParameters(MySqlCommand cmd, SinhVien sv)
        {
            cmd.Parameters.Add("@id", MySqlDbType.VarChar, 50).Value = sv.Id;
            cmd.Parameters.Add("@full_name", MySqlDbType.VarChar, 200).Value = sv.FullName;
            cmd.Parameters.Add("@dob", MySqlDbType.Date).Value = sv.Dob;
            cmd.Parameters.Add("@gender", MySqlDbType.VarChar, 10).Value = sv.Gender;
            cmd.Parameters.Add("@email", MySqlDbType.VarChar, 200).Value = (object?)sv.Email ?? DBNull.Value;
            cmd.Parameters.Add("@phone", MySqlDbType.VarChar, 50).Value = (object?)sv.Phone ?? DBNull.Value;
            var score = cmd.Parameters.Add("@score", MySqlDbType.Decimal);
            score.Precision = 4;
            score.Scale = 2;
            score.Value = sv.Score;
            cmd.Parameters.Add("@lop", MySqlDbType.VarChar, 200).Value = (object?)sv.Lop ?? DBNull.Value;
            cmd.Parameters.Add("@status", MySqlDbType.VarChar, 100).Value = (object?)sv.Status ?? DBNull.Value;
        }

        private static SinhVien Map(MySqlDataReader reader, in Columns c)
        {
            return new SinhVien
            {
                Id = reader.IsDBNull(c.Id) ? string.Empty : reader.GetString(c.Id),
                FullName = reader.IsDBNull(c.FullName) ? string.Empty : reader.GetString(c.FullName),
                Dob = reader.IsDBNull(c.Dob) ? DateTime.MinValue : reader.GetDateTime(c.Dob),
                Gender = reader.IsDBNull(c.Gender) ? string.Empty : reader.GetString(c.Gender),
                Email = reader.IsDBNull(c.Email) ? string.Empty : reader.GetString(c.Email),
                Phone = reader.IsDBNull(c.Phone) ? string.Empty : reader.GetString(c.Phone),
                Score = reader.IsDBNull(c.Score) ? 0m : reader.GetDecimal(c.Score),
                Lop = reader.IsDBNull(c.Lop) ? string.Empty : reader.GetString(c.Lop),
                Status = reader.IsDBNull(c.Status) ? string.Empty : reader.GetString(c.Status)
            };
        }

        // Ordinal được tính một lần cho mỗi reader thay vì GetOrdinal lặp lại theo từng dòng.
        private readonly struct Columns
        {
            public readonly int Id, FullName, Dob, Gender, Email, Phone, Score, Lop, Status;

            public Columns(MySqlDataReader reader)
            {
                Id = reader.GetOrdinal("id");
                FullName = reader.GetOrdinal("full_name");
                Dob = reader.GetOrdinal("dob");
                Gender = reader.GetOrdinal("gender");
                Email = reader.GetOrdinal("email");
                Phone = reader.GetOrdinal("phone");
                Score = reader.GetOrdinal("score");
                Lop = reader.GetOrdinal("lop");
                Status = reader.GetOrdinal("status");
            }
        }
    }
}
