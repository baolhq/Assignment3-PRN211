using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assignment3
{
    public partial class Form1 : Form
    {
        SqlDataAdapter adapter;
        DataSet data;
        SqlConnection connection;
        SqlCommand command;

        public Form1()
        {
            InitializeComponent();

            //Ket noi den csdl
            string connectionString = "server=BAOLHQ\\SQLEXPRESS;database=Assignment3;uid=sa;pwd=123456789aaaaaa";
            connection = new SqlConnection(connectionString);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username or password is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (connection.State == ConnectionState.Closed) connection.Open();

            string sql = "SELECT * FROM Accounts WHERE Username=@username AND Password=@password";
            command = new SqlCommand();
            command.CommandText = sql;
            command.Connection = connection;

            command.Parameters.AddWithValue("@username", username);
            var hashedPassword = CreateMD5(password).ToLower();
            command.Parameters.AddWithValue("@password", hashedPassword);

            var result = command.ExecuteScalar();
            if (result == null)
                MessageBox.Show("Username or password is incorrect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {

                this.Hide();
                var mainForm = new frmMain();
                mainForm.Closed += (_, __) => this.Close();
                mainForm.Show();
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
