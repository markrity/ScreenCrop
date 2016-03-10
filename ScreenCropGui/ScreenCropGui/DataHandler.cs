using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScreenCropGui
{
    public class DataHandler
    {
        private static DataHandler _instance;

        private static settingsClass cropperSettings;
        private static List<screenshotInfo> capturedInfo;
        private static Dictionary<int, screenshotInfo> infoDict;
        private static string lastLink;
        private string lastName;

        public static DataHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataHandler();
                }
                return _instance;
            }
        }

        public DataHandler()
        {
            cropperSettings = new settingsClass();
            capturedInfo = new List<screenshotInfo>();
            infoDict = new Dictionary<int, screenshotInfo>();
            lastLink = string.Empty;
            lastName = string.Empty;

            Load_Settings();
            Load_Screenshot_Logs();
        }

        public settingsClass CropperSettings
        {
            get
            {
                return cropperSettings;
            }

            set
            {
                cropperSettings = value;
            }
        }

        public List<screenshotInfo> CapturedInfo
        {
            get
            {
                return capturedInfo;
            }

            set
            {
                capturedInfo = value;
            }
        }

        public  Dictionary<int, screenshotInfo> InfoDict
        {
            get
            {
                return infoDict;
            }

            set
            {
                infoDict = value;
            }
        }

        public string LastLink
        {
            get
            {
                return lastLink;
            }

            set
            {
                lastLink = value;
            }
        }

        public string LastName
        {
            get
            {
                return lastName;
            }

            set
            {
                lastName = value;
            }
        }

        public void Load_Settings()
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

        public void Load_Screenshot_Logs()
        {
            // Check if captured screenshots log file exists. if so, read it and deserialize it.
            // Otherwise, do nothing.
            // SIDE NOTE: i dont know if that's the right thing to do. i havn't decided yet if i'm
            // going to create the file right now or just check if the info list has any content in it later.

            string name = string.Empty;
            string title = string.Empty;
            string save_location = string.Empty;
            string url = string.Empty;

            if (capturedInfo.Count != 0)
            {
                capturedInfo.Clear();
            }

            if (File.Exists(@"Logs\Captured.json"))
            {
                var json = File.ReadAllText(@"Logs\Captured.json");
                JArray objects = new JArray();

                try
                {
                    objects = JArray.Parse(json);
                }
                catch (JsonReaderException)
                {

                }
                

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

        public void Log_ScreenShot_Info(string name, string save_location, string url)
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

        public void Save_ScreenShot_Logs()
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
            else if (capturedInfo.Count == 0)
            {
                if (File.Exists(@"Logs\\Captured.json"))
                {
                    File.Delete(@"Logs\\Captured.json");
                }
            }
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
