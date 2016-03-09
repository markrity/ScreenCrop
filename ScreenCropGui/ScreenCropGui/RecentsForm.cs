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

            while (buttonAmount >= xDim)
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
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
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
                catch { }

                buttonMatrix[y, x].MouseUp += RecentsForm_MouseUp;

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
            if (buttonAmount >= 8)
            {
                buttonAmount = xDim;
            }
            this.Size = new Size(buttonSize * buttonAmount + 35, buttonSize * yDim + 60);
        }

        private void RecentsForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Button)
            {
                Button b = sender as Button;
                int tag = Convert.ToInt32(b.Tag);

                if (e.Button == MouseButtons.Left)
                {
                    try
                    {
                        Clipboard.Clear();
                        Clipboard.SetText(data.CapturedInfo[tag].Url);
                        SysTrayApp.trayIcon.BalloonTipText = data.CapturedInfo[tag].Name + " Added to clipboard";
                        SysTrayApp.trayIcon.ShowBalloonTip(1500);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    this.Hide();
                    using (RecentsTextChange form = new RecentsTextChange(MousePosition.X, MousePosition.Y))
                    {
                        form.ShowDialog();
                        DialogResult dr = form.DialogResult;
                        if (dr == DialogResult.OK)
                        {
                            if (form.text != string.Empty)
                            {
                                data.CapturedInfo[tag].Title = form.text;
                            }
                        }
                        RecentsForm recents = new RecentsForm();
                        this.Close();
                        recents.Show();
                    }
                    
                }
            }
        }
    }
}
