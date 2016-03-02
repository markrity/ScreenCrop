using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Utilities;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScreenCropGui;

namespace ScreenCrop
{
    public class SysTrayApp : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        globalKeyboardHook gkh = new globalKeyboardHook();
        private bool running = false;

        public SysTrayApp()
        {
            // Create tray menu.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Settings", OnSettings);
            trayMenu.MenuItems.Add("Help", OnHelp);
            trayMenu.MenuItems.Add("About", OnAbout);
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Screen Crop";
            trayIcon.Icon = new Icon(ScreenCropGui.Properties.Resources.AppCameraIco, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            if (!File.Exists(@"settings.json"))
            {
                settingsClass cropSettings = new settingsClass
                {
                    save_location = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Screen Shots",
                    continuous_mode = false,
                    imgur_upload = true,
                    rec_color = "#ff3535",
                    rec_width = Convert.ToDecimal(1.3),
                    image_format = ".png"
                };

                string json = JsonConvert.SerializeObject(cropSettings, Formatting.Indented);
                File.WriteAllText("settings.json", json);
            }
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            if (running == false)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = @"ScreenCropper\ScreenCropper.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                running = true;
                proc.WaitForExit();
                foreach (var process in Process.GetProcessesByName("ScreenCropper.exe"))
                {
                    process.Kill();
                }
                running = false;
                this.grabLinks();
            }
        }

        private void grabLinks()
        {
            if (File.Exists(@"links.json"))
            {
                JObject links = JObject.Parse(File.ReadAllText("links.json"));
                Clipboard.SetText(links["link"].ToString());
                balloomTip(links["name"].ToString());
                File.Delete(@"links.json");
            }

            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory.ToString() + "temporary", true);
        }

        private void balloomTip(string text)
        {
            this.trayIcon.BalloonTipText = text + " Added to clipboard";
            this.trayIcon.ShowBalloonTip(15000);
        }

        protected override void OnLoad(EventArgs e)
        {
            gkh.HookedKeys.Add(Keys.PrintScreen);
            gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);

            Visible = false;       // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        protected void OnSettings(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        protected void OnHelp(object sender, EventArgs e)
        {
            ScreenCropGui.Help help = new ScreenCropGui.Help();
            help.Show();
        }

        protected void OnAbout(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SysTrayApp
            // 
            this.ClientSize = new Size(120, 0);
            this.Name = "ScreenCrop";
            this.ResumeLayout(false);
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