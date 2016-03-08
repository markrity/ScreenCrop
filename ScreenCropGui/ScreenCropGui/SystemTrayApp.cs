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

        //private static void fillRecents()
        //{
        //    // fillRecents method will fill the recents submenu
        //    int i = 0;

        //    // This condition is here in the case that we need to refresh 
        //    if ((trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Count > 0)
        //    {
        //        Thread clearRecentsThread = new Thread(new ThreadStart(clearRecentsSubmenu));
        //        clearRecentsThread.SetApartmentState(ApartmentState.STA);
        //        clearRecentsThread.Start();

        //        infoDict.Clear();
        //    }

        //    // Go over every screenshot we took, and add it a dictionary and a into the recents submenu
        //    foreach (var info in capturedInfo)
        //    {
        //        // Register events
        //        ToolStripItem subItem = new ToolStripMenuItem();
        //        subItem.MouseDown += SubItem_MouseDown;
        //        subItem.MouseEnter += SubItem_MouseEnter;
        //        subItem.MouseLeave += SubItem_MouseLeave;

        //        // Use title if available
        //        if (info.Title == string.Empty)
        //        {
        //            subItem.Text = info.Name;
        //        }
        //        else
        //        {
        //            subItem.Text = info.Title;
        //        }

        //        // Add item to dropdown menu
        //        Thread addMenuItemThread = new Thread(() => addMenuItemToRecents(subItem));
        //        addMenuItemThread.SetApartmentState(ApartmentState.STA);
        //        addMenuItemThread.Start();

        //        //(trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Add(subItem);

        //        // Set tooltip text for each item
        //        String toolTipText = "Name: " + info.Name + Environment.NewLine +
        //                             "Location: " + info.Save_Location + Environment.NewLine +
        //                             "URL: " + info.Url;

        //        // Add item to dictionary and set the tag and the tool tip text
        //        infoDict.Add(i, info);
        //        (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems[i].Tag = i;
        //        (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems[i++].ToolTipText = toolTipText;
        //    }
        //}

        //private void Load_Settings()
        //{
        //    // Check if settings file exists. if that's the case, initialize cropperSettings variable.
        //    // Otherwise, initialize cropperSettings using defualt parameters
        //    if (File.Exists(@"settings.json"))
        //    {
        //        JObject settingsJSON = JObject.Parse(File.ReadAllText("settings.json"));
        //        cropperSettings = new settingsClass
        //        {
        //            save_location = settingsJSON["save_location"].ToString(),
        //            continuous_mode = Convert.ToBoolean(settingsJSON["continuous_mode"]),
        //            imgur_upload = Convert.ToBoolean(settingsJSON["imgur_upload"]),
        //            rec_color = settingsJSON["rec_color"].ToString(),
        //            rec_width = Convert.ToDecimal(settingsJSON["rec_width"]),
        //            image_format = settingsJSON["image_format"].ToString()
        //        };

        //    }
        //    else
        //    {
        //        cropperSettings = new settingsClass
        //        {
        //            save_location = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Screen Shots",
        //            continuous_mode = false,
        //            imgur_upload = true,
        //            rec_color = "#ff3535",
        //            rec_width = Convert.ToDecimal(1.3),
        //            image_format = ".png"
        //        };

        //        string json = JsonConvert.SerializeObject(cropperSettings, Formatting.Indented);
        //        File.WriteAllText("settings.json", json);
        //    }
        //}

        //private void Load_Screenshot_Logs()
        //{
        //    // Check if captured screenshots log file exists. if so, read it and deserialize it.
        //    // Otherwise, do nothing.
        //    // SIDE NOTE: i dont know if that's the right thing to do. i havn't decided yet if i'm
        //    // going to create the file right now or just check if the info list has any content in it later.

        //    string name = string.Empty;
        //    string title = string.Empty;
        //    string save_location = string.Empty;
        //    string url = string.Empty;

        //    if (File.Exists(@"Logs\Captured.json"))
        //    {
        //        var json = File.ReadAllText(@"Logs\Captured.json");
        //        var objects = JArray.Parse(json);

        //        foreach (JObject capturedinfo in objects)
        //        {
        //            // Extract data from json
        //            foreach (KeyValuePair<string, JToken> screenshot in capturedinfo)
        //            {
        //                if (screenshot.Key.Equals("Name"))
        //                {
        //                    name = screenshot.Value.ToString();
        //                }
        //                if (screenshot.Equals("Title"))
        //                {
        //                    title = screenshot.Value.ToString();
        //                }
        //                if (screenshot.Key.Equals("Save_Location"))
        //                {
        //                    save_location = screenshot.Value.ToString();
        //                }
        //                if (screenshot.Key.Equals("Url"))
        //                {
        //                    url = screenshot.Value.ToString();
        //                }

        //            }

        //            // Construct info container
        //            screenshotInfo info = new screenshotInfo
        //            {
        //                Name = name,
        //                Title = title,
        //                Save_Location = save_location,
        //                Url = url
        //            };

        //            // Add colected data to info collection
        //            capturedInfo.Add(info);
        //        }

        //        //List<screenshotInfo> deserializedProduct = JsonConvert.DeserializeObject<settingsClass>(json);
        //    }
        //}

        //private void Log_ScreenShot_Info(string name, string save_location, string url)
        //{
        //    screenshotInfo imageInfo = new screenshotInfo
        //    {
        //        Title = string.Empty,
        //        Name = name,
        //        Save_Location = save_location,
        //        Url = url
        //    };

        //    capturedInfo.Add(imageInfo);
        //}

        //private void Save_ScreenShot_Logs()
        //{
        //    // If there are any logs 
        //    if (capturedInfo.Count > 0)
        //    {
        //        FileInfo file = new FileInfo(@"Logs\\Captured.json");

        //        // Create directory if doesn't exists
        //        file.Directory.Create();

        //        // Serialize the list to a json string
        //        string json = JsonConvert.SerializeObject(capturedInfo, Formatting.Indented);
        //        File.WriteAllText(file.FullName, json);
        //    }
        //}

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
            RecentsForm recents = new RecentsForm();
            recents.Show();
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