using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assignment3
{
    public partial class frmMain : Form
    {
        SqlDataAdapter adapter;
        DataSet data;
        SqlConnection connection;
        SqlCommand command;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Ket noi den csdl
            string connectionString = "server=BAOLHQ\\SQLEXPRESS;database=Assignment3;uid=sa;pwd=123456789aaaaaa";
            connection = new SqlConnection(connectionString);
            connection.Open();

            ReloadGridView();
        }

        public void ReloadGridView()
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //Thiet lap truy van du lieu
            string sql = "select * from clients";
            command = new SqlCommand();
            command.CommandText = sql;
            command.Connection = connection;

            //Tao container cho data truy van duoc
            data = new DataSet();

            //Thuc thi truy van va do du lieu vao dataset
            adapter = new SqlDataAdapter(command);

            adapter.Fill(data, "clients");

            dgvClient.DataSource = data;
            dgvClient.DataMember = "clients";
        }

        private void dgvClient_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                txtCode.Text = dgvClient.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtName.Text = dgvClient.Rows[e.RowIndex].Cells[1].Value.ToString();
                dtpBirthday.Text = dgvClient.Rows[e.RowIndex].Cells[2].Value.ToString();
                txtAddress.Text = dgvClient.Rows[e.RowIndex].Cells[3].Value.ToString();
                txtPhone.Text = dgvClient.Rows[e.RowIndex].Cells[4].Value.ToString();
                txtEmail.Text = dgvClient.Rows[e.RowIndex].Cells[5].Value.ToString();

                Image image = null;
                try
                {
                    var fileUri = dgvClient.Rows[e.RowIndex].Cells[6].Value.ToString();

                    if (!string.IsNullOrEmpty(fileUri) && !File.Exists(fileUri))
                        MessageBox.Show($"Ảnh của người dùng này không tồn tại, vui lòng cập nhật đường dẫn.\n\nKhông tìm thấy file tại: {fileUri}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (string.IsNullOrEmpty(fileUri))
                        image = Image.FromFile(Directory.GetCurrentDirectory() + "//user.png");
                    else
                        image = Image.FromFile(fileUri);
                }
                catch (FileNotFoundException exception)
                {
                    Console.WriteLine(exception.Message);
                }

                if (image != null)
                    pbPicture.Image = image;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        void ClearForm()
        {
            txtCode.Text = "";
            txtName.Text = "";
            dtpBirthday.Text = "";
            txtAddress.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            pbPicture.Image = null;
            pbPicture.ImageLocation = "";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!IsFormFullyFilled())
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (connection.State == ConnectionState.Closed)
                connection.Open();
            string sql = "SELECT Code From Clients WHERE Code=@code";
            command = new SqlCommand();
            command.CommandText = sql;
            command.Connection = connection;

            command.Parameters.AddWithValue("@code", txtCode.Text);
            var result = command.ExecuteScalar();

            if (result != null)
                MessageBox.Show("Mã số khách hàng đã tồn tại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                sql = "INSERT INTO Clients VALUES (@code, @name, @birthday, @address, @phone, @email, @picture)";
                command = new SqlCommand();
                command.CommandText = sql;
                command.Connection = connection;

                command.Parameters.AddWithValue("@code", txtCode.Text);
                command.Parameters.AddWithValue("@name", txtName.Text);
                command.Parameters.AddWithValue("@birthday", dtpBirthday.Value);
                command.Parameters.AddWithValue("@address", txtAddress.Text);
                command.Parameters.AddWithValue("@phone", txtPhone.Text);
                command.Parameters.AddWithValue("@email", txtEmail.Text);
                command.Parameters.AddWithValue("@picture", pbPicture.ImageLocation ?? "");

                var insertResult = command.ExecuteNonQuery();
                if (insertResult > 0)
                {
                    MessageBox.Show("Đã thêm khách hàng thành công.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadGridView();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra, vui lòng thử lại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        bool IsFormFullyFilled()
        {
            return !(string.IsNullOrEmpty(txtCode.Text)
                || string.IsNullOrEmpty(txtName.Text)
                || string.IsNullOrEmpty(dtpBirthday.Value.ToString())
                || string.IsNullOrEmpty(txtAddress.Text)
                || string.IsNullOrEmpty(txtPhone.Text)
                || string.IsNullOrEmpty(txtEmail.Text));
        }

        private void btnPicker_Click(object sender, EventArgs e)
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";
            choofdlog.FilterIndex = 1;
            choofdlog.InitialDirectory = $"{homeDir}\\Pictures";

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                try
                {
                    pbPicture.Image = Image.FromFile(sFileName);
                    pbPicture.ImageLocation = sFileName;
                }
                catch (OutOfMemoryException exception)
                {
                    MessageBox.Show($"Kích cỡ ảnh quá lớn, vui lòng thử lại.\nMã lỗi: {exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!IsFormFullyFilled())
            {
                MessageBox.Show("Chưa có khách hàng nào được chọn.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Bạn thật sự muốn xoá tài khoản này?", "Danger",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            { 
                return;
            }

            if (connection.State == ConnectionState.Closed)
                connection.Open();
            string sql = "DELETE FROM Clients WHERE Code=@code";
            command = new SqlCommand();
            command.CommandText = sql;
            command.Connection = connection;

            command.Parameters.AddWithValue("@code", txtCode.Text);
            var result = command.ExecuteNonQuery();

            if (result > 0)
            {
                MessageBox.Show("Xoá khách hàng thành công.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ReloadGridView();
                ClearForm();
            }
            else
            {
                MessageBox.Show($"Mã số khách hàng không tồn tại.\nKhông tìm thấy người dùng với mã số: {txtCode.Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!IsFormFullyFilled())
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (connection.State == ConnectionState.Closed)
                connection.Open();
            string sql = "SELECT Code From Clients WHERE Code=@code";
            command = new SqlCommand();
            command.CommandText = sql;
            command.Connection = connection;

            command.Parameters.AddWithValue("@code", txtCode.Text);
            var result = command.ExecuteScalar();

            if (result == null)
                MessageBox.Show("Mã số khách hàng không tồn tại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                sql = "UPDATE Clients " +
                    "SET Name=@name, Birthday=@birthday, Address=@address, Phone=@phone, Email=@email, Picture=@picture " +
                    "WHERE Code=@code";
                command = new SqlCommand();
                command.CommandText = sql;
                command.Connection = connection;

                command.Parameters.AddWithValue("@code", txtCode.Text);
                command.Parameters.AddWithValue("@name", txtName.Text);
                command.Parameters.AddWithValue("@birthday", dtpBirthday.Value);
                command.Parameters.AddWithValue("@address", txtAddress.Text);
                command.Parameters.AddWithValue("@phone", txtPhone.Text);
                command.Parameters.AddWithValue("@email", txtEmail.Text);
                command.Parameters.AddWithValue("@picture", pbPicture.ImageLocation ?? "");

                var insertResult = command.ExecuteNonQuery();
                if (insertResult > 0)
                {
                    MessageBox.Show("Cập nhật khách hàng thành công.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadGridView();
                }
                else
                {
                    MessageBox.Show($"Mã số khách hàng không tồn tại.\nKhông tìm thấy người dùng với mã số: {txtCode.Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
