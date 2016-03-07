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
    public partial class EditTitleForm : Form
    {
        public string Title = string.Empty;

        public EditTitleForm()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Title = textBoxTitle.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
