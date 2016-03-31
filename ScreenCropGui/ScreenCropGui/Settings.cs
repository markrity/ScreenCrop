using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ScreenCropGui
{
    public partial class Settings : Form
    {
        private settingsClass cropSettings = null;

        public Settings(settingsClass settings)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.ShowIcon = true;
            if (File.Exists(@"settings.json"))
            {
                cropSettings = settings;

                Color color = ColorTranslator.FromHtml(cropSettings.rec_color);
                buttonColor.BackColor = color;

                thicknessUpDown.Value = cropSettings.rec_width;
                if (string.IsNullOrEmpty(cropSettings.save_location))
                {
                    saveToTextBox.Text = AppDomain.CurrentDomain.BaseDirectory + @"\Screen Shots";
                }
                else
                {
                    saveToTextBox.Text = cropSettings.save_location;
                }
                
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                buttonColor.BackColor = colorDialog.Color;
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog searchFolder = new FolderBrowserDialog();

            if (searchFolder.ShowDialog() == DialogResult.OK)
            {
                saveToTextBox.Text = searchFolder.SelectedPath;
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.cropSettings.save_location = saveToTextBox.Text.ToString().Replace("\\\\", "\\");
            // TODO: work on continuous mode
            this.cropSettings.continuous_mode = false;
            this.cropSettings.imgur_upload = Convert.ToBoolean(this.checkBoxUpload.CheckState);
            this.cropSettings.rec_color = HexConverter(buttonColor.BackColor);
            this.cropSettings.rec_width = thicknessUpDown.Value;
            this.cropSettings.image_format = ".png";

            string json = JsonConvert.SerializeObject(this.cropSettings, Formatting.Indented);
            File.WriteAllText("settings.json", json);
            this.Close();
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            float thickness = (float)thicknessUpDown.Value;
            Pen pen = new Pen(buttonColor.BackColor, thickness);
            graphics.DrawRectangle(pen, new Rectangle(pictureBox1.ClientSize.Width / 8,
                                                      pictureBox1.ClientSize.Height / 8,
                                                      Convert.ToInt32(pictureBox1.ClientSize.Width * 0.80),
                                                      Convert.ToInt32(pictureBox1.ClientSize.Height * 0.80)));
        }

        private void thicknessUpDown_ValueChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
