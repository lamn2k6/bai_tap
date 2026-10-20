using System;
using System.IO;
using System.Text.Json;
using MySqlConnector;

namespace t5
{
    // Cấu hình kết nối. Thứ tự ưu tiên (sau ghi đè trước):
    //   1. Giá trị mặc định
    //   2. appsettings.json
    //   3. appsettings.local.json  (không đưa lên source control - chứa mật khẩu thật)
    //   4. Biến môi trường BAITAP10_DB_*  (khuyến nghị cho môi trường thật/CI)
    // Nhờ đó mã nguồn không cần hard-code mật khẩu.
    public sealed class DbConfig
    {
        public string Server { get; set; } = "127.0.0.1";
        public uint Port { get; set; } = 3306;
        public string Database { get; set; } = "mydb";
        public string AdminUser { get; set; } = "admin";
        public string AdminPassword { get; set; } = string.Empty;
        public string AppUser { get; set; } = "myuser";
        public string AppPassword { get; set; } = string.Empty;
        public string SslMode { get; set; } = "Preferred";
        public uint ConnectionTimeout { get; set; } = 10;

        public string AdminConnectionString => BuildConnectionString(AdminUser, AdminPassword, null);
        public string AppConnectionString => BuildConnectionString(AppUser, AppPassword, Database);

        public static DbConfig Load()
        {
            var config = new DbConfig();
            ApplyFile(config, "appsettings.json");
            ApplyFile(config, "appsettings.local.json");
            ApplyEnvironment(config);
            return config;
        }

        private static void ApplyFile(DbConfig config, string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, fileName);
            if (!File.Exists(path)) return;

            using var doc = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            if (!doc.RootElement.TryGetProperty("Db", out var db) || db.ValueKind != JsonValueKind.Object) return;

            foreach (var property in db.EnumerateObject())
            {
                switch (property.Name.ToLowerInvariant())
                {
                    case "server": config.Server = property.Value.GetString() ?? config.Server; break;
                    case "port": if (property.Value.TryGetUInt32(out var port)) config.Port = port; break;
                    case "database": config.Database = property.Value.GetString() ?? config.Database; break;
                    case "adminuser": config.AdminUser = property.Value.GetString() ?? config.AdminUser; break;
                    case "adminpassword": config.AdminPassword = property.Value.GetString() ?? config.AdminPassword; break;
                    case "appuser": config.AppUser = property.Value.GetString() ?? config.AppUser; break;
                    case "apppassword": config.AppPassword = property.Value.GetString() ?? config.AppPassword; break;
                    case "sslmode": config.SslMode = property.Value.GetString() ?? config.SslMode; break;
                    case "connectiontimeout": if (property.Value.TryGetUInt32(out var timeout)) config.ConnectionTimeout = timeout; break;
                }
            }
        }

        private static void ApplyEnvironment(DbConfig config)
        {
            string Get(string key) => Environment.GetEnvironmentVariable("BAITAP10_DB_" + key);

            var server = Get("SERVER"); if (!string.IsNullOrWhiteSpace(server)) config.Server = server;
            var port = Get("PORT"); if (uint.TryParse(port, out var p)) config.Port = p;
            var database = Get("DATABASE"); if (!string.IsNullOrWhiteSpace(database)) config.Database = database;
            var adminUser = Get("ADMINUSER"); if (!string.IsNullOrWhiteSpace(adminUser)) config.AdminUser = adminUser;
            var adminPassword = Get("ADMINPASSWORD"); if (adminPassword != null) config.AdminPassword = adminPassword;
            var appUser = Get("APPUSER"); if (!string.IsNullOrWhiteSpace(appUser)) config.AppUser = appUser;
            var appPassword = Get("APPPASSWORD"); if (appPassword != null) config.AppPassword = appPassword;
            var sslMode = Get("SSLMODE"); if (!string.IsNullOrWhiteSpace(sslMode)) config.SslMode = sslMode;
        }

        private string BuildConnectionString(string user, string password, string database)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = Server,
                Port = Port,
                UserID = user,
                Password = password,
                ConnectionTimeout = ConnectionTimeout,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 50,
                AllowPublicKeyRetrieval = true,
                SslMode = ParseSslMode(SslMode)
            };
            if (!string.IsNullOrEmpty(database)) builder.Database = database;
            return builder.ConnectionString;
        }

        private static MySqlSslMode ParseSslMode(string value)
        {
            return Enum.TryParse<MySqlSslMode>(value, ignoreCase: true, out var mode) ? mode : MySqlSslMode.Preferred;
        }
    }
}
