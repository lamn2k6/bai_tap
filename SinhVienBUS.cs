using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace t5
{
    // Business Layer: kiểm tra dữ liệu rồi gọi DAO.
    // Mọi phương thức ném ngoại lệ ra ngoài để tầng View xử lý.
    public class SinhVienBUS
    {
        private readonly SinhVienDAO _dao;

        public SinhVienBUS(string connectionString)
        {
            _dao = new SinhVienDAO(connectionString);
        }

        public Task<List<SinhVien>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _dao.GetAllAsync(cancellationToken);
        }

        public Task<List<SinhVien>> GetByLopAsync(string lop, CancellationToken cancellationToken = default)
        {
            return _dao.GetByLopAsync(lop, cancellationToken);
        }

        public Task<SinhVien?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return _dao.GetByIdAsync(id, cancellationToken);
        }

        public async Task ThemSinhVienAsync(SinhVien sv, CancellationToken cancellationToken = default)
        {
            KiemTra(sv);
            if (await _dao.GetByIdAsync(sv.Id, cancellationToken).ConfigureAwait(false) != null)
            {
                throw new Exception($"Mã sinh viên '{sv.Id}' đã tồn tại.");
            }
            await _dao.InsertAsync(sv, cancellationToken).ConfigureAwait(false);
        }

        public async Task CapNhatSinhVienAsync(SinhVien sv, CancellationToken cancellationToken = default)
        {
            KiemTra(sv);
            await _dao.UpdateAsync(sv, cancellationToken).ConfigureAwait(false);
        }

        public Task XoaSinhVienAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new Exception("Mã sinh viên không hợp lệ.");
            }
            return _dao.DeleteAsync(id, cancellationToken);
        }

        private static void KiemTra(SinhVien sv)
        {
            if (sv == null) throw new ArgumentNullException(nameof(sv));
            if (!sv.Validate(out var results))
            {
                throw new Exception(string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage)));
            }
        }
    }
}
