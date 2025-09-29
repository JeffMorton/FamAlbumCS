
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamAlbum
{
    public partial class DisplayPics : Form
    {
        public string SFileName { get; set; }
        private PictureBox pictureBox = new PictureBox();
        private MenuStrip menuStrip = new MenuStrip();
        private Button btnRestart = new Button();
        private VideoView videoView;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private int TypeI;
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private string mfilename;
        private bool _mediaPlayerDisposed = false;
        private bool _libVLCDisposed = false;

        public DisplayPics()
        {
            Core.Initialize(); // Required by LibVLC
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView = new VideoView { MediaPlayer = _mediaPlayer };
            InitializeComponent();
        }

        private async void DisplayPics_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);

            videoView.Name = "videoView";
            videoView.Visible = false;
            videoView.Size = new Size(this.Width / 2, (int)(0.95 * (this.Height / 2)));
            videoView.Location = new Point((this.Width - videoView.Width) / 2, 50);
            Controls.Add(videoView);

            btnRestart.Text = "Restart";
            btnRestart.BackColor = Color.LightBlue;
            btnRestart.ForeColor = Color.DarkBlue;
            btnRestart.Font = new Font("Arial", 12f, FontStyle.Bold);
            btnRestart.Size = new Size(220, 40);
            btnRestart.Visible = false;
            btnRestart.Click += btnRestart_Click;
            Controls.Add(btnRestart);

            menuStrip = fmmenus.fmenus();
            var menuItemExit = new ToolStripMenuItem("Exit") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
            menuItemExit.Click += MenuItemExit_Click;
            menuStrip.Items.RemoveAt(0);
            menuStrip.Items.Insert(0, menuItemExit);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);

            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BorderStyle = BorderStyle.Fixed3D;

            string Ddir = SharedCode.GetDefaultDir();
            string filePath = Path.Combine(Ddir, SFileName);

            if (File.Exists(filePath))
            {
                var connection = Manager.GetConnection();
                var command = new SQLiteCommand("SELECT PType FROM Pictures WHERE Pfilename = @name", connection);
                command.Parameters.AddWithValue("@name", SFileName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TypeI = Conversions.ToInteger(reader["PType"]);
                        if (TypeI != 1)
                        {
                            mfilename = filePath;
                            PlayVideo(filePath);
                        }
                        else
                        {
                            videoView.Visible = false;
                            btnRestart.Visible = false;

                            byte[] imgBytes = File.ReadAllBytes(filePath);
                            using (var ms = new MemoryStream(imgBytes))
                            {
                                var originalImg = Image.FromStream(ms);
                                var scaledImg = ResizeImageToFitScreen(originalImg);
                                pictureBox.Image = scaledImg;
                            }

                            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                            pictureBox.Location = new Point((screenWidth - pictureBox.Width) / 2, (screenHeight - pictureBox.Height) / 2);
                            Controls.Add(pictureBox);
                        }
                    }
                }
            }

            await Task.Delay(500); // Optional delay for smoother load
        }

        private void PlayVideo(string path)
        {
            videoView.Visible = true;
            btnRestart.Visible = true;

            var media = new Media(_libVLC, path, FromType.FromPath);
            _mediaPlayer.Play(media);

            btnRestart.Location = new Point(videoView.Left + videoView.Width / 2 - btnRestart.Width / 2, videoView.Bottom + 50);
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Play();
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
            double scale = Math.Min(scaleX, scaleY) * 0.92;

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

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (TypeI == 2)
            {
                try
                {
                    if (!_mediaPlayerDisposed && _mediaPlayer != null)
                    {
                        try
                        {
                            if (_mediaPlayer.IsPlaying)
                            {
                                _mediaPlayer.Stop();
                                System.Threading.Thread.Sleep(100); // Let VLC release threads
                            }
                        }
                        catch (AccessViolationException ave)
                        {
                            Debug.WriteLine("Access violation during Stop: " + ave.ToString());
                        }

                        _mediaPlayer.Dispose();
                        _mediaPlayerDisposed = true;
                    }

                    if (!_libVLCDisposed && _libVLC != null)
                    {
                        _libVLC.Dispose();
                        _libVLCDisposed = true;
                    }
                }
                catch (AccessViolationException ave)
                {
                    Debug.WriteLine("Access violation during disposal: " + ave.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while stopping video: " + ex.ToString());
                }

            }
        }
    }

    }