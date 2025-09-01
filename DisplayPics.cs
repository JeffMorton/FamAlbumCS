using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AxWMPLib;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{

    public partial class DisplayPics
    {
        public string SFileName { get; set; }
        private PictureBox pictureBox = new PictureBox();
        private MenuStrip menuStrip = new MenuStrip();
        private Button BtnRestart;
        private AxWindowsMediaPlayer pl = new AxWindowsMediaPlayer();
        private int TypeI;
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private string mfilename;

        public DisplayPics()
        {
            BtnRestart = new Button();
            InitializeComponent();
        }
        private void DisplayPics_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            var playerPanel = new Panel()
            {
                Left = 0,
                Top = 20,
                Width = screenWidth,
                Height = screenHeight - 20,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoScroll = true
            };
            Controls.Add(playerPanel);


            // pl = New AxWMPLib.AxWindowsMediaPlayer()
            playerPanel.Controls.Add(pl);  // Add it before initializing

            pl.BeginInit();
            pl.Name = "pl";
            pl.uiMode = "none";
            pl.EndInit();

            {
                ref var withBlock = ref pl;
                withBlock.Width = playerPanel.Width / 2;
                withBlock.Height = (int)Math.Round(0.95d * (playerPanel.Height / 2));
                withBlock.Location = new Point((playerPanel.Width - withBlock.Width) / 2, 50);
                withBlock.stretchToFit = true;
                withBlock.Visible = true;
            }

            int HT = 50 + pl.Height + 50;
            {
                var withBlock1 = BtnRestart;
                withBlock1.Text = "Restart";
                withBlock1.BackColor = Color.LightBlue;
                withBlock1.ForeColor = Color.DarkBlue;
                withBlock1.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock1.Size = new Size(220, 40);
                withBlock1.Location = new Point(pl.Left + pl.Width / 2 - 110, HT);
                withBlock1.Visible = true;
                withBlock1.AutoSize = true;
            }
            BtnRestart.Click += btnRestart_click;


            menuStrip = fmmenus.fmenus();
            var menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += MenuItemExit_Click;
            // Add the Exit item to the MenuStrip
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            menuStrip.Items.RemoveAt(0);
            menuStrip.Items.Insert(0, menuItemExit);
            Controls.Add(menuStrip);


            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            pictureBox.BorderStyle = BorderStyle.Fixed3D;
            // Me.Controls.Add(pictureBox)
            string Ddir;
            Ddir = SharedCode.GetDefaultDir();
            // Load the image from file
            string filePath = Ddir + SFileName;
            if (File.Exists(filePath))
            {
                string fileName = SFileName;
                var connection = Manager.GetConnection();
                var command = new SQLiteCommand("SELECT  PPeoplelist, PMonth, PYear,Pheight, Pwidth,PType FROM Pictures WHERE Pfilename = @name", connection);
                command.Parameters.AddWithValue("@name", fileName);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TypeI = Conversions.ToInteger(reader["PType"]);
                        if (TypeI != 1)
                        {
                            mfilename = Ddir + SFileName;
                            playerPanel.Controls.Add(pl);
                            Application.DoEvents();
                            playerPanel.Controls.Add(BtnRestart);


                            Playvideo(Ddir + SFileName);
                        }
                        else
                        {
                            if (SFileName.Contains(Ddir))
                            {
                                SFileName = SFileName.Replace(Ddir, "");
                            }
                            pl.Visible = false;
                            // Force a true decoupling from the file stream
                            byte[] imgBytes = File.ReadAllBytes(Ddir + SFileName);
                            using (var ms = new MemoryStream(imgBytes))
                            {
                                var originalImg = Image.FromStream(ms);
                                var scaledImg = ResizeImageToFitScreen(originalImg);
                                pictureBox.Image = scaledImg;
                            }
                            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                            pictureBox.Location = new Point((screenWidth - pictureBox.Width) / 2, (screenHeight - pictureBox.Height) / 2);

                            playerPanel.Controls.Add(pictureBox);
                        }
                    }
                }
            }
        }
        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }
        public Image ResizeImageToFitScreen(Image img)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            double scaleX = screenWidth / (double)img.Width;
            double scaleY = screenHeight / (double)img.Height;
            double scale = Math.Min(scaleX, scaleY) * 0.92d; // Leave some padding

            int newWidth = (int)Math.Round(img.Width * scale);
            int newHeight = (int)Math.Round(img.Height * scale);

            var resizedImg = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(resizedImg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, newWidth, newHeight);
            }

            return resizedImg;
        }


        private void CenterControl(ref Control ctrl)
        {
            // Calculate the position to center the control
            int x = (ClientSize.Width - ctrl.Width) / 2;
            int y = (ClientSize.Height - ctrl.Height) / 2;

            // Set the control's position
            ctrl.Location = new Point(x, y);
        }
        private void Playvideo(string videopath)
        {
            // Ensure the control is created before modifying properties
            if (!pl.IsHandleCreated)
            {
                pl.CreateControl();
            }
            Process[] processes = Process.GetProcessesByName("wmplayer"); // For classic Windows Media Player
            foreach (Process proc in processes)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit();
                    Console.WriteLine("Windows Media Player closed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error closing Windows Media Player: " + ex.Message);
                }
            }
            // Set video path and play
            pl.URL = videopath;
            pl.uiMode = "none";
            pl.Ctlcontrols.play();


        }
        private void btnRestart_click(object sender, EventArgs e)
        {
            if (pl.Ctlcontrols.currentPosition == 0d)
            {
                pl.Ctlcontrols.play();
            }
        }
    }
}