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
            initButtons();
            reSize();
        }

        private void initButtons()
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
                    Tag = cellNum,
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
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Tag = cellNum++;
                menu.Items.Add("Set title", null, new EventHandler(SetTitle));
                menu.Items.Add("Delete", null, new EventHandler(DeleteIndividualRecent));
                
                buttonMatrix[y, x].ContextMenuStrip = menu;

                x++;

                if (x == xDim)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private void SetTitle(object sender, EventArgs e)
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
                        // Try to cast the sender to a ToolStripItem
                        ToolStripItem menuItem = sender as ToolStripItem;
                        if (menuItem != null)
                        {
                            // Retrieve the ContextMenuStrip that owns this ToolStripItem
                            ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                            int index = Convert.ToInt32(owner.Tag);
                            data.CapturedInfo[Convert.ToInt32(index)].Title = form.text;
                            data.Save_ScreenShot_Logs();
                        }
                    }
                }
                RecentsForm recents = new RecentsForm();
                this.Close();
                recents.Show();
            }
        }

        private void DeleteIndividualRecent(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Saved screenshot will be premenantly deleted, Are you sure?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                this.Hide();
                try
                {
                    // Try to cast the sender to a ToolStripItem
                    ToolStripItem menuItem = sender as ToolStripItem;
                    if (menuItem != null)
                    {
                        // Retrieve the ContextMenuStrip that owns this ToolStripItem
                        ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                        int index = Convert.ToInt32(owner.Tag);
                        data.CapturedInfo.RemoveAt(index);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
                if (data.CapturedInfo.Count > 0)
                {
                    RecentsForm recents = new RecentsForm();
                    this.Close();
                    recents.Show();
                }
                else
                {
                    this.Close();
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
            this.Size = new Size(buttonSize * buttonAmount + 35, buttonSize * yDim + 60 + buttonClear.Height + 10);
            buttonClear.Location = new Point(this.Size.Width - buttonClear.Size.Width - 25, this.Size.Height - buttonClear.Size.Height - 45);
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
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("All saved data will be lost, Are you sure?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                data.CapturedInfo.Clear();
                data.Save_ScreenShot_Logs();
                this.Close();
            }
        }
    }
}
