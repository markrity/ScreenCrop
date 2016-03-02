using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenCropGui
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", "ScreenCrop");
            this.labelProductName.Text = "ScreenCrop";
            this.labelVersion.Text = String.Format("Version {0}", "0.1");
            this.labelCompanyName.Text = "Michael Liv";
            this.textBoxDescription.Text = "ScreenCrop is a tool for an easier and smarter way of capturing and croping screenshots." + Environment.NewLine +
                                           "Press the print screen button on your keyboard to automaticly save a snapshot locally, upload it to imgur.com and copy its link to your clipboard." +
                                           Environment.NewLine + "Documentation and further information on what's inside can be found at:" + Environment.NewLine +
                                           "https://github.com/InviBear/ScreenCrop";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
