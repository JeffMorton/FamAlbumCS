using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;

namespace FamAlbum
{

    public partial class Thum
    {

        private FlowLayoutPanel flowPanel = new FlowLayoutPanel();
        public string[] NamesSelected { get; set; }
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();
        private List<string> picList;
        private MenuStrip menuStrip = new MenuStrip();

        public Thum()
        {
            InitializeComponent();
        }
        private void Thum_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            InitializeFlowLayoutPanel();
            connection = Manager.GetConnection();

            // Setup Exit Menu Item
            var menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += MenuItemExit_Click;
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
            Controls.Add(flowPanel);

            picList = new List<string>();

            // Select case handling different scenarios
            try
            {
                switch (NamesSelected[0] ?? "")
                {
                    case "Event":
                        {
                            LoadEventPictures();
                            break;
                        }
                    case "Noname":
                        {
                            LoadNonamePictures();
                            break;
                        }
                    case "Names":
                        {
                            LoadNamesPictures();
                            break;
                        }
                }

                // Remove duplicates
                picList = picList.Distinct().ToList();

                // Display thumbnails
                DisplayThumbnails(picList, picList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                if (connection is not null)
                    connection.Close();
            }
        }

        // Methods for different cases
        private void LoadEventPictures()
        {
            string qryGetFiles = "SELECT NLFileName FROM Namelist WHERE NLName = @Name";
            using (var command = new SQLiteCommand(qryGetFiles, connection))
            {
                command.Parameters.AddWithValue("@Name", NamesSelected[1]);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        picList.Add(reader["NLFileName"].ToString());
                }
            }
        }

        private void LoadNonamePictures()
        {
            string qryNoname = "SELECT PFilename FROM Pictures WHERE PPeopleList = ''";
            using (var command = new SQLiteCommand(qryNoname, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        picList.Add(reader["PFilename"].ToString());
                }
            }
        }

        private void LoadNamesPictures()
        {
            string qryPic = "SELECT Namelist.NLFileName FROM Namelist INNER JOIN Pictures ON Namelist.NLFileName = Pictures.PFileName WHERE Namelist.NLName IN (@NLName1, @NLName2, @NLName3, @NLName4, @NLName5)";

            using (var command = new SQLiteCommand(qryPic, connection))
            {
                for (int i = 1; i <= 5; i++)
                    command.Parameters.AddWithValue("@NLName" + i, NamesSelected[i]);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        picList.Add(reader["NLFileName"].ToString());
                }
            }
        }




        private void DisplayThumbnails(List<string> namelist, int cnt)
        {
            flowPanel.Controls.Clear();
            connection = Manager.GetConnection();

            try
            {
                using (connection)
                {
                    string qry = "SELECT Pthumbnail, Ppeoplelist FROM Pictures WHERE Pfilename = @name";

                    foreach (string name in namelist)
                    {
                        if (string.IsNullOrEmpty(name))
                            continue;

                        using (var command = new SQLiteCommand(qry, connection))
                        {
                            command.Parameters.AddWithValue("@name", name);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    byte[] imageData = reader["Pthumbnail"] as byte[];
                                    string peopleList = reader["Ppeoplelist"].ToString();

                                    if (imageData is not null && imageData.Length > 0 && (CheckPicture(peopleList, cnt) || NamesSelected[0] != "Event"))
                                    {
                                        var picBox = new PictureBox()
                                        {
                                            SizeMode = PictureBoxSizeMode.Zoom,
                                            Width = 200,
                                            Height = 200,
                                            Tag = name
                                        };

                                        using (var ms = new MemoryStream(imageData))
                                        {
                                            picBox.Image = Image.FromStream(ms);
                                        }

                                        picBox.MouseUp += PictureBox_MouseUp;
                                        flowPanel.Controls.Add(picBox);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private bool CheckPicture(string names, int cnt)
        {
            if (NamesSelected[0] == "Event")
                return true;

            int matchCount = 0;

            for (int i = 1; i <= 5; i++)
            {
                if (names.Contains(NamesSelected[i]))
                    matchCount += 1;
            }

            return matchCount == cnt;
        }


        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            PictureBox picBox = (PictureBox)sender;
            string SfileName = Conversions.ToString(picBox.Tag);

            if (e.Button == MouseButtons.Left)
            {
                var DisplayPicForm = new DisplayPics();
                DisplayPicForm.StartPosition = FormStartPosition.Manual;
                StartPosition = FormStartPosition.Manual;

                // Calculate the X position to center the form horizontally
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int formWidth = Width;
                int xPosition = (screenWidth - formWidth) / 2;

                // Set the Y position to 0 (top of the screen)
                int yPosition = 0;

                // Set the form's Location
                DisplayPicForm.Location = new Point(xPosition, yPosition);

                DisplayPicForm.SFileName = SfileName;
                DisplayPicForm.Show();
            }
            // Add more logic here for left-click actions
            else if (e.Button == MouseButtons.Right)
            {
                // Handle right-click event
                var DisplayInfom = new DisplayPics();
                My.MyProject.Forms.Displayinfo.StartPosition = FormStartPosition.Manual;
                StartPosition = FormStartPosition.Manual;

                // Calculate the X position to center the form horizontally
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int formWidth = Width;
                int xPosition = (screenWidth - formWidth) / 2;

                // Set the Y position to 0 (top of the screen)
                int yPosition = 0;

                // Set the form's Location
                My.MyProject.Forms.Displayinfo.Location = new Point(xPosition, yPosition);

                My.MyProject.Forms.Displayinfo.SFileName = SfileName;
                My.MyProject.Forms.Displayinfo.Show();
                // Add more logic here for right-click actions
            }
        }

        private void InitializeFlowLayoutPanel()
        {

            flowPanel.Name = "flowLayoutPanel1";
            flowPanel.Size = new Size(1000, 1000);
            flowPanel.Dock = DockStyle.Fill; // Adjust as needed
            flowPanel.AutoScroll = true;
            // Add the FlowLayoutPanel to the form
            Controls.Add(flowPanel);
        }


        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }
        public string GetDefaultDir()
        {
            // Open the registry key
            var key = Registry.CurrentUser.OpenSubKey(@"Software\FamilyAlbum");
            if (key is not null)
            {
                string value = key.GetValue("DefaultDir", "Default Value").ToString();
                key.Close();
                return value;

            }
            else
            {
                var GetDir = new GetDefaultFile();
                GetDir.Show();
                return null;
            }
        }

    }
}