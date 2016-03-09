using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Utilities;

namespace ScreenCropGui
{
    public class SysTrayApp : Form
    {
        public static NotifyIcon trayIcon;
        private DataHandler data = DataHandler.Instance;
        private ContextMenuStrip trayMenu;
        private globalKeyboardHook gkh = new globalKeyboardHook();
        private bool running = false;

        [STAThread]
        public static void Main()
        {
            // Main entry point to the program
            try
            {
                Application.Run(new SysTrayApp());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public SysTrayApp()
        {
            //Set system hook
            gkh.HookedKeys.Add(Keys.PrintScreen);
            gkh.KeyDown += new KeyEventHandler(GlobalHook_KeyDown);

            // Create tray menu.
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Recents", null, onRecents);
            trayMenu.Items.Add("Settings", null, OnSettings);
            trayMenu.Items.Add("Help", null, OnHelp);
            trayMenu.Items.Add("About", null, OnAbout);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, OnExit);

            // Create tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Screen Crop";
            trayIcon.Icon = new Icon(ScreenCropGui.Properties.Resources.AppCameraIco, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Load settings and screenshot info logs
            //data.Load_Settings();
            //data.Load_Screenshot_Logs();
        }

        private void copyToClipBoard(string text)
        {
            // Clear clipboard and copy text into it
            Clipboard.Clear();
            Clipboard.SetText(text);
        }

        void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            // Capture printscreen key press using the global keyboard hook event.
            // Make sure the process is not running already.
            if (running == false)
            {
                try
                {
                    // Create new process.
                    Process proc = new Process();
                    proc.StartInfo.FileName = @"ScreenCropper\ScreenCropper.exe";
                    proc.StartInfo.UseShellExecute = false;

                    // Strat the process
                    proc.Start();
                    running = true;

                    // Wait for the precess to finish.
                    proc.WaitForExit();

                    // Make sure that the process is killed to prevent memory leaks.
                    // SIDE NOTE: Looks like python tkinter gui library, witch is used at the Cropper module, 
                    // doesn't kill the process properly. dont know why it heppends yet, but here im making sure that 
                    // the process is gone for good.
                    foreach (var process in Process.GetProcessesByName("ScreenCropper.exe"))
                    {
                        process.Kill();
                    }
                    running = false;

                    // Grab the produced link.
                    Grab_Link();

                    // Log the file.
                    data.Log_ScreenShot_Info(data.LastName, data.CropperSettings.save_location, data.LastLink);
                    data.LastLink = string.Empty;
                    data.LastName = string.Empty;
                    data.Save_ScreenShot_Logs();
                    data.Load_Screenshot_Logs();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            }
        }

        void Grab_Link()
        {
            // Check that the link json file exists, the Cropper module creates that file after uploading 
            // the taken screenshot to imgur.com.
            // If the link file exists, parse the link and the name from the file and initialze the 
            // coresponding variables.
            // After grabing the link, place in the clipboard of the user and notify him that the action
            // was done.
            if (File.Exists(@"links.json"))
            {
                JObject link = JObject.Parse(File.ReadAllText("links.json"));
                data.LastLink = link["link"].ToString();
                data.LastName = link["name"].ToString();
                copyToClipBoard(data.LastLink);
                Show_Balloontip(data.LastName);
                File.Delete(@"links.json");
            }

            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory.ToString() + "temporary", true);
        }

        public void Show_Balloontip(string text)
        {
            trayIcon.BalloonTipText = text + " Added to clipboard";
            trayIcon.ShowBalloonTip(1500);
        }

        protected void onRecents(object sender, EventArgs e)
        {
            if (data.CapturedInfo.Count != 0)
            {
                RecentsForm recents = new RecentsForm();
                recents.Show();
            }
        }

        protected override void OnLoad(EventArgs e)
        {

            Visible = false;       // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        protected void OnSettings(object sender, EventArgs e)
        {
            Settings settings = new Settings(data.CropperSettings);
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
            data.Save_ScreenShot_Logs();
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
            this.ClientSize = new System.Drawing.Size(120, 0);
            this.Name = "SysTrayApp";
            this.ResumeLayout(false);

        }
    }
}