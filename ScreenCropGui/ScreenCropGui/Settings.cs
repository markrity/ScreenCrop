﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenCropGui
{
    public partial class Settings : Form
    {
        private settingsClass cropSettings = null;

        public Settings()
        {
            InitializeComponent();
            if (File.Exists(@"settings.json"))
            {
                JObject settingsJSON = JObject.Parse(File.ReadAllText("settings.json"));
                cropSettings = new settingsClass
                {
                    save_location = settingsJSON["save_location"].ToString(),
                    continuous_mode = Convert.ToBoolean(settingsJSON["continuous_mode"]),
                    imgur_upload = Convert.ToBoolean(settingsJSON["imgur_upload"]),
                    rec_color = settingsJSON["rec_color"].ToString(),
                    rec_width = Convert.ToDecimal(settingsJSON["rec_width"]),
                    image_format = settingsJSON["image_format"].ToString()
                };

                Color color = ColorTranslator.FromHtml(cropSettings.rec_color);
                buttonColor.BackColor = color;

                widthTextbox.Text = cropSettings.rec_width.ToString();
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
            this.cropSettings.rec_width = Convert.ToDecimal(widthTextbox.Text);
            this.cropSettings.image_format = ".png";

            string json = JsonConvert.SerializeObject(this.cropSettings, Formatting.Indented);
            File.WriteAllText("settings.json", json);
            settingsClass deserializedProduct = JsonConvert.DeserializeObject<settingsClass>(json);
            this.Close();
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        internal class settingsClass
        {
            public string save_location { get; set; }
            public bool continuous_mode { get; set; }
            public bool imgur_upload { get; set; }
            public string rec_color { get; set; }
            public decimal rec_width { get; set; }
            public string image_format { get; set; }
        }
    }
}