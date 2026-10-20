using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace t5
{
    // Business Layer: kiểm tra dữ liệu rồi gọi DAO.
    // Mọi phương thức ném ngoại lệ ra ngoài để tầng View xử lý.
    public class LopHocBUS
    {
        private readonly LopHocDAO _dao;

        public LopHocBUS(string connectionString)
        {
            _dao = new LopHocDAO(connectionString);
        }

        public Task<List<LopHoc>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _dao.GetAllAsync(cancellationToken);
        }

        public Task<LopHoc?> GetByIdAsync(string maLop, CancellationToken cancellationToken = default)
        {
            return _dao.GetByIdAsync(maLop, cancellationToken);
        }

        public async Task ThemLopHocAsync(LopHoc lh, CancellationToken cancellationToken = default)
        {
            KiemTra(lh);
            if (await _dao.GetByIdAsync(lh.MaLop, cancellationToken).ConfigureAwait(false) != null)
            {
                throw new Exception($"Mã lớp '{lh.MaLop}' đã tồn tại.");
            }
            await _dao.InsertAsync(lh, cancellationToken).ConfigureAwait(false);
        }

        public async Task CapNhatLopHocAsync(LopHoc lh, CancellationToken cancellationToken = default)
        {
            KiemTra(lh);
            await _dao.UpdateAsync(lh, cancellationToken).ConfigureAwait(false);
        }

        public Task XoaLopHocAsync(string maLop, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(maLop))
            {
                throw new Exception("Mã lớp không hợp lệ.");
            }
            return _dao.DeleteAsync(maLop, cancellationToken);
        }

        private static void KiemTra(LopHoc lh)
        {
            if (lh == null) throw new ArgumentNullException(nameof(lh));
            if (string.IsNullOrWhiteSpace(lh.MaLop))
            {
                throw new Exception("Mã lớp là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(lh.TenLop))
            {
                throw new Exception("Tên lớp là bắt buộc.");
            }
        }
    }
}
