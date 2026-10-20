using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace t5
{
    // Khởi tạo schema bằng tài khoản admin. Tài khoản ứng dụng chỉ được cấp quyền DML tối thiểu.
    public static class DataHelper
    {
        private static readonly Regex IdentifierPattern = new("^[A-Za-z0-9_]{1,64}$", RegexOptions.Compiled);

        public static async Task EnsureSchemaAsync(
            string adminConnectionString,
            string dbName,
            string appUser,
            string appPassword,
            CancellationToken cancellationToken = default)
        {
            if (!IdentifierPattern.IsMatch(dbName))
                throw new ArgumentException($"Tên database không hợp lệ: '{dbName}'.", nameof(dbName));
            if (!IdentifierPattern.IsMatch(appUser))
                throw new ArgumentException($"Tên người dùng không hợp lệ: '{appUser}'.", nameof(appUser));

            var safePassword = EscapeSqlLiteral(appPassword ?? string.Empty);

            await using var conn = new MySqlConnection(adminConnectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await ExecuteAsync(conn, $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", cancellationToken).ConfigureAwait(false);

            // Tạo lại tài khoản ứng dụng với đúng quyền DML tối thiểu (loại bỏ quyền thừa nếu có),
            // cho cả 'localhost' và '127.0.0.1' (tùy cấu hình phân giải tên miền của MySQL).
            foreach (var host in new[] { "localhost", "127.0.0.1" })
            {
                await ExecuteAsync(conn, $"DROP USER IF EXISTS '{appUser}'@'{host}';", cancellationToken).ConfigureAwait(false);
                await ExecuteAsync(conn, $"CREATE USER '{appUser}'@'{host}' IDENTIFIED BY '{safePassword}';", cancellationToken).ConfigureAwait(false);
                await ExecuteAsync(conn, $"GRANT SELECT, INSERT, UPDATE, DELETE ON `{dbName}`.* TO '{appUser}'@'{host}';", cancellationToken).ConfigureAwait(false);
            }
            await ExecuteAsync(conn, "FLUSH PRIVILEGES;", cancellationToken).ConfigureAwait(false);

            await ExecuteAsync(conn, $"USE `{dbName}`;", cancellationToken).ConfigureAwait(false);

            await ExecuteAsync(conn, @"CREATE TABLE IF NOT EXISTS lop (
                malop VARCHAR(50) NOT NULL PRIMARY KEY,
                tenlop VARCHAR(200) NOT NULL
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", cancellationToken).ConfigureAwait(false);

            await ExecuteAsync(conn, @"INSERT IGNORE INTO lop (malop, tenlop) VALUES
                ('L01', 'Kỹ thuật phần mềm 01'),
                ('L02', 'Trí tuệ nhân tạo 01'),
                ('L03', 'Khoa học dữ liệu 01');", cancellationToken).ConfigureAwait(false);

            await ExecuteAsync(conn, @"CREATE TABLE IF NOT EXISTS students (
                id VARCHAR(50) NOT NULL PRIMARY KEY,
                full_name VARCHAR(200) NOT NULL,
                dob DATE NOT NULL,
                gender VARCHAR(10) NOT NULL,
                email VARCHAR(200) NULL,
                phone VARCHAR(50) NULL,
                score DECIMAL(4,2) NULL,
                lop VARCHAR(200) NULL,
                status VARCHAR(100) NULL,
                INDEX idx_students_lop (lop)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", cancellationToken).ConfigureAwait(false);

            // Với database đã tồn tại trước đó, bảo đảm chỉ mục tra cứu theo lớp tồn tại.
            var indexExists = Convert.ToInt64(await ScalarAsync(
                conn,
                "SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'students' AND INDEX_NAME = 'idx_students_lop';",
                cancellationToken,
                ("@db", dbName)).ConfigureAwait(false));

            if (indexExists == 0)
            {
                await ExecuteAsync(conn, "ALTER TABLE `students` ADD INDEX idx_students_lop (lop);", cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task ExecuteAsync(MySqlConnection conn, string sql, CancellationToken cancellationToken)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static async Task<object?> ScalarAsync(MySqlConnection conn, string sql, CancellationToken cancellationToken, params (string Name, object Value)[] parameters)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var (name, value) in parameters)
            {
                cmd.Parameters.AddWithValue(name, value);
            }
            return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }

        private static string EscapeSqlLiteral(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "''");
        }
    }
}
