using System;
using System.Drawing;
using System.Windows.Forms;
using Utilities;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScreenCropGui;
using System.Collections.Generic;

namespace ScreenCrop
{
    public class SysTrayApp : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private globalKeyboardHook gkh = new globalKeyboardHook();
        private settingsClass cropperSettings = new settingsClass();
        private List<screenshotInfo> capturedInfo = new List<screenshotInfo>();
        private string lastLink = string.Empty;
        private string lastName = string.Empty;
        private bool running = false;

        [STAThread]
        public static void Main()
        {
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

            // Load settings and screenshot info logs
            loadSettings();
            loadScreenshotLogs();
        }
        
        void loadSettings()
        {
            // Check if settings file exists. if that's the case, initialize cropperSettings variable.
            // Otherwise, initialize cropperSettings using defualt parameters
            if (File.Exists(@"settings.json"))
            {
                JObject settingsJSON = JObject.Parse(File.ReadAllText("settings.json"));
                cropperSettings = new settingsClass
                {
                    save_location = settingsJSON["save_location"].ToString(),
                    continuous_mode = Convert.ToBoolean(settingsJSON["continuous_mode"]),
                    imgur_upload = Convert.ToBoolean(settingsJSON["imgur_upload"]),
                    rec_color = settingsJSON["rec_color"].ToString(),
                    rec_width = Convert.ToDecimal(settingsJSON["rec_width"]),
                    image_format = settingsJSON["image_format"].ToString()
                };
                
            }
            else
            {
                cropperSettings = new settingsClass
                {
                    save_location = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Screen Shots",
                    continuous_mode = false,
                    imgur_upload = true,
                    rec_color = "#ff3535",
                    rec_width = Convert.ToDecimal(1.3),
                    image_format = ".png"
                };

                string json = JsonConvert.SerializeObject(cropperSettings, Formatting.Indented);
                File.WriteAllText("settings.json", json);
            }
        }

        void loadScreenshotLogs()
        {
            // Check if captured screenshots log file exists. if so, read it and deserialize it.
            // Otherwise, do nothing.
            // SIDE NOTE: i dont know if that's the right thing to do. i havn't decided yet if i'm
            // going to create the file right now or just check if the info list has any content in it later.
            if (File.Exists(@"Logs\Captured.json"))
            {
                var json = File.ReadAllText(@"Logs\Captured.json");
                var objects = JArray.Parse(json);

                foreach (JObject capturedinfo in objects)
                {
                    foreach (KeyValuePair<String, JToken> screenshot in capturedinfo)
                    {

                        screenshotInfo info = new screenshotInfo
                        {
                            Name = screenshot.Value["Name"].ToString(),
                            Title = screenshot.Value["Title"].ToString(),
                            Save_Location = screenshot.Value["Save_Locvation"].ToString(),
                            Url = screenshot.Value["Url"].ToString()
                        };

                        capturedInfo.Add(info);
                    }

                }

                //List<screenshotInfo> deserializedProduct = JsonConvert.DeserializeObject<settingsClass>(json);
            }
        }

        private void logScreenShotInfo(string name, string save_location, string url)
        {
            screenshotInfo imageInfo = new screenshotInfo
            {
                Title = string.Empty,
                Name = name,
                Save_Location = save_location,
                Url = url
            };

            capturedInfo.Add(imageInfo);
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
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
                    grabLink();

                    // Log the file.
                    logScreenShotInfo(lastName, cropperSettings.save_location, lastLink);
                    lastLink = string.Empty;
                    lastName = string.Empty;
                    
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("Whoops! Looks like ScreenCropper.exe is not found", "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            }
        }

        void grabLink()
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
                lastLink = link["link"].ToString();
                lastName = link["name"].ToString();
                Clipboard.SetText(lastLink);
                balloomTip(lastName);
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
            Settings settings = new Settings(cropperSettings);
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
    }

    public class settingsClass
    {
        public string save_location { get; set; }
        public bool continuous_mode { get; set; }
        public bool imgur_upload { get; set; }
        public string rec_color { get; set; }
        public decimal rec_width { get; set; }
        public string image_format { get; set; }
    }

    public class screenshotInfo
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Save_Location { get; set; }
        public string Url { get; set; }
    }
}