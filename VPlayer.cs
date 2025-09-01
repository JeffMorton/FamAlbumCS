using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{
    public partial class VPlayer : Form
    {
        private Button PlayButton1;
        public string SFileName
        {
            get
            {
                return _sFileName;
            }
            set
            {
                _sFileName = value;
            }
        }

        private string _sFileName; // Private backing field
                                   // Public Property SFileName As String
        private Button Button1 = new Button();

        public VPlayer()
        {
            PlayButton1 = new Button();
            InitializeComponent();
        }

        private void VPlayer_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Initialize the Windows Media Player control and add it to the form
            var playerPanel = new Panel()
            {
                Size = new Size(1600, 1400),
                Location = new Point(100, 100)
            };

            Controls.Add(playerPanel);
            PlayerV();

        }



        private void PlayerV()
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"C:\Program Files (x86)\Windows Media Player\wmplayer.exe";
            processStartInfo.Arguments = "\"" + SFileName + "\"";
            processStartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(SFileName);
            processStartInfo.UseShellExecute = true;

            Process mediaPlayerProcess = null;  // Initialize to Nothing

            // Kill existing Media Player instances
            foreach (Process proc in Process.GetProcessesByName("wmplayer"))
                proc.Kill();

            try
            {
                // Start the process and assign the result
                mediaPlayerProcess = Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error launching media player: " + ex.Message);
            }

            // Ensure mediaPlayerProcess is initialized before calling WaitForExit
            if (mediaPlayerProcess is not null)
            {
                mediaPlayerProcess.WaitForExit();
                Close();
            }
            else
            {
                Debug.WriteLine("Failed to start the media player.");
            }
        }

    }
}