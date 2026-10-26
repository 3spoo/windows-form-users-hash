using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static WFormUsers.Form1;

// WITHOUT COLLISION MANAGEMENT.
namespace WFormUsers
{
    public partial class Form1 : Form
    {
        const string FILE_DATA = "data.dat";

        const int M = 250;

        const int DIM_USERNAME = 25;
        const int DIM_PASSWORD = 25;
        const int DIM_RECORD = (DIM_USERNAME + DIM_PASSWORD) * 2 + 1;

        public Form1()
        {
            InitializeComponent();

            CreateEnvironment();
        }

        public class User
        {
            private string _username;

            public string Username
            {
                get { return _username; }
                set { _username = value; }
            }

            private string _password;

            public string Password
            {
                get { return _password; }
                set { _password = value; }
            }

            private bool _isActive;

            public bool IsActive
            {
                get { return _isActive; }
                set { _isActive = value; }
            }

            public User(string u, string p)
            {
                _username = u;
                _password = p;
                _isActive = true;
            }

            public User(string u, string p, bool a)
            {
                _username = u;
                _password = p;
                _isActive = a;
            }

            public static long Hash(string username)
            {
                return (username.GetHashCode() & 0x7fffffff) % M;
            }
        }

        public void CreateEnvironment()
        {
            using (FileStream fs = new FileStream(FILE_DATA, FileMode.OpenOrCreate))
            {
                if (fs.Length == 0)
                {
                    fs.SetLength(M * DIM_RECORD);
                    return;
                }

                if (fs.Length % DIM_RECORD != 0)
                {
                    MessageBox.Show("FILE DI DIMENSIONE NON VALIDA, CORROTTO", "ERROR");
                    return;
                }

                for (int i = 0; i * DIM_RECORD < fs.Length; i++)
                {
                    User tmp = ReadUser(fs, i);
                    if (tmp != null)
                    {
                        dataGridView1.Rows.Add(tmp.Username, tmp.Password);
                    }
                }
            }
        }

        public User ReadUser(FileStream fs, long position)
        {
            fs.Seek(position * DIM_RECORD, SeekOrigin.Begin);

            using (BinaryReader br = new BinaryReader(fs, Encoding.Unicode, true))
            {
                string tmp_username = ReadString(br, DIM_USERNAME);
                string tmp_password = ReadString(br, DIM_PASSWORD);
                bool tmp_active = br.ReadBoolean();

                if (tmp_username == "" || tmp_password == "" || !tmp_active)
                    return null;

                return new User(tmp_username, tmp_password, tmp_active);
            }
        }

        public void WriteUser(FileStream fs, User user, long position)
        {
            fs.Seek(position * DIM_RECORD, SeekOrigin.Begin);

            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.Unicode, true))
            {
                WriteString(bw, user.Username, DIM_USERNAME);
                WriteString(bw, user.Password, DIM_PASSWORD);
                bw.Write(user.IsActive);
            }
        }

        public string ReadString(BinaryReader br, int dimension)
        {
            byte[] data = br.ReadBytes(dimension * 2);
            return Encoding.Unicode.GetString(data).TrimEnd('\0', ' ');
        }

        public void WriteString(BinaryWriter bw, string data, int dimension)
        {
            data = data.PadRight(dimension).Substring(0, dimension);
            byte[] byte_data = Encoding.Unicode.GetBytes(data);

            bw.Write(byte_data);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox2.Text.Trim();
            if (username.Length < 5)
            {
                MessageBox.Show("USERNAME NON VALIDO", "ERROR");
                return;
            }

            string password = textBox3.Text.Trim();
            if (password.Length < 5)
            {
                MessageBox.Show("PASSWORD NON VALIDA", "ERROR");
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (username.Equals(row.Cells[0].Value))
                {
                    MessageBox.Show("USERNAME GIA IN USO", "ERROR");
                    return;
                }
            }

            User user = new User(username, password);
            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                WriteUser(fs, user, User.Hash(user.Username));
            }

            dataGridView1.Rows.Add(user.Username, user.Password);
            textBox2.Text = "";
            textBox3.Text = "";
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string username = textBox2.Text.Trim();
            if (username.Length < 5)
            {
                MessageBox.Show("USERNAME NON VALIDO", "ERROR");
                return;
            }

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                User user = ReadUser(fs, User.Hash(username));
                if (user != null)
                {
                    textBox2.Text = user.Username;
                    textBox3.Text = user.Password;
                    return;
                }

                MessageBox.Show("UTENTE INESISTENTE", "ERROR");
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string username = textBox2.Text.Trim();
            if (username.Length < 5)
            {
                MessageBox.Show("USERNAME NON VALIDO", "ERROR");
                return;
            }

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                User user = ReadUser(fs, User.Hash(username));
                if (user != null)
                {
                    string password = textBox3.Text.Trim();
                    if (password.Length < 5)
                    {
                        MessageBox.Show("PASSWORD NON VALIDA", "ERROR");
                        return;
                    }

                    user.Password = password;
                    WriteUser(fs, user, User.Hash(user.Username));

                    // UPDATE DGV
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (username.Equals(row.Cells[0].Value))
                        {
                            row.Cells[1].Value = user.Password;
                        }
                    }
                    return;
                }

                MessageBox.Show("UTENTE INESISTENTE", "ERROR");
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            string username = textBox2.Text.Trim();
            if (username.Length < 5)
            {
                MessageBox.Show("USERNAME NON VALIDO", "ERROR");
                return;
            }

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                User user = ReadUser(fs, User.Hash(username));
                if (user != null)
                {
                    string password = textBox3.Text.Trim();
                    if (password.Length < 5 || !password.Equals(user.Password))
                    {
                        MessageBox.Show("PASSWORD NON VALIDA O ERRATA", "ERROR");
                        return;
                    }

                    user.IsActive = false;
                    WriteUser(fs, user, User.Hash(user.Username));

                    // UPDATE DGV
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (username.Equals(row.Cells[0].Value))
                        {
                            dataGridView1.Rows.Remove(row);
                            return;
                        }
                    }

                    return;
                }

                MessageBox.Show("UTENTE INESISTENTE", "ERROR");
            }
        }
    }
}
