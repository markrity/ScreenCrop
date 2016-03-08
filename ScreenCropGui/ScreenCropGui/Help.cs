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
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
            textBox.GetPreferredSize(Size.Empty);
            appendRegular("Press the");
            appendBold(" PrintScreen ");
            appendRegular("button on your keyboard to launch the cropper." + Environment.NewLine);
            appendRegular("------------------------------------" + Environment.NewLine);

            appendRegular("Drag your mouse while pressing on");
            appendBold(" mouse 1 ");
            appendRegular("crop and press");
            appendBold(" Enter ");
            appendRegular("to save the snapshot." + Environment.NewLine);
            appendRegular("------------------------------------" + Environment.NewLine);

            appendRegular("Drag your mouse while pressing on");
            appendBold(" mouse 3 ");
            appendRegular("to move the crop square." + Environment.NewLine);
            appendRegular("------------------------------------" + Environment.NewLine);

            appendRegular("Keep in mind that while 'Upload to imgur.com' option is");
            appendBold(" checked, ");
            appendRegular("your screenshots will not be private and anyone would have access to them.");

            //String helpText = "Press the PrintScreen button on your keyboard to launch the cropper." + Environment.NewLine +
            //                  "Drag your mouse while pressing on mouse 1 to crop and press Enter to save the snapshot" + Environment.NewLine +
            //                  "Drag your mouse while pressing on mouse 3 to move the crop square" + Environment.NewLine +
            //                  "Keep in mind that while 'Upload to imgur.com' option is checked, " +
            //                  "your screenshots will not be private and anyone would have access to them.";
            //textBox.Text = helpText;

            textBox.GetPreferredSize(Size.Empty);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void appendBold(string text)
        {
            textBox.SelectionFont = new Font(textBox.Font, FontStyle.Bold);
            textBox.AppendText(text);
        }

        private void appendRegular(string text)
        {
            textBox.SelectionFont = new Font(textBox.Font, FontStyle.Regular);
            textBox.AppendText(text);
        }
    }
}
