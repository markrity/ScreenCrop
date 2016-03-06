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
        private ContextMenuStrip trayMenu;
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
            trayMenu = new ContextMenuStrip();
            // ToolStripMenuItem item = new ToolStripMenuItem("Settings", null, OnSettings);
            trayMenu.Items.Add("Recents", null, null);
            trayMenu.Items.Add("Settings", null, OnSettings);
            trayMenu.Items.Add("Help", null, OnHelp);
            trayMenu.Items.Add("About", null, OnAbout);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, OnExit);

            // Create tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Screen Crop";
            trayIcon.Icon = new Icon(ScreenCropGui.Properties.Resources.AppCameraIco, 40, 40);
            trayIcon.MouseClick += TrayIcon_MouseClick;

            // Add menu to tray icon and show it.
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Load settings and screenshot info logs
            loadSettings();
            //loadRecents();
            loadScreenshotLogs();
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            //trayMenu.Show(this, e.X, e.Y);//places the menu at the pointer position
        }

        void loadRecents()
        {
            if (File.Exists(@"Logs\Captured.json"))
            {
                JObject capturedJSON = JObject.Parse(File.ReadAllText("Captured.json"));

                screenshotInfo recents = new screenshotInfo
                {
                    Name = capturedJSON["Name"].ToString(),
                    Title = capturedJSON["Title"].ToString(),
                    Save_Location = capturedJSON["Save_Location"].ToString(),
                    Url = capturedJSON["Url"].ToString()
                };

            }
                (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Add("123");
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

            string name = string.Empty;
            string title = string.Empty;
            string save_location = string.Empty;
            string url = string.Empty;

            if (File.Exists(@"Logs\Captured.json"))
            {
                var json = File.ReadAllText(@"Logs\Captured.json");
                var objects = JArray.Parse(json);

                foreach (JObject capturedinfo in objects)
                {
                    foreach (KeyValuePair<string, JToken> screenshot in capturedinfo)
                    {
                        if (screenshot.Key.Equals("Name"))
                        {
                            name = screenshot.Value.ToString();
                        }
                        if (screenshot.Equals("Title"))
                        {
                            title = screenshot.Value.ToString();
                        }
                        if (screenshot.Key.Equals("Save_Location"))
                        {
                            save_location = screenshot.Value.ToString();
                        }
                        if (screenshot.Key.Equals("Url"))
                        {
                            url = screenshot.Value.ToString();
                        }

                    }

                    screenshotInfo info = new screenshotInfo
                    {
                        Name = name,
                        Title = title,
                        Save_Location = save_location,
                        Url = url
                    };

                    capturedInfo.Add(info);
                    (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Add(info.Name);

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

        private void saveScreenShotLogs()
        {
            // If there are any logs 
            if (capturedInfo.Count > 0)
            {
                FileInfo file = new FileInfo(@"Logs\\Captured.json");

                // Create directory if doesn't exists
                file.Directory.Create();

                // Serialize the list to a json string
                string json = JsonConvert.SerializeObject(capturedInfo, Formatting.Indented);
                File.WriteAllText(file.FullName, json);
            }
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
                catch (System.ComponentModel.Win32Exception )
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
            saveScreenShotLogs();
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
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TrayIcon_MouseClick);
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