using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{
    public partial class NewName
    {
        private string _parentFormType;  // To know which form called us
        private string _sFileName1;       // To store the incoming filename

        public NewName(string sFileName, string fromForm)
        {
            btnAdd = new Button();
            InitializeComponent();
            _sFileName1 = sFileName;
            _parentFormType = fromForm;
            // Maybe display sFileName somewhere in your NewName form
        }
        public Label Label1 { get; set; } = new Label();

        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();

        public string SFileName => _sFileName1;
        private TextBox TXTFullName = new TextBox()
        {
            Location = new Point(250, 200),
            Font = new Font("Arial", 12f),
            Width = 600,
            Height = 30,
            AutoSize = true
        };

        private TextBox TXTPosition = new TextBox()
        {
            Location = new Point(250, 600),
            Font = new Font("Arial", 12f),
            Width = 30,
            Height = 30,
            AutoSize = true
        };

        private TextBox TXTRelation = new TextBox()
        {
            Location = new Point(250, 250),
            Font = new Font("Arial", 12f),
            Width = 600,
            Height = 300,
            Multiline = true,
            AutoSize = true
        };
        private Displayinfo mainForm;
        public NewName(Displayinfo parentForm)
        {
            btnAdd = new Button();
            InitializeComponent();
            mainForm = parentForm;
        }
        private MenuStrip menuStrip = new MenuStrip();
        private Button btnAdd;
        private void NewName_Load(object sender, EventArgs e)
        {
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 900;
            Height = 1000;

            Label @init = new Label();
            var lblTitle = (@init.Font = new Font(@init.Font.FontFamily, 24f), @init.Text = "Please fill in all fields", @init.Location = new Point(50, 75), @init.Width = 800, @init.Height = 60, @init.TextAlign = ContentAlignment.MiddleCenter, @init).@init;


            var menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += MenuItemExit_click;
            // Add the Exit item to the MenuStrip
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);

            Label @init1 = new Label();
            var lblFullName = (@init1.Font = new Font(@init1.Font.FontFamily, 12f), @init1.Text = "Full Name:", @init1.Location = new Point(100, 200), @init1.Width = 140, @init1.Height = 30, @init1.TextAlign = ContentAlignment.MiddleRight, @init1).@init1;

            Label @init2 = new Label();
            var lblRelation = (@init2.Font = new Font(@init2.Font.FontFamily, 12f), @init2.Text = "Relation:", @init2.Location = new Point(75, 250), @init2.Width = 140, @init2.Height = 30, @init2.TextAlign = ContentAlignment.MiddleRight, @init2).@init2;

            var lblRelation1 = new Label()
            {
                Font = new Font("Arial Novw Light", 10f),
                Text = "For example, 'Friend of' or 'Relative of'",
                Location = new Point(295, 550),
                AutoSize = true
            };

            Label @init3 = new Label();
            var lblPosition = (@init3.Font = new Font(@init3.Font.FontFamily, 12f), @init3.Text = "Position in list:", @init3.Location = new Point(50, 600), @init3.Width = 190, @init3.Height = 30, @init3.TextAlign = ContentAlignment.MiddleRight, @init3).@init3;

            var lblPosition1 = new Label()
            {
                Font = new Font("Arial Novw Light", 10f),
                Text = "Please list people from left to right",
                Location = new Point(300, 600),
                Width = 400,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            {
                var withBlock = btnAdd;
                withBlock.Text = "Save";
                withBlock.BackColor = Color.LightBlue;
                withBlock.ForeColor = Color.DarkBlue;
                withBlock.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock.Size = new Size(350, 50);
                withBlock.Location = new Point(190, 750);
            }

            Controls.Add(lblFullName);
            Controls.Add(TXTFullName);
            Controls.Add(lblRelation);
            Controls.Add(TXTRelation);
            Controls.Add(lblRelation1);
            Controls.Add(btnAdd);
            Controls.Add(lblTitle);
            Controls.Add(lblPosition);
            Controls.Add(TXTPosition);
            Controls.Add(lblPosition1);
            btnAdd.Click += btnAdd_click;
        }
        private void MenuItemExit_click(object sender, EventArgs e)
        {


            Close();
        }
        private void btnAdd_click(object sender, EventArgs e)
        {
            string namelist = "";
            string newKey = "0";
            var namecount = default(int);
            if (!Information.IsNumeric(TXTPosition.Text))
            {
                MessageBox.Show("You must indicate where this person in (from left to right in this picture");
            }
            else
            {
                newKey = SharedCode.AddNewName(TXTFullName.Text, TXTRelation.Text);

                connection = Manager.GetConnection();
                using (connection)
                {
                    var transaction = connection.BeginTransaction();
                    try
                    {

                        // insert into NamePhoto
                        var command2 = new SQLiteCommand("INSERT INTO NamePhoto (npID, npFilename) VALUES (@selectedPerson, @filename1);", connection, transaction);
                        command2.Parameters.AddWithValue("@selectedPerson", newKey);
                        command2.Parameters.AddWithValue("@filename1", _sFileName1);
                        command2.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    // MessageBox.Show("Transaction committed successfully.")
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show("Database error: " + ex.Message);
                        transaction.Rollback();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                        transaction.Rollback();
                    }
                }
                namelist = SharedCode.GetPPeopleList(_sFileName1, ref namecount);
                namelist = SharedCode.ModifyPeopleList(namelist, Conversions.ToInteger(TXTPosition.Text) - 1, 1, newKey);

            }
            try
            {
                if (_parentFormType == "DisplayInfo")
                {
                    Displayinfo parentForm = Parent.FindForm() as Displayinfo;
                    if (parentForm is not null)
                    {
                        parentForm.Label1.Text = namelist;
                    }
                }
                else if (_parentFormType == "AddPhoto")
                {
                    AddPhoto parentForm = Parent.FindForm() as AddPhoto;
                    if (parentForm is not null)
                    {
                        parentForm.Label1.Text = newKey + "|" + TXTPosition.Text;
                    }
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating parent form: " + ex.Message);
            }

            Close();
        }

    }

}