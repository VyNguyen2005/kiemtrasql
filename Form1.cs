using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnTapKT
{
    public partial class Form1 : Form
    {
        msQLSV ds = new msQLSV();
        msQLSVTableAdapters.SINHVIENTableAdapter adpSinhVien = new msQLSVTableAdapters.SINHVIENTableAdapter();
        msQLSVTableAdapters.KHOATableAdapter adpKhoa = new msQLSVTableAdapters.KHOATableAdapter();
        msQLSVTableAdapters.MONHOCTableAdapter adpMonHoc = new msQLSVTableAdapters.MONHOCTableAdapter();
        msQLSVTableAdapters.KETQUATableAdapter adpKetQua = new msQLSVTableAdapters.KETQUATableAdapter();
        BindingSource bsSV = new BindingSource();
        BindingSource bsKhoa = new BindingSource();
        int stt = -1;
        public Form1()
        {
            InitializeComponent();
            bsKhoa.CurrentChanged += BsKhoa_CurrentChanged;
        }

        private void BsKhoa_CurrentChanged(object sender, EventArgs e)
        {
            bdnSinhVien.BindingSource = bsKhoa;
            lblSTT.Text = bsKhoa.Position + 1 + "/" + bsKhoa.Count;
            if(bsKhoa.Current != null)
            {
                if(bsKhoa.Current is DataRowView rowView)
                {
                    string maKH = rowView["MaKH"].ToString();
                    txtMaKH.Text = maKH;

                    txtTongSV.Text = TinhTongSV(maKH).ToString();
                }
            }
            btnTruoc.Enabled = bsKhoa.Position > 0;
            btnSau.Enabled = bsKhoa.Position < bsKhoa.Count - 1;
        }

        private object TinhTongSV(string maKH)
        {
            int kq;
            Object tssv = ds.Tables["SINHVIEN"].Compute("count(MaSV)", "MaKH='" + maKH + "'");
            if (tssv == DBNull.Value)
                kq = 0;
            else
                kq = Convert.ToInt32(tssv);
            return kq;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Doc_Du_Lieu();
            Lien_Ket_Dieu_Khien();
        }

        private void Lien_Ket_Dieu_Khien()
        {
            foreach (Control control in this.Controls)
            {
                if(control is TextBox && control.Name != "txtTongSV")
                {
                    control.DataBindings.Add("text", bsKhoa, control.Name.Substring(3), true);
                }
            }
        }

        private void Doc_Du_Lieu()
        {
            // 1. Nạp dữ liệu cho DataTable
            adpSinhVien.Fill(ds.SINHVIEN);
            adpKhoa.Fill(ds.KHOA);
            adpMonHoc.Fill(ds.MONHOC);
            adpKetQua.Fill(ds.KETQUA);

            // 2. Nạp nguồn dữ liệu cho BindingSource bsKhoa
            bsKhoa.DataSource = ds.KHOA;

            // 3. Nạp nguồn dữ liệu cho BindingSource bsSV
            bsSV.DataSource = bsKhoa;
            bsSV.DataMember = "KHOASINHVIEN";

            // 4. Gán nguồn cho BindingSource bsSV
            if (ds.SINHVIEN.Rows.Count > 0)
            {
                dgvSinhVien.DataSource = bsSV;
            }

            // 5. Không hiển thị cột MaKH trong lưới
            dgvSinhVien.Columns["MaKH"].Visible = false;

        }
        private void btnTruoc_Click(object sender, EventArgs e)
        {
            bsKhoa.MovePrevious();
        }

        private void btnSau_Click(object sender, EventArgs e)
        {
            bsKhoa.MoveNext();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            txtMaKH.ReadOnly = false;
            stt = bsKhoa.Position;
            bsKhoa.AddNew();
            txtMaKH.Focus();
        }

        private void btnKhong_Click(object sender, EventArgs e)
        {
            bsKhoa.CancelEdit();
            txtMaKH.ReadOnly = true;
            bsKhoa.Position = stt;
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if(txtMaKH.ReadOnly == false)
            {
                msQLSV.KHOARow rKH = ds.KHOA.FindByMaKH(txtMaKH.Text);
                if(rKH != null)
                {
                    MessageBox.Show("Mã khoa bị trùng. Vui lòng nhập lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMaKH.Clear();
                    txtMaKH.Focus();
                    return;
                }
            }
            txtMaKH.ReadOnly = true;
            bsKhoa.EndEdit();
            int n = adpKhoa.Update(ds.KHOA);
            if(n > 0)
            {
                MessageBox.Show("Cập nhật (THÊM/SỬA) thành công!");
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            msQLSV.KHOARow rKhoa = (bsKhoa.Current as DataRowView).Row as msQLSV.KHOARow;
            if(rKhoa.GetSINHVIENRows().Length > 0)
            {
                MessageBox.Show("Bạn không thể huỷ khoa này vì đã có sinh viên học!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult tl = MessageBox.Show("Bạn có chắc chắn muốn huỷ khoa:" + "\r\n" +
                " + Mã khoa: " + txtMaKH.Text + "\r\n" +
                " + Tên khoa: " + txtTenKH.Text + ' ' +
                "này không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(tl == DialogResult.Yes)
            {
                bsKhoa.RemoveCurrent();
                int n = adpKhoa.Update(ds.KHOA);
                if(n > 0)
                {
                    MessageBox.Show("Hủy khoa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void dgvSinhVien_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dgvSinhVien.CurrentRow.IsNewRow == true) return;

            if (dgvSinhVien.IsCurrentRowDirty == true)
            {
                if ((dgvSinhVien.CurrentRow.DataBoundItem as DataRowView).IsNew == true)
                {
                    if (ds.SINHVIEN.FindByMaSV(dgvSinhVien.CurrentRow.Cells["colMaSV"].Value?.ToString()) != null)

                    {
                        MessageBox.Show("Mã sinh viên này đã tồn tại trong khoa. Vui lòng chọn mã sinh viên khác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        dgvSinhVien.CurrentCell = dgvSinhVien.CurrentRow.Cells["colMaSV"];

                        dgvSinhVien.CurrentRow.Cells["colMaSV"].Value = null;

                        dgvSinhVien.CurrentCell = dgvSinhVien.CurrentRow.Cells["colMaSV"];
                        return;

                    }
                }
                (dgvSinhVien.CurrentRow.DataBoundItem as DataRowView).EndEdit();
                int n = adpSinhVien.Update(ds.SINHVIEN);
                if (n > 0)
                {
                    MessageBox.Show("Cập nhật sinh viên cho khoa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } 
            }
        }
    }
}
