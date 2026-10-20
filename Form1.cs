using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace t5
{
    public partial class Form1 : Form
    {
        // UI fields and InitializeComponent are defined in Form1.Designer.cs (partial class)
        private List<SinhVien> students = new List<SinhVien>();

        // Cấu hình/secret đọc từ appsettings.json (và appsettings.local.json / biến môi trường),
        // không hard-code trong mã nguồn.
        private readonly DbConfig config = DbConfig.Load();

        // View chỉ gọi tầng Business; tầng Business/DAO ném ngoại lệ, View bắt và hiển thị.
        private readonly SinhVienBUS sinhVienBUS;
        private readonly LopHocBUS lopHocBUS;

        public Form1()
        {
            InitializeComponent();
            sinhVienBUS = new SinhVienBUS(config.AppConnectionString);
            lopHocBUS = new LopHocBUS(config.AppConnectionString);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Khởi tạo database/quyền/bảng (bất đồng bộ, await để không đóng băng UI)
            try
            {
                await DataHelper.EnsureSchemaAsync(
                    config.AdminConnectionString,
                    config.Database,
                    config.AppUser,
                    config.AppPassword);
            }
            catch (Exception ex)
            {
                // tầng dưới ném ngoại lệ, tầng View xử lý và tiếp tục với dữ liệu mẫu
                MessageBox.Show("Khởi tạo cơ sở dữ liệu thất bại (sẽ dùng dữ liệu mẫu): " + ex.Message,
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            await LoadClassesAsync();
            await LoadStudentsAsync();

            // defaults
            if (cboClass.Items.Count > 0) cboClass.SelectedIndex = 0;
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0;
            if (cboFilterClass.Items.Count > 0) cboFilterClass.SelectedIndex = 0;

            RefreshGrid();
        }

        private async Task LoadClassesAsync()
        {
            try
            {
                var lops = await lopHocBUS.GetAllAsync();
                if (lops == null || lops.Count == 0) return;

                cboClass.Items.Clear();
                cboFilterClass.Items.Clear();
                cboFilterClass.Items.Add("Tất cả lớp");
                foreach (var lh in lops)
                {
                    cboClass.Items.Add(lh.TenLop);
                    cboFilterClass.Items.Add(lh.TenLop);
                }
            }
            catch (Exception ex)
            {
                // giữ nguyên danh sách lớp mặc định của designer nếu không đọc được CSDL
                MessageBox.Show("Không tải được danh sách lớp học: " + ex.Message, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                var loaded = await sinhVienBUS.GetAllAsync();
                students = (loaded != null && loaded.Count > 0) ? loaded : SampleStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được danh sách sinh viên từ CSDL: " + ex.Message, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                students = SampleStudents();
            }
        }

        private static List<SinhVien> SampleStudents()
        {
            return new List<SinhVien>
            {
                new SinhVien("SV000123", "Nguyễn Văn An", new DateTime(2006, 8, 15), "Nam", "an.nv@vju.ac.vn", "0912345678", 8.5m, "Kỹ thuật phần mềm 01", "Đang học"),
                new SinhVien("SV000124", "Trần Minh Anh", new DateTime(2006, 1, 22), "Nữ", "anh.tm@vju.ac.vn", "0987654321", 9.0m, "Trí tuệ nhân tạo 01", "Đang học"),
                new SinhVien("SV000125", "Lê Hoàng Bình", new DateTime(2006, 5, 9), "Nam", "binh.lh@vju.ac.vn", "0355556677", 7.4m, "Kỹ thuật phần mềm 01", "Đang học")
            };
        }

        private void RefreshGrid(IEnumerable<SinhVien> source = null)
        {
            var list = source ?? students;
            dgvStudents.Rows.Clear();
            foreach (var s in list)
            {
                // first cell is Matched checkbox - default true when showing this list
                int idx = dgvStudents.Rows.Add(true, s.Id, s.FullName, s.Dob.ToString("dd/MM/yyyy"), s.Gender, s.Email, s.Phone, s.Score.ToString("0.0"), s.Lop, s.Status);
                dgvStudents.Rows[idx].Tag = s;
            }
            toolStripStatusLabel.Text = $"Tổng số: {dgvStudents.Rows.Count} sinh viên";
        }

        private void ClearInput()
        {
            txtStudentId.Text = "";
            txtFullName.Text = "";
            dtpDob.Value = DateTime.Today.AddYears(-18);
            rdoMale.Checked = true;
            txtEmail.Text = "";
            txtPhone.Text = "";
            if (cboClass.Items.Count > 0) cboClass.SelectedIndex = 0;
            nudScore.Value = 0;
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0;
        }

        private void PopulateInputs(SinhVien s)
        {
            if (s == null) return;
            txtStudentId.Text = s.Id;
            txtFullName.Text = s.FullName;
            dtpDob.Value = s.Dob;
            if (s.Gender == "Nữ") rdoFemale.Checked = true; else rdoMale.Checked = true;
            txtEmail.Text = s.Email;
            txtPhone.Text = s.Phone;
            cboClass.SelectedItem = s.Lop;
            nudScore.Value = Math.Max(nudScore.Minimum, Math.Min(nudScore.Maximum, s.Score));
            cboStatus.SelectedItem = s.Status;
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentId.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Mã sinh viên và Họ và tên là bắt buộc.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var s = new SinhVien(
                txtStudentId.Text.Trim(),
                txtFullName.Text.Trim(),
                dtpDob.Value.Date,
                rdoFemale.Checked ? "Nữ" : "Nam",
                txtEmail.Text.Trim(),
                txtPhone.Text.Trim(),
                nudScore.Value,
                cboClass.SelectedItem?.ToString() ?? string.Empty,
                cboStatus.SelectedItem?.ToString() ?? string.Empty
            );

            // persist to database via Business layer, then to in-memory list
            try
            {
                await sinhVienBUS.ThemSinhVienAsync(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm sinh viên thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            students.Add(s);
            RefreshGrid();
            // select newly added
            foreach (DataGridViewRow row in dgvStudents.Rows)
            {
                if (row.Tag == s) { row.Selected = true; dgvStudents.CurrentCell = row.Cells[0]; break; }
            }
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0) { MessageBox.Show("Chọn một sinh viên để sửa."); return; }
            var row = dgvStudents.SelectedRows[0];
            var s = row.Tag as SinhVien;
            if (s == null) return;

            var updated = new SinhVien(
                txtStudentId.Text.Trim(),
                txtFullName.Text.Trim(),
                dtpDob.Value.Date,
                rdoFemale.Checked ? "Nữ" : "Nam",
                txtEmail.Text.Trim(),
                txtPhone.Text.Trim(),
                nudScore.Value,
                cboClass.SelectedItem?.ToString() ?? string.Empty,
                cboStatus.SelectedItem?.ToString() ?? string.Empty
            );

            try
            {
                await sinhVienBUS.CapNhatSinhVienAsync(updated);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật sinh viên thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            s.Id = updated.Id;
            s.FullName = updated.FullName;
            s.Dob = updated.Dob;
            s.Gender = updated.Gender;
            s.Email = updated.Email;
            s.Phone = updated.Phone;
            s.Score = updated.Score;
            s.Lop = updated.Lop;
            s.Status = updated.Status;

            RefreshGrid();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0) { MessageBox.Show("Chọn một sinh viên để xóa."); return; }
            var row = dgvStudents.SelectedRows[0];
            var s = row.Tag as SinhVien;
            if (s == null) return;

            var res = MessageBox.Show($"Xóa sinh viên {s.FullName} ({s.Id})?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                try
                {
                    await sinhVienBUS.XoaSinhVienAsync(s.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xóa sinh viên thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                students.Remove(s);
                RefreshGrid();
                ClearInput();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ClearInput();
            dgvStudents.ClearSelection();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var kw = txtKeyword.Text?.Trim().ToLower() ?? string.Empty;
            var cls = cboFilterClass.SelectedItem?.ToString();
            var scoreFrom = nudFilterScoreFrom.Value;

            var filtered = students.Where(s =>
                (string.IsNullOrEmpty(kw) || s.Id.ToLower().Contains(kw) || s.FullName.ToLower().Contains(kw) || s.Email.ToLower().Contains(kw) || s.Phone.ToLower().Contains(kw))
                && (string.IsNullOrEmpty(cls) || cls == "Tất cả lớp" || s.Lop == cls)
                && s.Score >= scoreFrom
            );

            RefreshGrid(filtered);
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void dgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0) return;
            var row = dgvStudents.SelectedRows[0];
            var s = row.Tag as SinhVien;
            if (s != null) PopulateInputs(s);
        }

        // keep these handlers because the designer wires them
        private void txtEmail_TextChanged(object sender, EventArgs e) { }
        private void cboStatus_SelectedIndexChanged(object sender, EventArgs e) { }

        private void headerPanel_Paint(object sender, PaintEventArgs e)
        {
            // leave empty if you don't need custom painting
            // or add custom painting code here, e.g.:
            // using (var brush = new SolidBrush(headerPanel.BackColor)) { e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle); }
        }

        private void logoPictureBox_Click(object sender, EventArgs e) { }
        private void lblAppTitle_Click(object sender, EventArgs e) { }
        private void dgvStudents_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}
