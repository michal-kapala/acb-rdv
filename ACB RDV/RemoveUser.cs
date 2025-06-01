using QuazalWV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AcbRdv
{
    public partial class RemoveUser : Form
    {
        public RemoveUser()
        {
            InitializeComponent();
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (DBHelper.RemoveUser(Username.Text))
            {
                MessageBox.Show($"success removing {Username.Text}");
            }
            else
            {
                MessageBox.Show($"failed removing {Username.Text}");
            }
        }
    }
}
