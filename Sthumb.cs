using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{
    public partial class Sthumb
    {

        private FlowLayoutPanel flowPanel;
        public string[] NamesSelected { get; set; }
        private SQLiteConnection connection = new SQLiteConnection();
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());

        private string[] piclist = new string[5001];
        private MenuStrip menuStrip = new MenuStrip();

        private Label loadingLabel;
        private int x = 0;

        public Sthumb()
        {
            flowPanel = new FlowLayoutPanel();
            InitializeComponent();
        }

        private void SThumb_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.BeginInvoke((Action)(() =>
            {
                flowPanel.Controls.Clear();
            }));
        }

        private void FlowPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            int scrollStep = 20; // Fine-tuned for smoother feel
            int newValue = flowPanel.VerticalScroll.Value - Math.Sign(e.Delta) * scrollStep;
            newValue = Math.Max(flowPanel.VerticalScroll.Minimum, Math.Min(flowPanel.VerticalScroll.Maximum, newValue));
            flowPanel.VerticalScroll.Value = newValue;
        }

        private void SThumb_Load(object sender, EventArgs e)
        {
            // SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
            // UpdateStyles()
            loadingLabel = new Label()
            {
                AutoSize = false,
                Size = new Size(250, 30),
                BackColor = Color.LightYellow,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Loading..."
            };
            WindowState = FormWindowState.Maximized;
            AutoScroll = true;
            // SetupComponent()
            InitializeFlowLayoutPanel();
            connection = Manager.GetConnection();
            string DDir = SharedCode.GetDefaultDir();

            menuStrip = fmmenus.fmenus();
            var menuItemExit = new ToolStripMenuItem("Exit") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
            menuItemExit.Click += MenuItemExit_Click;
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            menuStrip.Items.RemoveAt(0);
            menuStrip.Items.Insert(0, menuItemExit);
            Controls.Add(menuStrip);
            flowPanel.Controls.Clear();
            // Style and size

            Controls.Add(loadingLabel);
            FormClosed += SThumb_FormClosed;
            flowPanel.MouseWheel += FlowPanel_MouseWheel;
            int centerX = (ClientSize.Width - loadingLabel.Width) / 2;
            int centerY = (ClientSize.Height - loadingLabel.Height) / 2;
            loadingLabel.Location = new Point(centerX, centerY);
            loadingLabel.BringToFront();
            loadingLabel.Visible = true;
            Application.DoEvents(); // Force paint before loading thumbnails

            string SortOrd;
            if (Strings.Mid(NamesSelected[0], 3, 4) == ":Old")
            {
                SortOrd = " Order by Pictures.PYear ,Pictures.Pmonth;";
            }
            else
            {
                SortOrd = " Order by Pictures.PYear  desc,Pictures.Pmonth asc;";
            }
            int SelectedPeople = 0;
            for (int i = 1; i <= 5; i++)
            {
                if (!ReferenceEquals(NamesSelected[i], "99999"))
                    SelectedPeople += 1;
            }
            var command = new SQLiteCommand();

            if (NamesSelected[0].StartsWith("NP"))
            {
                string qryPic = @"SELECT Distinct Pfilename,npFileName, Pictures.PMonth, Pyear, Pictures.PPeoplelist, Pictures.Pthumbnail, Pnamecount
            FROM NamePhoto
            INNER JOIN Pictures ON NamePhoto.npFileName = Pictures.PFileName
            where PPeoplelist ='1' or Ppeoplelist is NULl or PPeoplelist=''
            " + SortOrd;
                command.CommandText = qryPic;
                command.Connection = connection;
            }
            else
            {
                string qryPic = @"SELECT Distinct Pfilename,npFileName, Pictures.PMonth, Pyear, Pictures.PPeoplelist, Pictures.Pthumbnail, Pnamecount
            FROM NamePhoto
            INNER JOIN Pictures ON NamePhoto.npFileName = Pictures.PFileName
            WHERE npID IN (@NLName1,@NLName2,@NLName3,@NLName4,@NLName5)
            " + SortOrd;
                command.CommandText = qryPic;
                command.Connection = connection;
                command.Parameters.AddWithValue("@NLName1", NamesSelected[1]);
                command.Parameters.AddWithValue("@NLName2", NamesSelected[2]);
                command.Parameters.AddWithValue("@NLName3", NamesSelected[3]);
                command.Parameters.AddWithValue("@NLName4", NamesSelected[4]);
                command.Parameters.AddWithValue("@NLName5", NamesSelected[5]);
            }


            using (connection)


                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int n = 0;
                        string peopleList = Conversions.ToString(reader["PPeoplelist"]);
                        for (int j = 1; j <= 5; j++)
                        {
                            if (peopleList.Contains(NamesSelected[j]))
                                n += 1;
                        }
                        bool allowAction = false;

                        if (!NamesSelected[0].StartsWith("Ex"))
                        {
                            if (SelectedPeople == n | NamesSelected[0] == "-2")
                            {
                                allowAction = true;
                            }
                        }
                        else if (SelectedPeople == Conversions.ToInteger(reader["Pnamecount"]))
                        {
                            int j = 0;
                            for (int i = 1, loopTo = n; i <= loopTo; i++)
                            {
                                if (peopleList.Split(',').Contains(NamesSelected[i]))
                                    j += 1;
                            }
                            if (j == SelectedPeople)
                            {
                                allowAction = true;
                            }
                        }
                        if (allowAction)
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("Pthumbnail")))
                            {
                                byte[] imgData = (byte[])reader["Pthumbnail"];
                                var ms = new MemoryStream(imgData);
                                var thumbImage = Image.FromStream(ms);

                                var pb = new PictureBox();
                                pb.Image = thumbImage;
                                pb.SizeMode = PictureBoxSizeMode.Zoom;
                                pb.Width = 150;
                                pb.Height = 150;
                                pb.Margin = new Padding(5);
                                pb.Tag = reader["npFileName"].ToString();
                                pb.MouseUp += PictureBox_MouseUp;

                                flowPanel.Controls.Add(pb);
                                x += 1;
                                if (x > 4999)
                                    break;
                            }
                        }
                    }
                    reader.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            var countMenuItem = new ToolStripMenuItem($"{x} images") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
            menuStrip.Items.RemoveAt(6);
            menuStrip.Items.Insert(6, countMenuItem);
            loadingLabel.Visible = false;
            Controls.Add(flowPanel);

            flowPanel.Enabled = true;
            flowPanel.Refresh();
        }
        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            PictureBox picBox = (PictureBox)sender;
            string SfileName = Conversions.ToString(picBox.Tag);
            if (e.Button == MouseButtons.Left)
            {
                var DisplayPicForm = new DisplayPics();
                DisplayPicForm.AutoSize = true;
                DisplayPicForm.StartPosition = FormStartPosition.Manual;
                StartPosition = FormStartPosition.Manual;

                // Calculate the X position to center the form horizontally
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int formWidth = Width;
                int xPosition = (screenWidth - formWidth) / 2;

                // Set the Y position to 0 (top of the screen)

                if (SharedCode.CMovie(SfileName))
                {

                    DisplayPicForm.SFileName = SfileName;
                    DisplayPicForm.Show();
                }
                // 
                else
                {
                    DisplayPicForm.SFileName = SfileName;
                    DisplayPicForm.Show();
                    // Add more logic here for left-click actions
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                var di = new Displayinfo();
                di.SFileName = SfileName;
                di.Show();

            }
        }
        private void InitializeFlowLayoutPanel()
        {
            flowPanel.Name = "flowLayoutPanel1";
            flowPanel.Size = new Size(1000, 1000);
            flowPanel.Dock = DockStyle.Fill; // Adjust as needed
            flowPanel.AutoScroll = true;
        }
        private bool CheckPicture(string Names, int cnt)
        {
            bool CheckPictureRet = default;
            var j = default(int);
            if (NamesSelected[0] != "Exclusive")
            {
                CheckPictureRet = true;
                return CheckPictureRet;
            }
            for (int i = 1; i <= 5; i++)
            {
                if ((NamesSelected[i] ?? "") == (Names ?? ""))
                    j += 1;
            }
            if (j == cnt)
            {
                CheckPictureRet = true;
            }
            else
            {
                CheckPictureRet = false;

            }

            return CheckPictureRet;

        }

        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            try
            {
                this.Dispose();
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Close failed: {ex.Message}");
            }
            //this.Close();
        }

    }

}