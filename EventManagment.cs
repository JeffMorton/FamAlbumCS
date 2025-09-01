using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{
    public partial class EventManagmentType
    {

        private TextBox txtEvent = new TextBox();
        private TextBox txtEventDetails = new TextBox();
        private TextBox txtSearch = new TextBox();
        private Label lblEventCount = new Label();
        private Label lblEventID = new Label();
        private Label lblEvent = new Label();
        private Label lblFindEvent = new Label();
        private Label lblEventDetails = new Label();
        private Label lblSearch = new Label();
        private MenuStrip menuStrip = new MenuStrip();
        private ListView lvSearch;
        private ComboBox cbEventsOnFile;
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();
        private int Id;
        private int Count;
        private Button btnSave;
        private Button btnDelete;
        private Button btnSearch;
        private Button btnCopy;
        private Panel rhp = new Panel();
        private Panel lhp = new Panel();
        private int rpw;
        private DataTable dt = new DataTable();

        public EventManagmentType()
        {
            lvSearch = new ListView();
            cbEventsOnFile = new ComboBox();
            btnSave = new Button();
            btnDelete = new Button();
            btnSearch = new Button();
            btnCopy = new Button();
            InitializeComponent();
        }
        private void EventManagment(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            WindowState = FormWindowState.Maximized;
            {
                ref var withBlock = ref lhp;
                withBlock.Left = 0;
                withBlock.Top = 125;
                withBlock.Width = screenWidth / 2;
                withBlock.Height = screenHeight - 125;
                withBlock.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                withBlock.AutoScroll = true;
            }
            Controls.Add(lhp);

            {
                ref var withBlock1 = ref rhp;
                withBlock1.Left = lhp.Right; // ensures it starts right after lhp
                withBlock1.Top = 125;
                withBlock1.Width = screenWidth - lhp.Width;
                withBlock1.Height = screenHeight - 125;
                withBlock1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                withBlock1.AutoScroll = true;
            }
            rpw = rhp.Width;
            Controls.Add(rhp);

            int lpw = lhp.Width;
            int lph = lhp.Height;

            var title = new Label()
            {
                Text = "Family Album",
                Font = new Font("segoe", 40f),
                Size = new Size(screenWidth, 60),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var subtitle = new Label()
            {
                Text = "Event  Manager",
                Font = new Font("segoe", 20f),
                Size = new Size(screenWidth, 40),
                Location = new Point(0, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(title);
            Controls.Add(subtitle);

            menuStrip = fmmenus.fmenus();
            Controls.Add(menuStrip);

            var menuItemExit = new ToolStripMenuItem("Exit") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
            menuItemExit.Click += MenuItemExit_Click;
            // Add the Exit item to the MenuStrip
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
            menuStrip.Items.RemoveAt(0);
            menuStrip.Items.Insert(0, menuItemExit);


            {
                ref var withBlock2 = ref lblFindEvent;
                withBlock2.Height = 20;
                withBlock2.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock2.Location = new Point(10, 100);
                withBlock2.AutoSize = true;
                withBlock2.Visible = true;
                withBlock2.Text = "Select Event:";
            }

            {
                var withBlock3 = cbEventsOnFile;
                withBlock3.Location = new Point(1, 130);
                withBlock3.Size = new Size((int)Math.Round(0.8d * rpw), 18);
                withBlock3.Font = new Font(cbEventsOnFile.Font.FontFamily, 12f);
                withBlock3.MaxDropDownItems = 6;
                withBlock3.DropDownHeight = 150;
            }
            rhp.Controls.Add(cbEventsOnFile);

            dt.Columns.Add("Event", typeof(string));
            dt.Columns.Add("ID", typeof(string));

            {
                ref var withBlock4 = ref lblEvent;
                withBlock4.Height = 40;
                withBlock4.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock4.Location = new Point(10, 280);
                withBlock4.AutoSize = true;
                withBlock4.Visible = true;
                withBlock4.Text = "Event:";
            }
            {
                ref var withBlock5 = ref txtEvent;
                withBlock5.Location = new Point(1, 305);
                withBlock5.Visible = true;
                withBlock5.Width = (int)Math.Round(0.8d * rpw);
                withBlock5.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock5.Height = 40;
                withBlock5.AutoSize = true;
            }

            {
                ref var withBlock6 = ref lblEventDetails;
                withBlock6.Height = 40;
                withBlock6.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock6.Location = new Point(1, 340);
                withBlock6.AutoSize = true;
                withBlock6.Visible = true;
                withBlock6.Text = "Event Details:";
            }
            {
                ref var withBlock7 = ref txtEventDetails;
                withBlock7.Location = new Point(1, 370);
                withBlock7.Width = (int)Math.Round(0.8d * rpw);
                withBlock7.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock7.Height = 100;
                // .AutoSize = True
                withBlock7.Multiline = true;
            }
            {
                ref var withBlock8 = ref lblEventID;
                withBlock8.Height = 40;
                withBlock8.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock8.Location = new Point(201, 470);
                withBlock8.AutoSize = true;
                withBlock8.Visible = true;
            }
            {
                ref var withBlock9 = ref lblEventCount;
                withBlock9.Height = 40;
                withBlock9.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock9.Location = new Point(1, 470);
                withBlock9.AutoSize = true;
                withBlock9.Visible = true;
            }
            {
                ref var withBlock10 = ref lblSearch;
                withBlock10.Height = 20;
                withBlock10.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock10.Location = new Point(10, 100);
                withBlock10.AutoSize = true;
                withBlock10.Visible = true;
                withBlock10.Text = "Enter search term:";
            }
            {
                ref var withBlock11 = ref txtSearch;
                withBlock11.Location = new Point(10, 130);
                withBlock11.Visible = true;
                withBlock11.Width = (int)Math.Round(0.8d * rpw);
                withBlock11.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock11.Height = 30;
                withBlock11.AutoSize = true;
            }
            {
                var withBlock12 = lvSearch;
                withBlock12.Location = new Point(10, 265);
                withBlock12.Size = new Size((int)Math.Round(0.8d * rpw), 400);
                withBlock12.Font = new Font("Arial", 12f);
                withBlock12.View = View.Details;
                withBlock12.Columns.Add("Event", (int)Math.Round(0.4d * rpw));
                withBlock12.Columns.Add("Event Detail", (int)Math.Round(0.4d * rpw));
            }

            {
                var withBlock13 = btnSearch;
                withBlock13.Text = "Search";
                withBlock13.Location = new Point(240, 175);
                withBlock13.BackColor = Color.LightBlue;
                withBlock13.ForeColor = Color.DarkBlue;
                withBlock13.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock13.Size = new Size(250, 50);
                withBlock13.Enabled = true;
            }
            int btp = (int)Math.Round(txtEvent.Left + txtEvent.Width / 2d - 125d);
            {
                var withBlock14 = btnSave;
                withBlock14.Text = "Save";
                withBlock14.Location = new Point(btp, 495);
                withBlock14.BackColor = Color.LightBlue;
                withBlock14.ForeColor = Color.DarkBlue;
                withBlock14.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock14.Size = new Size(250, 50);
                withBlock14.Enabled = true;
            }
            {
                var withBlock15 = btnDelete;
                withBlock15.Text = "Delete";
                withBlock15.Location = new Point(btp, 545);
                withBlock15.BackColor = Color.LightBlue;
                withBlock15.ForeColor = Color.DarkBlue;
                withBlock15.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock15.Size = new Size(250, 50);
                withBlock15.Enabled = true;
            }
            {
                var withBlock16 = btnCopy;
                withBlock16.Text = "Copy Event Files";
                withBlock16.Location = new Point(btp, 595);
                withBlock16.BackColor = Color.LightBlue;
                withBlock16.ForeColor = Color.DarkBlue;
                withBlock16.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock16.Size = new Size(250, 50);
                withBlock16.Enabled = true;
            }

            FillcbEventsOnFile();
            rhp.Controls.Add(lblFindEvent);
            rhp.Controls.Add(lblEvent);
            rhp.Controls.Add(lblEventDetails);
            rhp.Controls.Add(txtEvent);
            rhp.Controls.Add(txtEventDetails);
            rhp.Controls.Add(lblEventCount);
            rhp.Controls.Add(btnSave);
            rhp.Controls.Add(lblEventID);
            rhp.Controls.Add(btnCopy);
            rhp.Controls.Add(btnDelete);
            lhp.Controls.Add(lblSearch);
            lhp.Controls.Add(txtSearch);
            lhp.Controls.Add(btnSearch);
            lhp.Controls.Add(lvSearch);
            btnSave.Click += btnSave_click;
            btnCopy.Click += btnCopy_click;
            btnDelete.Click += btnDelete_click;
            btnSearch.Click += btnSearch_click;
            cbEventsOnFile.SelectionChangeCommitted += cbEventsOnFile_SelectedChanedCommitted;
        }

        private void cbEventsOnFile_SelectedChanedCommitted(object sender, EventArgs e)
        {
            // Get selected item(s) from ComboBox and store them in the array

            DataRowView drv = cbEventsOnFile.SelectedItem as DataRowView;
            if (drv is not null && !string.IsNullOrEmpty(Strings.Trim(drv["Event"].ToString())))
            {
                Id = Conversions.ToInteger(drv["Id"]);
            }

            connection = Manager.GetConnection();
            string qryEvent = "Select ID,neName,neRelation from NameEvent where neType ='E' and ID=@ID";

            using (connection)
            {
                var command = new SQLiteCommand(qryEvent, connection);
                command.Parameters.AddWithValue("@ID", Id);
                try
                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        txtEvent.Text = reader["neName"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("neRelation")))
                        {
                            txtEventDetails.Text = reader["neRelation"].ToString();
                        }
                        else
                        {
                            txtEventDetails.Text = "";
                        }
                    }

                    reader.Close();
                    string qryCnt = "select count(npID) from NamePhoto where npID= @ID";
                    var command1 = new SQLiteCommand(qryCnt, connection);
                    command1.Parameters.AddWithValue("@ID", Id);
                    Count = Conversions.ToInteger(command1.ExecuteScalar());

                    lblEventCount.Text = "Number of image: " + Count;
                    lblEventID.Text = "Event ID: " + Id.ToString();
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

        }
        private void btnSave_click(object sender, EventArgs e)
        {
            connection = Manager.GetConnection();
            string qryEvent = "Update NameEvent set neName =@event, neRelation =@details where ID = @ID";
            int re;
            using (connection)
            {
                var command = new SQLiteCommand(qryEvent, connection);
                command.Parameters.AddWithValue("@ID", Id);
                command.Parameters.AddWithValue("@event", txtEvent.Text);
                command.Parameters.AddWithValue("@details", txtEventDetails.Text);
                try
                {
                    re = command.ExecuteNonQuery();
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

        }
        private void FillcbEventsOnFile()
        {
            dt.Clear();
            string qryEvent = "Select neName,ID from NameEvent where neType ='E' order by neName";

            connection = Manager.GetConnection();
            using (connection)
            {
                var command = new SQLiteCommand(qryEvent, connection);
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

            cbEventsOnFile.DataSource = dt;
            cbEventsOnFile.DisplayMember = "Event";
            cbEventsOnFile.ValueMember = "ID";
            txtEvent.Text = "";
            txtEventDetails.Text = "";
        }
        private void btnDelete_click(object sender, EventArgs e)
        {
            if (Count != 0)
            {
                MessageBox.Show("Cannot delete Event with Pictures");
                return;
            }
            else
            {
                connection = Manager.GetConnection();
                string qryEvent = "Delete from NameEvent  where ID = @ID";
                int re;
                using (connection)
                {
                    var command = new SQLiteCommand(qryEvent, connection);
                    command.Parameters.AddWithValue("@ID", Id);

                    try
                    {
                        re = command.ExecuteNonQuery();
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
            }
            FillcbEventsOnFile();
        }
        private void MenuItemExit_Click(object sender, EventArgs e)
        {

            Close();
        }
        private void btnSearch_click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Search terms cannot be empty");
                return;
            }
            lvSearch.Items.Clear();
            connection = Manager.GetConnection();

            string qrySearch = "SELECT neName, neRelation FROM NameEvent WHERE neType='E' AND neName LIKE @term";
            using (var command = new SQLiteCommand(qrySearch, connection))
            {
                command.Parameters.AddWithValue("@term", "%" + txtSearch.Text + "%");
                var reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        string nameVal = reader["neName"].ToString();
                        string relVal = reader["neRelation"].ToString();

                        var item = new ListViewItem(nameVal);
                        item.SubItems.Add(relVal);
                        lvSearch.Items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: " + ex.Message);
                }
            }
        }
        private void btnCopy_click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            string DDir = SharedCode.GetDefaultDir();
            string destinationpath;
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var connection = Manager.GetConnection();
                    string qry = "Select npFilename from NamePhoto where npID =@EventID";
                    using (connection)
                    {
                        var command = new SQLiteCommand(qry, connection);
                        command.Parameters.AddWithValue("@EventID", Id);
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            destinationpath = Path.Combine(folderDialog.SelectedPath, Path.GetFileName(reader["npFilename"].ToString()));
                            File.Copy(DDir + reader["npFilename"].ToString(), destinationpath, true);
                        }
                    }
                    MessageBox.Show("Files copied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}