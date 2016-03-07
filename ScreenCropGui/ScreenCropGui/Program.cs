using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScreenCropGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Utilities;

namespace ScreenCrop
{
    public class SysTrayApp : Form
    {
        private NotifyIcon trayIcon;
        private static ContextMenuStrip trayMenu;
        private globalKeyboardHook gkh = new globalKeyboardHook();
        private settingsClass cropperSettings = new settingsClass();
        private static List<screenshotInfo> capturedInfo = new List<screenshotInfo>();
        private static Dictionary<int,screenshotInfo> infoDict = new Dictionary<int, screenshotInfo>();
        private static int currentDropDownIndex = 0;
        private static System.Timers.Timer ClickTimer;
        private static int ClickCounter = 0;
        private string lastLink = string.Empty;
        private string lastName = string.Empty;
        private CheckBox checkBox1;
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
            // Initiate click counter
            ClickTimer = new System.Timers.Timer(300);
            ClickTimer.Elapsed += new ElapsedEventHandler(Evaluate_Clicks);

            // Create tray menu.
            trayMenu = new ContextMenuStrip();
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

            // Add menu to tray icon and show it.
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Load settings and screenshot info logs
            Load_Settings();
            Load_Screenshot_Logs();

            // Load recents submenu
            fillRecents();
        }

        private void copyToClipBoard(string text)
        {
            // Clear clipboard and copy text into it
            Clipboard.Clear();
            Clipboard.SetText(text);
        }

        private static void copyCurrentToClipBoard()
        {
            // thread safe clipboard copy and clear
            Clipboard.Clear();
            Clipboard.SetText(infoDict[currentDropDownIndex].Url.ToString());
        }

        private static void clearRecentsSubmenu()
        {
            (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Clear();
        }

        private static void addMenuItemToRecents(ToolStripItem subItem)
        {
            (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Add(subItem);
        }

        private static void fillRecents()
        {
            // fillRecents method will fill the recents submenu
            int i = 0;

            // This condition is here in the case that we need to refresh 
            if ((trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Count > 0)
            {
                Thread clearRecentsThread = new Thread(new ThreadStart(clearRecentsSubmenu));
                clearRecentsThread.SetApartmentState(ApartmentState.STA);
                clearRecentsThread.Start();

                infoDict.Clear();
            }

            // Go over every screenshot we took, and add it a dictionary and a into the recents submenu
            foreach (var info in capturedInfo)
            {
                // Register events
                ToolStripItem subItem = new ToolStripMenuItem();
                subItem.MouseDown += SubItem_MouseDown;
                subItem.MouseEnter += SubItem_MouseEnter;
                subItem.MouseLeave += SubItem_MouseLeave;

                // Use title if available
                if (info.Title == string.Empty)
                {
                    subItem.Text = info.Name;
                }
                else
                {
                    subItem.Text = info.Title;
                }
                
                // Add item to dropdown menu
                Thread addMenuItemThread = new Thread(() => addMenuItemToRecents(subItem));
                addMenuItemThread.SetApartmentState(ApartmentState.STA);
                addMenuItemThread.Start();

                //(trayMenu.Items[0] as ToolStripMenuItem).DropDownItems.Add(subItem);

                // Set tooltip text for each item
                String toolTipText = "Name: " + info.Name + Environment.NewLine +
                                     "Location: " + info.Save_Location + Environment.NewLine +
                                     "URL: " + info.Url;
                
                // Add item to dictionary and set the tag and the tool tip text
                infoDict.Add(i, info);
                (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems[i].Tag = i;
                (trayMenu.Items[0] as ToolStripMenuItem).DropDownItems[i++].ToolTipText = toolTipText;
            }
        }

        private static void SubItem_MouseEnter(object sender, EventArgs e)
        {
            // Recents submenu mouse enter event
            // this event function will stop the dropdownmenu from closing while the mouse is on the menu
            //  and it will update the currentDropDownIndex
            (trayMenu.Items[0] as ToolStripMenuItem).DropDown.AutoClose = false;
            currentDropDownIndex = (int)(sender as ToolStripMenuItem).Tag;
        }

        private static void SubItem_MouseLeave(object sender, EventArgs e)
        {
            // Mouse leave event function
            // allow the recents submenu to close
            (trayMenu.Items[0] as ToolStripMenuItem).DropDown.AutoClose = true;
        }

        private static void SubItem_MouseDown(object sender, MouseEventArgs e)
        {
            // Mouse click event funciton
            // Counts the amount of clicks on a submenu item
            ClickTimer.Stop();
            ClickCounter++;
            ClickTimer.Start();
        }

        private void Evaluate_Clicks(object sender, ElapsedEventArgs e)
        {
            // This function evaluates the amount of clicks on a submenu item and 
            // takes action 

            ClickTimer.Stop();
            switch (ClickCounter)
            {
                case 1:
                    using (EditTitleForm editForm = new EditTitleForm())
                    {
                        DialogResult dr = editForm.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            if (editForm.Title != string.Empty)
                            {
                                infoDict[currentDropDownIndex].Title = editForm.Title;
                                fillRecents();
                            }
                            

                        }

                    }
                    break;
                case 2:
                    try
                    {
                        Thread newThread = new Thread(new ThreadStart(copyCurrentToClipBoard));
                        newThread.SetApartmentState(ApartmentState.STA);
                        newThread.Start();
                        Show_Balloontip(infoDict[currentDropDownIndex].Url.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;

            }
            ClickCounter = 0;
        }

        private void Load_Settings()
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

        private void Load_Screenshot_Logs()
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
                    // Extract data from json
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

                    // Construct info container
                    screenshotInfo info = new screenshotInfo
                    {
                        Name = name,
                        Title = title,
                        Save_Location = save_location,
                        Url = url
                    };

                    // Add colected data to info collection
                    capturedInfo.Add(info);
                }

                //List<screenshotInfo> deserializedProduct = JsonConvert.DeserializeObject<settingsClass>(json);
            }
        }

        private void Log_ScreenShot_Info(string name, string save_location, string url)
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

        private void Save_ScreenShot_Logs()
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
                    Log_ScreenShot_Info(lastName, cropperSettings.save_location, lastLink);
                    lastLink = string.Empty;
                    lastName = string.Empty;

                    fillRecents();
                    
                }
                catch (System.ComponentModel.Win32Exception )
                {
                    MessageBox.Show("Whoops! Looks like ScreenCropper.exe is not found", "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                lastLink = link["link"].ToString();
                lastName = link["name"].ToString();
                copyToClipBoard(lastLink);
                Show_Balloontip(lastName);
                File.Delete(@"links.json");
            }

            Directory.Delete(AppDomain.CurrentDomain.BaseDirectory.ToString() + "temporary", true);
        }

        private void Show_Balloontip(string text)
        {
            this.trayIcon.BalloonTipText = text + " Added to clipboard";
            this.trayIcon.ShowBalloonTip(2500);
        }

        protected override void OnLoad(EventArgs e)
        {
            gkh.HookedKeys.Add(Keys.PrintScreen);
            gkh.KeyDown += new KeyEventHandler(GlobalHook_KeyDown);

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
            Save_ScreenShot_Logs();
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
            this.checkBox1 = new CheckBox();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // SysTrayApp
            // 
            this.ClientSize = new System.Drawing.Size(120, 0);
            this.Controls.Add(this.checkBox1);
            this.Name = "SysTrayApp";
            this.ResumeLayout(false);
            this.PerformLayout();

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