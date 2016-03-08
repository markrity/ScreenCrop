using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using ScreenCropGui;

namespace ScreenCropGui
{
    public partial class RecentsForm : Form
    {
        DataHandler data = DataHandler.Instance;
        Button[,] buttonMatrix;
        int buttonSize = 140;
        int xDim = 8;
        int yDim = 1;

        public RecentsForm()
        {
            InitializeComponent();
            init_Buttons();
            reSize();

        }

        private void init_Buttons()
        {
            int buttonAmount = data.CapturedInfo.Count;
            int y = 0;
            int x = 0;
            int cellNum = 0;
            

            while (buttonAmount >= xDim )
            {
                buttonAmount -= xDim;
                yDim++;
            }

            buttonMatrix = new Button[yDim, xDim];

            foreach (var item in data.CapturedInfo)
            {
                string text;
                Image background = null;
                if (item.Title != string.Empty)
                {
                    text = item.Title;
                }
                else
                {
                    text = item.Name;
                }

                buttonMatrix[y, x] = new Button()
                {
                    Size = new Size(buttonSize, buttonSize),
                    AutoSize = false,
                    Tag = cellNum++,
                    Text = text,
                    TextAlign = ContentAlignment.BottomCenter,
                    Font = new Font(Font.Name, Font.Size, FontStyle.Bold),
                    BackColor = Color.White,
                    Location = new Point(x * buttonSize + 10, y * buttonSize + 10),
                    Parent = panel1
                };

                try
                {
                    background = Image.FromFile(item.@Save_Location + "\\" + item.Name);
                    buttonMatrix[y, x].BackgroundImage = background;
                    buttonMatrix[y, x].BackgroundImageLayout = ImageLayout.Zoom;
                }
                catch{}

                buttonMatrix[y, x].MouseDown += RecentsForm_MouseDown;

                x++;

                if (x == xDim)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private void reSize()
        {
            int buttonAmount = data.CapturedInfo.Count;
            this.Size = new Size(buttonSize * buttonAmount, buttonSize * yDim + 60);
        }

        private void RecentsForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Button)
            {
                Button b = sender as Button;
                int tag = Convert.ToInt32(b.Tag);

                try
                {
                    Clipboard.Clear();
                    Clipboard.SetText(data.CapturedInfo[tag].Url);
                    SysTrayApp.trayIcon.BalloonTipText = data.CapturedInfo[tag].Name + " Added to clipboard";
                    SysTrayApp.trayIcon.ShowBalloonTip(1500);                    
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
