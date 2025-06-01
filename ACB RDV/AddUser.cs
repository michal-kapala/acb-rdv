using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;
using QuazalWV;



namespace AcbRdv
{
    public partial class AddUser : Form
    {
        private static readonly object addUserLock = new object();
        public static string Generate(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
        public static string GeneratePasswd(int length = 13)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-_=+";
            StringBuilder res = new StringBuilder();
            var rng = RandomNumberGenerator.Create();
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (res.Length < length)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }

            return res.ToString();
        
        }
        public AddUser()
        {
            InitializeComponent();
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }



        private void GenerateUBIid_Click(object sender, EventArgs e)
        {
            Guid id = Guid.NewGuid();
            UbisoftIDField.Text = id.ToString();

        }




        private void generate_data_Click(object sender, EventArgs e)
        {
            Guid id = Guid.NewGuid();
            UbisoftIDField.Text = id.ToString();

            NameField.Text = Generate(8);
            PasswordField.Text = GeneratePasswd(10);
            int temp_pid = DBHelper.GetHighestPid();
            if (temp_pid==-1)
                GamePidField.Text = "1";
            else
            {
                GamePidField.Text = temp_pid.ToString();
            }
            EmailField.Text = Generate(8) + "@" + Generate(4) + "." + Generate(3);
            CountryCodeField.Text = "US";
            PreferedLanguageField.Text = "en";

        }
        private void verifyUser(object sender, EventArgs e)
        {
            bool returnState;
            string message;
            (returnState, message) = DBHelper.CheckDuplicateInformation(NameField.Text, EmailField.Text, UbisoftIDField.Text, GamePidField.Text);
            if (returnState == true)
            {

            }
            else
            {

            }
        }
        private void add_user_Click(object sender, EventArgs e)
        {
            bool returnState;
            string message;
            (returnState, message) = DBHelper.CheckDuplicateInformation(NameField.Text, EmailField.Text, UbisoftIDField.Text, GamePidField.Text);
            if (returnState==true)
            {
                MessageBox.Show(message);
                return;
            }
            lock (addUserLock)
            {
                try
                {
                    DBHelper.CheckDuplicateInformation(NameField.Text, EmailField.Text, UbisoftIDField.Text, GamePidField.Text);
                    DBHelper.AddUser(NameField.Text,PasswordField.Text, UbisoftIDField.Text, EmailField.Text,  GamePidField.Text, CountryCodeField.Text, PreferedLanguageField.Text);
                }
                catch(Exception exception )
                {
                    Log.WriteLine(1,$"[add user] {exception}");
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            PasswordField.UseSystemPasswordChar = !PasswordField.UseSystemPasswordChar;
        }
    }
}
