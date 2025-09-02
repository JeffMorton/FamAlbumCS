<<<<<<< HEAD
﻿using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{
    public partial class Start : Form
    {
        private string[] NamesSelected;
        private int cnt = 0;
        private string DefaultDir;
        private SQLiteConnection connection;
        private ConnectionManager Manager;

        // UI Controls
        private Button btnContinue;
        private ComboBox cbNamesOnFile;
        private ListBox lvNamesSelected;
        private CheckBox Exclusive;
        private Label lb2;
        private MenuStrip menuStrip;

        public Start()
        {
          InitializeComponent();
            this.Load += Start_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "FamAlbum - Start";
            this.Size = new Size(400, 300);

            Manager = new ConnectionManager(SharedCode.GetConnectionString());
            connection = new SQLiteConnection();

            cbNamesOnFile = new ComboBox { Location = new Point(20, 20), Width = 200 };
            lvNamesSelected = new ListBox { Location = new Point(20, 60), Width = 200, Height = 100 };
            btnContinue = new Button { Text = "Continue", Location = new Point(240, 20) };
            Exclusive = new CheckBox { Text = "", Location = new Point(240, 60) };
            lb2 = new Label { Text = "Select Names", Location = new Point(20, 180) };
            menuStrip = new MenuStrip();

            btnContinue.Click += btnContinue_Click;

            this.Controls.Add(cbNamesOnFile);
            this.Controls.Add(lvNamesSelected);
            this.Controls.Add(btnContinue);
            this.Controls.Add(Exclusive);
            this.Controls.Add(lb2);
            this.Controls.Add(menuStrip);
        }

        private void Start_Load(object sender, EventArgs e)
        {
            DefaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LoadNamesFromDatabase();
            InitializeUI(); // Your renamed second Start_Load
        }
        private void LoadNamesFromDatabase()
{
    string query = "SELECT neName, ID FROM NameEvent WHERE neType = 'N' ORDER BY neName";
    DataTable dt = new DataTable();
    dt.Columns.Add("Name", typeof(string));
    dt.Columns.Add("ID", typeof(string));

    try
    {
        connection = Manager.GetConnection();
        //connection.Open();

        using (var command = new SQLiteCommand(query, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                dt.Rows.Add(reader["neName"].ToString(), reader["ID"].ToString());
            }
        }

        cbNamesOnFile.DataSource = dt;
        cbNamesOnFile.DisplayMember = "Name";
        cbNamesOnFile.ValueMember = "ID";
    }
    catch (SQLiteException ex)
    {
        MessageBox.Show("Database error: " + ex.Message);
    }
    catch (Exception ex)
    {
        MessageBox.Show("Unexpected error: " + ex.Message);
    }
    finally
    {
        if (connection.State == ConnectionState.Open)
            connection.Close();
    }
}

private void InitializeUI()
        {
            try
            {
                AutoScroll = true;
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;
                WindowState = FormWindowState.Maximized;
                NamesSelected = new string[6];
                NamesSelected[0] = "Al:Old";
                NamesSelected[1] = "99999";
                NamesSelected[2] = "99999";
                NamesSelected[3] = "99999";
                NamesSelected[4] = "99999";
                NamesSelected[5] = "99999";

                Controls.Add(Exclusive);
                menuStrip = fmmenus.fmenus();
                var menuItemSelectPeople = new ToolStripMenuItem("Select People") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
                var SortOldNew = new ToolStripMenuItem("Sort Old to New");
                var SortNewOld = new ToolStripMenuItem("Sort New to Old");
                var ClearSelected = new ToolStripMenuItem("Clear Selected");
                menuStrip.Items.Add(menuItemSelectPeople);
                menuItemSelectPeople.DropDownItems.Add(SortOldNew);
                menuItemSelectPeople.DropDownItems.Add(SortNewOld);
                menuItemSelectPeople.DropDownItems.Add(ClearSelected);

                SortOldNew.Click += Sortold_click;
                SortNewOld.Click += SortNew_click;
                ClearSelected.Click += ClearSelected_Click;
                Exclusive.CheckedChanged += Exclusive_checked;
                Controls.Add(menuStrip);
                menuStrip.Items.RemoveAt(1);
                menuStrip.Items.Insert(1, menuItemSelectPeople);

                var menuItemExit = new ToolStripMenuItem("Exit") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
                menuItemExit.Click += Exitapp;
                menuStrip.Items.RemoveAt(0);
                menuStrip.Items.Insert(0, menuItemExit);

                var title = new Label()
                {
                    Text = "Family Album",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("segoe", 45f),
                    Size = new Size(600, 65),
                    Location = new Point((int)Math.Round(screenWidth / 2d - 300d), 22)
                };

                Controls.Add(title);
                title.BringToFront();

                var subtitle = new Label()
                {
                    Text = "Select People",
                    Font = new Font("segoe", 24f),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(600, 60),
                    Location = new Point((int)Math.Round(screenWidth / 2d - 300d), 85)
                };
                Controls.Add(subtitle);
                var lb1 = new Label()
                {
                    Text = "Choose up to 5 People",
                    Location = new Point((int)Math.Round((screenWidth - 400) / 2d), 150),
                    Font = new Font("Arial", 16f, FontStyle.Bold),
                    Size = new Size(400, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Controls.Add(lb1);
                {
                    ref var withBlock = ref lb2;
                    withBlock.Text = "(Old to New)";
                    withBlock.Location = new Point((int)Math.Round((screenWidth - 400) / 2d), 180);
                    withBlock.Font = new Font("Arial", 10f, FontStyle.Bold);
                    withBlock.Size = new Size(400, 20);
                    withBlock.TextAlign = ContentAlignment.MiddleCenter;
                }
                Controls.Add(lb2);
                {
                    var withBlock1 = cbNamesOnFile;
                    withBlock1.Location = new Point((int)Math.Round(screenWidth / 2d - 400d), 200);
                    withBlock1.Size = new Size(800, 18);
                    withBlock1.Font = new Font(cbNamesOnFile.Font.FontFamily, 12f);
                    withBlock1.MaxDropDownItems = 6;
                    withBlock1.DropDownHeight = 255;
                }
                cbNamesOnFile.SelectionChangeCommitted += cbNamesOnFile_SelectedIndexChanged;
                CenterControl(cbNamesOnFile, 200);
                Controls.Add(cbNamesOnFile);

                {
                    ref var withBlock2 = ref lvNamesSelected;
                    withBlock2.ForeColor = Color.DarkBlue;
                    withBlock2.Font = new Font("Arial", 12f, FontStyle.Bold);
                    withBlock2.Size = new Size(800, 100);
                    withBlock2.ForeColor = Color.DarkBlue;
                }
                Controls.Add(lvNamesSelected);
                CenterControl(lvNamesSelected, 490);

                var lbEx = new Label()
                {
                    Size = new Size(400, 21),
                    Font = new Font("Arial", 12f, FontStyle.Bold),
                    Text = "Only these people: ",
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point((int)Math.Round((screenWidth - 400) / 2d - 150d), 600)
                };
                Controls.Add(lbEx);
                {
                    var withBlock3 = Exclusive;
                    withBlock3.Size = new Size(30, 20);
                    withBlock3.TextAlign = ContentAlignment.MiddleRight;
                    withBlock3.Location = new Point((int)Math.Round((screenWidth - 400) / 2d + 258d), 600);
                }

                {
                    var withBlock4 = btnContinue;
                    withBlock4.Text = "Continue";
                    withBlock4.BackColor = Color.LightBlue;
                    withBlock4.ForeColor = Color.DarkBlue;
                    withBlock4.Font = new Font("Arial", 12f, FontStyle.Bold);
                    withBlock4.Size = new Size(250, 40);
                    withBlock4.Enabled = false;
                }
                btnContinue.Click += btnContinue_Click;
                CenterControl(btnContinue, 630);
                var dt = new DataTable();
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("ID", typeof(string));
                Controls.Add(lvNamesSelected);
                Controls.Add(btnContinue);
                // Add cbNamesOnFile to the form's controls
                Controls.Add(cbNamesOnFile);

                string qryNameCnt = "Select neName,ID from NameEvent where neType ='N' order by neName";
                connection = Manager.GetConnection();
                using (connection)
                {
                    var command = new SQLiteCommand(qryNameCnt, connection);
                    try
                    {
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                            dt.Rows.Add(reader["neName"], reader["ID"].ToString());

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
                }

                cbNamesOnFile.DataSource = dt;
                cbNamesOnFile.DisplayMember = "Name";
                cbNamesOnFile.ValueMember = "ID";
                Cursor = Cursors.Default;
                //cbNamesOnFile.Focus();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Start_Shown(object sender, EventArgs e)
        {
            cbNamesOnFile.DroppedDown = true;
            cbNamesOnFile.Focus();
        }

        private void cbNamesOnFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get selected item(s) from ComboBox and store them in the array
            cnt += 1;
            if (cnt >= 6)
            {
                MessageBox.Show("Only 5 names can be selected");
                return;
            }
            DataRowView drv = (DataRowView)cbNamesOnFile.SelectedItem;
            string nameValue = drv["Name"].ToString();

            if (!string.IsNullOrWhiteSpace(nameValue))
            {
                NamesSelected[cnt] = nameValue;
                lvNamesSelected.Items.Add(nameValue);
                NamesSelected[cnt] = drv["ID"].ToString();

                btnContinue.Enabled = true;
            }
           // cbNamesOnFile.DroppedDown = true;
        }
        private void CenterControl(Control ctrl, int y)
        {
            // Calculate the position to center the control
            int x = (ClientSize.Width - ctrl.Width) / 2;
            // Dim y As Integer = () \ 2

            // Set the control's position
            ctrl.Location = new Point(x, y);
        }
        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (Exclusive.Checked && NamesSelected != null && NamesSelected.Length > 0)
            {
                // Replace first two characters with "Ex"
                if (NamesSelected[0].Length >= 2)
                    NamesSelected[0] = "Ex" + NamesSelected[0].Substring(2);
            }

            var thumbForm = new Sthumb
            {
                NamesSelected = NamesSelected
            };
            thumbForm.Show();
        }

        private void MenuItemPeople_Click(object sender, EventArgs e)
        {
            var strt = new Start();
            strt.Show();
        }
        private void MenuItemClear_Click(object sender, EventArgs e)
        {
            lvNamesSelected.Items.Clear();
            for (int i = 1, loopTo = NamesSelected.Length - 1; i <= loopTo; i++)
                NamesSelected[i] = (-1).ToString();
            Refresh();
        }
        private void MenuitemAdd_Click(object sender, EventArgs e)
        {
            var AddP = new AddPhoto();
            AddP.Show();
        }
        private void Exclusive_checked(object sender, EventArgs e)
        {
            if (NamesSelected == null || NamesSelected.Length == 0 || NamesSelected[0].Length < 2)
                return; // Safety check to avoid runtime errors

            if (Exclusive.Checked)
            {
                // Replace first two characters with "Ex"
                NamesSelected[0] = "Ex" + NamesSelected[0].Substring(2);
            }
            else
            {
                // Replace first two characters with "Al"
                NamesSelected[0] = "Al" + NamesSelected[0].Substring(2);
            }
        }
        
        private void ClearSelected_Click(object sender, EventArgs e)
        {
            lvNamesSelected.Items.Clear();
            NamesSelected[1] = "99999";
            NamesSelected[2] = "99999";
            NamesSelected[3] = "99999";
            NamesSelected[4] = "99999";
            NamesSelected[5] = "99999";
            cnt = 0;
        }
        private void Sortold_click(object sender, EventArgs e)
        {
            if (NamesSelected != null && NamesSelected.Length > 0 && NamesSelected[0].Length >= 6)
            {
                NamesSelected[0] = NamesSelected[0].Substring(0, 2) + ":Old" + NamesSelected[0].Substring(6);
            }

            lb2.Text = "(Old to New)";
            cbNamesOnFile.DroppedDown = true;
        }

        private void SortNew_click(object sender, EventArgs e)
        {
            if (NamesSelected != null && NamesSelected.Length > 0 && NamesSelected[0].Length >= 6)
            {
                NamesSelected[0] = NamesSelected[0].Substring(0, 2) + ":New" + NamesSelected[0].Substring(6);
            }
            lb2.Text = "(New to Old)";
            cbNamesOnFile.DroppedDown = true;
        }
        private void Exitapp(object sender, EventArgs e)
        {
            string qry = "SELECT PDateEntered FROM Pictures ORDER BY PDateEntered DESC LIMIT 1";
            connection = Manager.GetConnection();

            DateTime lastDate = new DateTime(2000, 1, 1); // Default fallback

            using (connection)
            {
                var command = new SQLiteCommand(qry, connection);
                var result = command.ExecuteScalar();

                if (result != null && DateTime.TryParse(result.ToString(), out DateTime parsedDate))
                {
                    lastDate = parsedDate;
                }
            }

            if (lastDate.Date == DateTime.Today.Date)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Do you want to backup the database?",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (dialogResult == DialogResult.Yes)
                {
                    string connectionString = SharedCode.GetConnectionString();
                    string filename = $"FamAlbum{DateTime.Today.Month}{DateTime.Today.Day}{DateTime.Today.Year}.bak";
                    string backupPath = BackupRest.GetBackupPath() + filename;

                    BackupRest.BackupSQLiteDatabase();
                }
            }

            Application.Exit();
        }

    }
=======
﻿using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{
    public partial class Start : Form
    {
        private string[] NamesSelected;
        private int cnt = 0;
        private string DefaultDir;
        private SQLiteConnection connection;
        private ConnectionManager Manager;

        // UI Controls
        private Button btnContinue;
        private ComboBox cbNamesOnFile;
        private ListBox lvNamesSelected;
        private CheckBox Exclusive;
        private Label lb2;
        private MenuStrip menuStrip;

        public Start()
        {
          InitializeComponent();
            this.Load += Start_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "FamAlbum - Start";
            this.Size = new Size(400, 300);

            Manager = new ConnectionManager(SharedCode.GetConnectionString());
            connection = new SQLiteConnection();

            cbNamesOnFile = new ComboBox { Location = new Point(20, 20), Width = 200 };
            lvNamesSelected = new ListBox { Location = new Point(20, 60), Width = 200, Height = 100 };
            btnContinue = new Button { Text = "Continue", Location = new Point(240, 20) };
            Exclusive = new CheckBox { Text = "", Location = new Point(240, 60) };
            lb2 = new Label { Text = "Select Names", Location = new Point(20, 180) };
            menuStrip = new MenuStrip();

            btnContinue.Click += btnContinue_Click;

            this.Controls.Add(cbNamesOnFile);
            this.Controls.Add(lvNamesSelected);
            this.Controls.Add(btnContinue);
            this.Controls.Add(Exclusive);
            this.Controls.Add(lb2);
            this.Controls.Add(menuStrip);
        }

        private void Start_Load(object sender, EventArgs e)
        {
            DefaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LoadNamesFromDatabase();
            InitializeUI(); // Your renamed second Start_Load
        }
        private void LoadNamesFromDatabase()
{
    string query = "SELECT neName, ID FROM NameEvent WHERE neType = 'N' ORDER BY neName";
    DataTable dt = new DataTable();
    dt.Columns.Add("Name", typeof(string));
    dt.Columns.Add("ID", typeof(string));

    try
    {
        connection = Manager.GetConnection();
        //connection.Open();

        using (var command = new SQLiteCommand(query, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                dt.Rows.Add(reader["neName"].ToString(), reader["ID"].ToString());
            }
        }

        cbNamesOnFile.DataSource = dt;
        cbNamesOnFile.DisplayMember = "Name";
        cbNamesOnFile.ValueMember = "ID";
    }
    catch (SQLiteException ex)
    {
        MessageBox.Show("Database error: " + ex.Message);
    }
    catch (Exception ex)
    {
        MessageBox.Show("Unexpected error: " + ex.Message);
    }
    finally
    {
        if (connection.State == ConnectionState.Open)
            connection.Close();
    }
}

private void InitializeUI()
        {
            try
            {
                AutoScroll = true;
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;
                WindowState = FormWindowState.Maximized;
                NamesSelected = new string[6];
                NamesSelected[0] = "Al:Old";
                NamesSelected[1] = "99999";
                NamesSelected[2] = "99999";
                NamesSelected[3] = "99999";
                NamesSelected[4] = "99999";
                NamesSelected[5] = "99999";

                Controls.Add(Exclusive);
                menuStrip = fmmenus.fmenus();
                var menuItemSelectPeople = new ToolStripMenuItem("Select People") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
                var SortOldNew = new ToolStripMenuItem("Sort Old to New");
                var SortNewOld = new ToolStripMenuItem("Sort New to Old");
                var ClearSelected = new ToolStripMenuItem("Clear Selected");
                menuStrip.Items.Add(menuItemSelectPeople);
                menuItemSelectPeople.DropDownItems.Add(SortOldNew);
                menuItemSelectPeople.DropDownItems.Add(SortNewOld);
                menuItemSelectPeople.DropDownItems.Add(ClearSelected);

                SortOldNew.Click += Sortold_click;
                SortNewOld.Click += SortNew_click;
                ClearSelected.Click += ClearSelected_Click;
                Exclusive.CheckedChanged += Exclusive_checked;
                Controls.Add(menuStrip);
                menuStrip.Items.RemoveAt(1);
                menuStrip.Items.Insert(1, menuItemSelectPeople);

                var menuItemExit = new ToolStripMenuItem("Exit") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
                menuItemExit.Click += Exitapp;
                menuStrip.Items.RemoveAt(0);
                menuStrip.Items.Insert(0, menuItemExit);

                var title = new Label()
                {
                    Text = "Family Album",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("segoe", 45f),
                    Size = new Size(600, 65),
                    Location = new Point((int)Math.Round(screenWidth / 2d - 300d), 22)
                };

                Controls.Add(title);
                title.BringToFront();

                var subtitle = new Label()
                {
                    Text = "Select People",
                    Font = new Font("segoe", 24f),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(600, 60),
                    Location = new Point((int)Math.Round(screenWidth / 2d - 300d), 85)
                };
                Controls.Add(subtitle);
                var lb1 = new Label()
                {
                    Text = "Choose up to 5 People",
                    Location = new Point((int)Math.Round((screenWidth - 400) / 2d), 150),
                    Font = new Font("Arial", 16f, FontStyle.Bold),
                    Size = new Size(400, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Controls.Add(lb1);
                {
                    ref var withBlock = ref lb2;
                    withBlock.Text = "(Old to New)";
                    withBlock.Location = new Point((int)Math.Round((screenWidth - 400) / 2d), 180);
                    withBlock.Font = new Font("Arial", 10f, FontStyle.Bold);
                    withBlock.Size = new Size(400, 20);
                    withBlock.TextAlign = ContentAlignment.MiddleCenter;
                }
                Controls.Add(lb2);
                {
                    var withBlock1 = cbNamesOnFile;
                    withBlock1.Location = new Point((int)Math.Round(screenWidth / 2d - 400d), 200);
                    withBlock1.Size = new Size(800, 18);
                    withBlock1.Font = new Font(cbNamesOnFile.Font.FontFamily, 12f);
                    withBlock1.MaxDropDownItems = 6;
                    withBlock1.DropDownHeight = 255;
                }
                cbNamesOnFile.SelectionChangeCommitted += cbNamesOnFile_SelectedIndexChanged;
                CenterControl(cbNamesOnFile, 200);
                Controls.Add(cbNamesOnFile);

                {
                    ref var withBlock2 = ref lvNamesSelected;
                    withBlock2.ForeColor = Color.DarkBlue;
                    withBlock2.Font = new Font("Arial", 12f, FontStyle.Bold);
                    withBlock2.Size = new Size(800, 100);
                    withBlock2.ForeColor = Color.DarkBlue;
                }
                Controls.Add(lvNamesSelected);
                CenterControl(lvNamesSelected, 490);

                var lbEx = new Label()
                {
                    Size = new Size(400, 21),
                    Font = new Font("Arial", 12f, FontStyle.Bold),
                    Text = "Only these people: ",
                    TextAlign = ContentAlignment.MiddleRight,
                    Location = new Point((int)Math.Round((screenWidth - 400) / 2d - 150d), 600)
                };
                Controls.Add(lbEx);
                {
                    var withBlock3 = Exclusive;
                    withBlock3.Size = new Size(30, 20);
                    withBlock3.TextAlign = ContentAlignment.MiddleRight;
                    withBlock3.Location = new Point((int)Math.Round((screenWidth - 400) / 2d + 258d), 600);
                }

                {
                    var withBlock4 = btnContinue;
                    withBlock4.Text = "Continue";
                    withBlock4.BackColor = Color.LightBlue;
                    withBlock4.ForeColor = Color.DarkBlue;
                    withBlock4.Font = new Font("Arial", 12f, FontStyle.Bold);
                    withBlock4.Size = new Size(250, 40);
                    withBlock4.Enabled = false;
                }
                btnContinue.Click += btnContinue_Click;
                CenterControl(btnContinue, 630);
                var dt = new DataTable();
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("ID", typeof(string));
                Controls.Add(lvNamesSelected);
                Controls.Add(btnContinue);
                // Add cbNamesOnFile to the form's controls
                Controls.Add(cbNamesOnFile);

                string qryNameCnt = "Select neName,ID from NameEvent where neType ='N' order by neName";
                connection = Manager.GetConnection();
                using (connection)
                {
                    var command = new SQLiteCommand(qryNameCnt, connection);
                    try
                    {
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                            dt.Rows.Add(reader["neName"], reader["ID"].ToString());

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
                }

                cbNamesOnFile.DataSource = dt;
                cbNamesOnFile.DisplayMember = "Name";
                cbNamesOnFile.ValueMember = "ID";
                Cursor = Cursors.Default;
                //cbNamesOnFile.Focus();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Start_Shown(object sender, EventArgs e)
        {
            cbNamesOnFile.DroppedDown = true;
            cbNamesOnFile.Focus();
        }

        private void cbNamesOnFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get selected item(s) from ComboBox and store them in the array
            cnt += 1;
            if (cnt >= 6)
            {
                MessageBox.Show("Only 5 names can be selected");
                return;
            }
            DataRowView drv = (DataRowView)cbNamesOnFile.SelectedItem;
            string nameValue = drv["Name"].ToString();

            if (!string.IsNullOrWhiteSpace(nameValue))
            {
                NamesSelected[cnt] = nameValue;
                lvNamesSelected.Items.Add(nameValue);
                NamesSelected[cnt] = drv["ID"].ToString();

                btnContinue.Enabled = true;
            }
           // cbNamesOnFile.DroppedDown = true;
        }
        private void CenterControl(Control ctrl, int y)
        {
            // Calculate the position to center the control
            int x = (ClientSize.Width - ctrl.Width) / 2;
            // Dim y As Integer = () \ 2

            // Set the control's position
            ctrl.Location = new Point(x, y);
        }
        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (Exclusive.Checked && NamesSelected != null && NamesSelected.Length > 0)
            {
                // Replace first two characters with "Ex"
                if (NamesSelected[0].Length >= 2)
                    NamesSelected[0] = "Ex" + NamesSelected[0].Substring(2);
            }

            var thumbForm = new Sthumb
            {
                NamesSelected = NamesSelected
            };
            thumbForm.Show();
        }

        private void MenuItemPeople_Click(object sender, EventArgs e)
        {
            var strt = new Start();
            strt.Show();
        }
        private void MenuItemClear_Click(object sender, EventArgs e)
        {
            lvNamesSelected.Items.Clear();
            for (int i = 1, loopTo = NamesSelected.Length - 1; i <= loopTo; i++)
                NamesSelected[i] = (-1).ToString();
            Refresh();
        }
        private void MenuitemAdd_Click(object sender, EventArgs e)
        {
            var AddP = new AddPhoto();
            AddP.Show();
        }
        private void Exclusive_checked(object sender, EventArgs e)
        {
            if (NamesSelected == null || NamesSelected.Length == 0 || NamesSelected[0].Length < 2)
                return; // Safety check to avoid runtime errors

            if (Exclusive.Checked)
            {
                // Replace first two characters with "Ex"
                NamesSelected[0] = "Ex" + NamesSelected[0].Substring(2);
            }
            else
            {
                // Replace first two characters with "Al"
                NamesSelected[0] = "Al" + NamesSelected[0].Substring(2);
            }
        }
        
        private void ClearSelected_Click(object sender, EventArgs e)
        {
            lvNamesSelected.Items.Clear();
            NamesSelected[1] = "99999";
            NamesSelected[2] = "99999";
            NamesSelected[3] = "99999";
            NamesSelected[4] = "99999";
            NamesSelected[5] = "99999";
            cnt = 0;
        }
        private void Sortold_click(object sender, EventArgs e)
        {
            if (NamesSelected != null && NamesSelected.Length > 0 && NamesSelected[0].Length >= 6)
            {
                NamesSelected[0] = NamesSelected[0].Substring(0, 2) + ":Old" + NamesSelected[0].Substring(6);
            }

            lb2.Text = "(Old to New)";
            cbNamesOnFile.DroppedDown = true;
        }

        private void SortNew_click(object sender, EventArgs e)
        {
            if (NamesSelected != null && NamesSelected.Length > 0 && NamesSelected[0].Length >= 6)
            {
                NamesSelected[0] = NamesSelected[0].Substring(0, 2) + ":New" + NamesSelected[0].Substring(6);
            }
            lb2.Text = "(New to Old)";
            cbNamesOnFile.DroppedDown = true;
        }
        private void Exitapp(object sender, EventArgs e)
        {
            string qry = "SELECT PDateEntered FROM Pictures ORDER BY PDateEntered DESC LIMIT 1";
            connection = Manager.GetConnection();

            DateTime lastDate = new DateTime(2000, 1, 1); // Default fallback

            using (connection)
            {
                var command = new SQLiteCommand(qry, connection);
                var result = command.ExecuteScalar();

                if (result != null && DateTime.TryParse(result.ToString(), out DateTime parsedDate))
                {
                    lastDate = parsedDate;
                }
            }

            if (lastDate.Date == DateTime.Today.Date)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Do you want to backup the database?",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (dialogResult == DialogResult.Yes)
                {
                    string connectionString = SharedCode.GetConnectionString();
                    string filename = $"FamAlbum{DateTime.Today.Month}{DateTime.Today.Day}{DateTime.Today.Year}.bak";
                    string backupPath = BackupRest.GetBackupPath() + filename;

                    BackupRest.BackupSQLiteDatabase();
                }
            }

            Application.Exit();
        }

    }
>>>>>>> 7cba59675801a0b8e862e7f0276de92de193daa6
}