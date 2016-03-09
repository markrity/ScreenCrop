using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenCropGui
{
    public partial class RecentsTextChange : Form
    {
        public string text = string.Empty;
        private int mousepositionX = 0;
        private int mousepositionY = 0; 

        public RecentsTextChange(int x, int y)
        {
            InitializeComponent();
            mousepositionX = x;
            mousepositionY = y;
            this.ActiveControl = textBox1;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {
                text = textBox1.Text;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void RecentsTextChange_Load(object sender, EventArgs e)
        {
            this.SetDesktopLocation(mousepositionX, mousepositionY);
        }
    }
}
