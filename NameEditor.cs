using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{

    public partial class NameEditor
    {
        private Panel lfp = new Panel();
        private Panel rhp = new Panel();
        private TextBox txtName = new TextBox();
        private TextBox txtRelation = new TextBox();
        private TextBox txtSearch = new TextBox();
        private Label lblNameCount = new Label();
        private Label lblName = new Label();
        private Label lblFindName = new Label();
        private Label lblRelation = new Label();
        private Label lblSearch = new Label();
        private MenuStrip menuStrip = new MenuStrip();
        private ListView lvSearch;
        private ComboBox cbNamesOnFile;
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();
        private int Id;
        private int Count;
        private Button btnSave;
        private Button btnDelete;
        private Button btnSearch;
        private DataTable dt = new DataTable();

        public NameEditor()
        {
            lvSearch = new ListView();
            cbNamesOnFile = new ComboBox();
            btnSave = new Button();
            btnDelete = new Button();
            btnSearch = new Button();
            InitializeComponent();
        }
        private void NameManagment(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            WindowState = FormWindowState.Maximized;
            var lhp = new Panel()
            {
                Left = 0,
                Top = 123,
                Width = screenWidth / 2,
                Height = screenHeight - 123,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Visible = true,
                AutoScroll = true
            };

            Controls.Add(lhp);

            var rhp = new Panel()
            {
                Left = lhp.Right, // ensures it starts right after lhp
                Top = 123,
                Width = screenWidth - lhp.Width,
                Height = screenHeight - 123,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = true,
                AutoScroll = true
            };
            Controls.Add(rhp);
            int rpw = rhp.Width;
            int lpw = lhp.Width;
            int lph = lhp.Height;
            Controls.Add(rhp);
            var title = new Label()
            {
                Text = "Family Album",
                Font = new Font("segoe", 40f),
                Size = new Size(screenWidth, 65),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(title);
            var subtitle = new Label()
            {
                Text = "Name  Manager",
                Font = new Font("segoe", 30f),
                Size = new Size(screenWidth, 60),
                Location = new Point(0,72),
                TextAlign = ContentAlignment.MiddleCenter
            };
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
                ref var withBlock = ref lblFindName;
                withBlock.Height = 20;
                withBlock.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock.Location = new Point(10, 100);
                withBlock.AutoSize = true;
                withBlock.Visible = true;
                withBlock.Text = "Select Name:";
            }


            {
                var withBlock1 = cbNamesOnFile;
                withBlock1.Location = new Point(1, 130);
                withBlock1.Size = new Size((int)Math.Round(0.8d * rpw), 18);
                withBlock1.Font = new Font(cbNamesOnFile.Font.FontFamily, 12f);
                withBlock1.MaxDropDownItems = 6;
                withBlock1.DropDownHeight = 150;
            }
            cbNamesOnFile.SelectionChangeCommitted += CbNamesOnFile_SelectionChangeCommitted;
            rhp.Controls.Add(cbNamesOnFile);

            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("ID", typeof(string));


            {
                ref var withBlock2 = ref lblName;
                withBlock2.Height = 40;
                withBlock2.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock2.Location = new Point(10, 270);
                withBlock2.AutoSize = true;
                withBlock2.Visible = true;
                withBlock2.Text = "Name:";
            }
            {
                ref var withBlock3 = ref txtName;
                withBlock3.Location = new Point(1, 300);
                withBlock3.Visible = true;
                withBlock3.Width = (int)Math.Round(0.8d * rpw);
                withBlock3.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock3.Height = 40;
                withBlock3.AutoSize = true;
            }

            {
                ref var withBlock4 = ref lblRelation;
                withBlock4.Height = 40;
                withBlock4.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock4.Location = new Point(1, 330);
                withBlock4.AutoSize = true;
                withBlock4.Visible = true;
                withBlock4.Text = "Relationship:";
            }
            {
                ref var withBlock5 = ref txtRelation;
                withBlock5.Location = new Point(1, 350);
                withBlock5.Width = (int)Math.Round(0.8d * rpw);
                withBlock5.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock5.Height = 100;
                // .AutoSize = True
                withBlock5.Multiline = true;
            }

            {
                ref var withBlock6 = ref lblNameCount;
                withBlock6.Height = 40;
                withBlock6.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock6.Location = new Point(1, 470);
                withBlock6.AutoSize = true;
                withBlock6.Visible = true;
            }

            {
                ref var withBlock7 = ref lblSearch;
                withBlock7.Height = 20;
                withBlock7.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock7.Location = new Point(10, 100);
                withBlock7.AutoSize = true;
                withBlock7.Visible = true;
                withBlock7.Text = "Enter search term:";
            }
            {
                ref var withBlock8 = ref txtSearch;
                withBlock8.Location = new Point(10, 130);
                withBlock8.Visible = true;
                withBlock8.Width = (int)Math.Round(0.8d * rpw);
                withBlock8.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock8.Height = 30;
                withBlock8.AutoSize = true;
            }
            {
                var withBlock9 = lvSearch;
                withBlock9.Location = new Point(10, 265);
                withBlock9.Size = new Size((int)Math.Round(0.8d * rpw), 400);
                withBlock9.Font = new Font("Arial", 12f);
                withBlock9.View = View.Details;
                withBlock9.Columns.Add("Name", (int)Math.Round(0.4d * rpw));
                withBlock9.Columns.Add("Relation", (int)Math.Round(0.4d * rpw));
            }

            {
                var withBlock10 = btnSearch;
                withBlock10.Text = "Search";
                withBlock10.Location = new Point(240, 175);
                withBlock10.BackColor = Color.LightBlue;
                withBlock10.ForeColor = Color.DarkBlue;
                withBlock10.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock10.Size = new Size(250, 50);
                withBlock10.Enabled = true;

            }
            int btp = (int)Math.Round(txtName.Left + txtName.Width / 2d - 125d);
            {
                var withBlock11 = btnSave;
                withBlock11.Text = "Save";
                withBlock11.Location = new Point(btp, 490);
                withBlock11.BackColor = Color.LightBlue;
                withBlock11.ForeColor = Color.DarkBlue;
                withBlock11.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock11.Size = new Size(250, 50);
                withBlock11.Enabled = true;
            }
            {
                var withBlock12 = btnDelete;
                withBlock12.Text = "Delete";
                withBlock12.Location = new Point(btp, 540);
                withBlock12.BackColor = Color.LightBlue;
                withBlock12.ForeColor = Color.DarkBlue;
                withBlock12.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock12.Size = new Size(250, 50);
                withBlock12.Enabled = true;
            }
            FillcbNamesOnFile();
            rhp.Controls.Add(lblFindName);
            rhp.Controls.Add(lblName);
            rhp.Controls.Add(lblRelation);
            rhp.Controls.Add(txtName);
            rhp.Controls.Add(txtRelation);
            rhp.Controls.Add(lblNameCount);
            rhp.Controls.Add(btnSave);
            rhp.Controls.Add(btnDelete);
            lhp.Controls.Add(lblSearch);
            lhp.Controls.Add(txtSearch);
            lhp.Controls.Add(btnSearch);
            lhp.Controls.Add(lvSearch);
            btnSave.Click += btnSave_click;
            btnDelete.Click += btnDelete_click;
            btnSearch.Click += btnSearch_click;

        }

        private void CbNamesOnFile_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Get selected item(s) from ComboBox and store them in the array

            DataRowView drv = cbNamesOnFile.SelectedItem as DataRowView;
            if (drv is not null && !string.IsNullOrEmpty(Strings.Trim(drv["Name"].ToString())))
            {
                Id = Conversions.ToInteger(drv["Id"]);
            }
            connection = Manager.GetConnection();
            string qryName = "Select ID,neName,neRelation from NameEvent where neType ='N' and ID=@ID";

            using (connection)
            {
                var command = new SQLiteCommand(qryName, connection);
                command.Parameters.AddWithValue("@ID", Id);
                try
                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        txtName.Text = reader["neName"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("neRelation")))
                        {
                            txtRelation.Text = reader["neRelation"].ToString();
                        }
                        else
                        {
                            txtRelation.Text = "";
                        }
                    }

                    reader.Close();
                    string qryCnt = "select count(npID) from NamePhoto where npID= @ID";
                    var command1 = new SQLiteCommand(qryCnt, connection);
                    command1.Parameters.AddWithValue("@ID", Id);
                    Count = Conversions.ToInteger(command1.ExecuteScalar());

                    lblNameCount.Text = "Number of images: " + Count;
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
            string qryName = "Update NameEvent set neName =@Name, neRelation =@details where ID = @ID";
            int re;
            using (connection)
            {
                var command = new SQLiteCommand(qryName, connection);
                command.Parameters.AddWithValue("@ID", Id);
                command.Parameters.AddWithValue("@Name", txtName.Text);
                command.Parameters.AddWithValue("@details", txtRelation.Text);
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
        private void FillcbNamesOnFile()
        {
            dt.Clear();
            string qryName = "Select neName,ID from NameEvent where neType ='N' order by neName";

            connection = Manager.GetConnection();
            using (connection)
            {
                var command = new SQLiteCommand(qryName, connection);
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
            txtName.Text = "";
            txtRelation.Text = "";
        }
        private void btnDelete_click(object sender, EventArgs e)
        {
            if (Count != 0)
            {
                MessageBox.Show("Cannot delete Name with Pictures");
                return;
            }
            else
            {
                connection = Manager.GetConnection();
                string qryName = "Delete from NameEvent  where ID = @ID";
                int re;
                using (connection)
                {
                    var command = new SQLiteCommand(qryName, connection);
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
            FillcbNamesOnFile();
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

            string qrySearch = "SELECT neName, neRelation FROM NameEvent WHERE neType='N' AND neName LIKE @term";
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
        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}