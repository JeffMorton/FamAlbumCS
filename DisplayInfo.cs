using FamAlbum.My;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FamAlbum
{


    public partial class Displayinfo
    {
        private ListView lvNames;
        public string SFileName { get; set; }
        private PictureBox picBox = new PictureBox();
        private MenuStrip menuStrip = new MenuStrip();
        private List<string> NameArray;
        private Button btnAdd;
        private Button btnNew;
        private Button BtnDelete;
        private Button BtnUpdateThumb;
        private Button btnRestart;
        private Button btnEmbed;
        private TextBox Tposition = new TextBox();
        private ComboBox combobox1 = new ComboBox();
        private Label lblPosition = new Label();
        private DataTable dt = new DataTable();


        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();
        private string mfilename;
        private string MemberID;
        private int TypeI;
        private bool Formchanged = false;
        public Label Label1 = new Label()
        {
            Size = new Size(130, 50),
            Font = new Font("Arial", 12f, FontStyle.Bold),
            Visible = false,
            Location = new Point(1500, 700)
        };

        private TextBox txtRelation;

        public Displayinfo()
        {
            lvNames = new ListView();
            btnAdd = new Button();
            btnNew = new Button();
            BtnDelete = new Button();
            BtnUpdateThumb = new Button();
            btnEmbed = new Button();
            btnRestart = new Button();
            txtRelation = new TextBox()
            {
                Location = new Point(20, 812),
                Size = new Size(800, 300),
                Font = new Font("Arial", 12f, FontStyle.Regular),
                Multiline = true,
                BorderStyle = BorderStyle.None,
                Visible = false,
                AutoSize = true
            };
            TxtFullName = new TextBox()
            {
                Location = new Point(20, 750),
                Size = new Size(800, 30),
                Font = new Font("Arial", 12f, FontStyle.Bold),
                Multiline = false,
                Visible = false,
                AutoSize = true
            };
            btnUpdate = new Button()
            {
                Text = "Update",
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                Font = new Font("Arial", 12f, FontStyle.Bold),
                Size = new Size(220, 40),
                Location = new Point(252, 1130),
                Visible = false,
                AutoSize = true
            };


            btnCancel = new Button()
            {
                Text = "Cancel",
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                Font = new Font("Arial", 12f, FontStyle.Bold),
                Size = new Size(220, 40),
                Location = new Point(472, 1130),
                Visible = false,
                AutoSize = true
            };

            btnCopyFile = new Button();
            txtDescription = new TextBox();
            txtEvent = new TextBox();
            InitializeComponent();
        }
        static Label initLblRelate()
        {
            var init = new Label();
            return (init.Font = new Font(init.Font.FontFamily, 12f), init.Text = "Relation to Morton Family ", init.Location = new Point(20, 890), init.Visible = false, init.AutoSize = true, init).init;
        }

        private Label lblRelate = initLblRelate();
        private TextBox TxtFullName;

        static Label initLblName1()
        {
            var init = new Label();
            return (init.Font = new Font(init.Font.FontFamily, 12f), init.Size = new Size(800, 30), init.Text = "Name", init.Location = new Point(20, 720), init.Visible = false, init.AutoSize = true, init).init;
        }
        private Label lblName1 = initLblName1();
        private Button btnUpdate;
        private Button btnCancel;
        private Button btnCopyFile;

        private TextBox TxtMonth = new TextBox();
        private TextBox txtYear = new TextBox();
        private Label lblDescription = new Label();
        private TextBox txtDescription;
        private TextBox txtEvent;
        private TextBox txtEventDetails = new TextBox();
        private Label lblEvent = new Label();
       private string DDir;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView _videoView;
        private bool _mediaPlayerDisposed = false;
        private bool _libVLCDisposed = false;

        private Panel lhp = new Panel();
        private Panel rhp = new Panel();
        public event SubFormClosingEventHandler SubFormClosing;

        public delegate void SubFormClosingEventHandler(object sender, FormClosedEventArgs e);

        public void DisplayInfo_Load(object sender, EventArgs e)
        {

            WindowState = FormWindowState.Maximized;
            AutoScaleMode = AutoScaleMode.Font;
            IsMdiContainer = true;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            menuStrip = fmmenus.fmenus();
            var menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += MenuItemExit_Click;
            // Add the Exit item to the MenuStrip
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            menuStrip.Items.RemoveAt(0);
            menuStrip.Items.Insert(0, menuItemExit);
            Controls.Add(menuStrip);
            dt.Columns.Add("Names", typeof(string));
            dt.Columns.Add("Relation", typeof(string));
            btnRestart.Click += btnRestart_Click;

            Core.Initialize(); // Required once per app

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

           



            {
                ref var withBlock = ref lhp;
                withBlock.Left = 0;
                withBlock.Top = 80;
                withBlock.Width = screenWidth / 2;
                withBlock.Height = screenHeight - 80;
                withBlock.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                withBlock.AutoScroll = true;
            }
            Controls.Add(lhp);

            {
                ref var withBlock1 = ref rhp;
                withBlock1.Left = lhp.Right; // ensures it starts right after lhp
                withBlock1.Top = 80;
                withBlock1.Width = screenWidth - lhp.Width;
                withBlock1.Height = screenHeight - 80;
                withBlock1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                withBlock1.AutoScroll = true;
            }
            Controls.Add(rhp);

            int lpw = lhp.Width;
            int lph = lhp.Height;
            _videoView = new VideoView
            {
                MediaPlayer = _mediaPlayer,
                Name = "videoView",
                Size = new Size(lpw - 50, lph / 2), // Adjust as needed
                Location = new Point(40, 50) // Adjust as needed
            };

            rhp.Controls.Add(_videoView);

            var title = new Label()
            {
                Text = "Family Album",
                Font = new Font("segoe", 40f),
                Size = new Size(screenWidth, 60),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            DDir = SharedCode.GetDefaultDir();
            {
                var withBlock2 = lvNames;
                withBlock2.Size = new Size((int)Math.Round(lpw * 0.6d), 450);
                withBlock2.Location = new Point(10, 270);
                withBlock2.View = View.Details;
                withBlock2.FullRowSelect = true;
                withBlock2.Font = new Font("Arial", 12f, FontStyle.Regular);
            }
            lvNames.Columns.Add("FullName", (int)Math.Round(lvNames.Width / 2d - 5d), HorizontalAlignment.Left);
            lvNames.Columns.Add("Relation", (int)Math.Round(lvNames.Width / 2d - 5d), HorizontalAlignment.Left);
            int Rpont = lvNames.Left + lvNames.Width + 20;


            {
                ref var withBlock4 = ref lblPosition;
                withBlock4.Location = new Point(Rpont, lvNames.Top + lvNames.Height + 30);
                withBlock4.Size = new Size(200, 40);
                withBlock4.Text = "Position of new Person";
                withBlock4.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock4.Visible = false;
            }


            {
                ref var withBlock3 = ref Tposition;
                withBlock3.Location = new Point(Rpont+lblPosition.Width, lvNames.Top + lvNames.Height +25);
                withBlock3.Size = new Size(40, 40);
                withBlock3.Font = new Font("Arial", 14f, FontStyle.Regular);
                withBlock3.Visible = false;
            }

            {
                ref var withBlock5 = ref combobox1;
                withBlock5.Location = new Point(lvNames.Left, lvNames.Top + lvNames.Height + 30);
                withBlock5.Size = new Size(lvNames.Width, 21);
                withBlock5.Font = new Font("Arial", 12f);
                withBlock5.Visible = false;
            }


            {
                var withBlock6 = btnAdd;
                withBlock6.Text = "Add Names";
                withBlock6.BackColor = Color.LightBlue;
                withBlock6.ForeColor = Color.DarkBlue;
                withBlock6.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock6.Size = new Size(220, 40);
                withBlock6.Location = new Point(Rpont, 300);
            }
            lhp.Controls.Add(btnAdd);

            {
                var withBlock7 = btnNew;
                withBlock7.Text = "Add Name not in List";
                withBlock7.BackColor = Color.LightBlue;
                withBlock7.ForeColor = Color.DarkBlue;
                withBlock7.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock7.Size = new Size(220, 40);
                withBlock7.Location = new Point(Rpont, 350);
            }
            lhp.Controls.Add(btnNew);

            {
                var withBlock8 = btnCopyFile;
                withBlock8.Text = "Copy Image";
                withBlock8.BackColor = Color.LightBlue;
                withBlock8.ForeColor = Color.DarkBlue;
                withBlock8.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock8.Size = new Size(220, 40);
                withBlock8.Location = new Point(Rpont, 400);
                withBlock8.Visible = true;
            }
            {
                var withBlock9 = BtnDelete;
                withBlock9.Text = "Delete Image";
                withBlock9.Location = new Point(Rpont, 450);
                withBlock9.BackColor = Color.LightBlue;
                withBlock9.ForeColor = Color.DarkBlue;
                withBlock9.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock9.Size = new Size(220, 40);
                withBlock9.AutoSize = true;
            }
            {
                var withBlock10 = btnEmbed;
            }
            {
                var withBlock11 = BtnUpdateThumb;
                withBlock11.Text = "Update Thumbnail";
                withBlock11.Location = new Point(Rpont, 550);
                withBlock11.BackColor = Color.LightBlue;
                withBlock11.ForeColor = Color.DarkBlue;
                withBlock11.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock11.Size = new Size(220, 40);
                withBlock11.AutoSize = true;
            }


            connection = Manager.GetConnection();

            string fileName = SFileName;
            var command = new SQLiteCommand("SELECT  PDescription,PPeoplelist, PMonth, PYear,Pheight, Pwidth,PType FROM Pictures WHERE Pfilename = @name", connection);
            command.Parameters.AddWithValue("@name", fileName);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    TypeI = Conversions.ToInteger(reader["PType"]);
                    if (TypeI != 1)
                    {
                        picBox.Visible = false;
                        _videoView.Visible = true;
                        mfilename = DDir + SFileName;
                       
                        btnRestart.Visible = true;
                        rhp.Controls.Add(btnRestart);
                        Playvideo(DDir + SFileName);
                    }
                    else
                    {
                        int width = Conversions.ToInteger(reader["Pwidth"]);
                        int height = Conversions.ToInteger(reader["Pheight"]);
                        if (SFileName.Contains(DDir))
                        {
                            SFileName = SFileName.Replace(DDir, "");
                        }
                        // Display dimensions
                        // Force a true decoupling from the file stream
                        _videoView.Visible = false;
                        byte[] imgBytes = null;
                        try
                        {
                            imgBytes = File.ReadAllBytes(DDir + SFileName);
                            using (var ms = new MemoryStream(imgBytes))
                            {
                                var img = Image.FromStream(ms);
                                picBox.Image = new Bitmap(img); // Forces it into memory and closes file
                            }

                            {
                                ref var withBlock13 = ref picBox;
                                withBlock13.SizeMode = PictureBoxSizeMode.Zoom;
                                withBlock13.Width = lpw;
                                withBlock13.Location = new Point(lhp.Width - picBox.Width, 50);
                                withBlock13.Height = (int)Math.Round(0.95d * (screenHeight / 2d));
                                withBlock13.Visible = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }


                    var metadata = jsonlist.ReadJsonFromMediaFile(DDir + SFileName);

                    if (metadata is not null)
                    {
                        Console.WriteLine("Event: " + metadata.EventName);
                        Console.WriteLine("Details: " + metadata.EventDetails);
                        foreach (var p in metadata.People)
                            Console.WriteLine($"- {p.Name}: {p.Relationship}");
                    }
                    else
                    {
                        Console.WriteLine("No metadata found in image.");
                    }


                    mfilename = "";

                    string PList;
                    string nm = Conversions.ToString(reader["PPeoplelist"]);
                    if (nm is null || ReferenceEquals(nm, ""))
                    {
                    }
                    // NameArray(0) = "No names on file"
                    else
                    {
                        PList = Conversions.ToString(reader["PPeoplelist"]);
                        NameArray = PList.Split(',').ToList();
                    }

                    Label @init = new Label();
                    var lblPeoplelist = (@init.Font = new Font(@init.Font.FontFamily, 12f), @init.Text = "People In Picture", @init.Location = new Point(50, 240), @init.Width = 100, @init.Height = 30, @init.AutoSize = true, @init).@init;
                    Label @init1 = new Label();
                    var lblMonth = (@init1.Font = new Font(@init1.Font.FontFamily, 12f), @init1.Text = "Month of Picture: ", @init1.Location = new Point(50, 40), @init1.Width = 100, @init1.Height = 30, @init1.AutoSize = true, @init1).@init1;
                    {
                        ref var withBlock14 = ref TxtMonth;
                        withBlock14.Font = new Font(withBlock14.Font.FontFamily, 12f);
                        withBlock14.Text = reader.IsDBNull(reader.GetOrdinal("PMonth")) ? "0" : reader["PMonth"].ToString();
                        withBlock14.Location = new Point(260, 40);
                        withBlock14.Width = 100;
                        withBlock14.Height = 30;
                        withBlock14.AutoSize = true;
                    }

                    Label @init2 = new Label();
                    var lblYear = (@init2.Font = new Font(@init2.Font.FontFamily, 12f), @init2.Text = "Year of Picture:", @init2.Location = new Point(50, 90), @init2.Width = 200, @init2).@init2;

                    {
                        ref var withBlock15 = ref txtYear;
                        withBlock15.Font = new Font(withBlock15.Font.FontFamily, 12f);
                        withBlock15.Text = reader.IsDBNull(reader.GetOrdinal("Pyear")) ? "0" : reader["Pyear"].ToString();
                        withBlock15.Location = new Point(260, 90);
                        withBlock15.Size = new Size(100, 30);
                    }

                   

                    Label @init3 = new Label();
                    var lblFile = (@init3.Font = new Font(@init3.Font.FontFamily, 12f), @init3.Text = "Name && location  of Picture  file: " + SFileName, @init3.Location = new Point(50, 150), @init3.AutoSize = true, @init3).@init3;
                    Label @init4 = new Label();
                    var lblDimension = (@init4.Font = new Font(@init4.Font.FontFamily, 12f), @init4.Text = "Height && Width of Picture: " + Conversions.ToInteger(reader["Pheight"]) + "/" + Conversions.ToInteger(reader["Pwidth"]), @init4.Location = new Point(50, 200), @init4.AutoSize = true, @init4).@init4;
                    txtDescription.Text = reader["PDescription"].ToString();


                    lvNames.MouseUp += Listbox_Mouseup;
                    txtEvent.MouseUp += Event_Mouseup;
                    btnCancel.Click += btnCancel_click;
                    BtnUpdateThumb.Click += btnUpdateThumb_click;
                    btnUpdate.Click += btnUpdate_click;
                    BtnDelete.Click += btnDelete_click;
                    btnNew.Click += BtnNew_click;
                    btnAdd.Click += btnAdd_click;
                    btnRestart.Click += btnRestart_click;
                    SubFormClosing += SubformClosed;
                    btnCopyFile.Click += btnCopyFile_Click;
                    txtDescription.TextChanged += (se, ev) => Formchanged = true;
                    this.FormClosing += new FormClosingEventHandler(OnFormClosing);
                    Controls.Add(title);
                    lhp.Controls.Add(lblPeoplelist);
                    lhp.Controls.Add(lvNames);
                    lhp.Controls.Add(lblMonth);
                    lhp.Controls.Add(lblYear);
                    lhp.Controls.Add(lblFile);
                    lhp.Controls.Add(lblDimension);
                    lhp.Controls.Add(Label1);
                    lhp.Controls.Add(txtRelation);
                    lhp.Controls.Add(lblRelate);
                    lhp.Controls.Add(TxtFullName);
                    lhp.Controls.Add(lblName1);
                    lhp.Controls.Add(lblPosition);
                    lhp.Controls.Add(Tposition);
                    rhp.Controls.Add(txtDescription);
                    rhp.Controls.Add(lblDescription);
                    rhp.Controls.Add(txtEvent);
                    rhp.Controls.Add(lblEvent);
                    rhp.Controls.Add(txtEventDetails);
                    lhp.Controls.Add(TxtMonth);
                    lhp.Controls.Add(txtYear);
                    lhp.Controls.Add(btnCancel);
                    // lhp.Controls.Add(btnEmbed)
                    //lhp.Controls.Add(BtnUpdateThumb);
                    if (TypeI == 1)
                    {
                        rhp.Controls.Add(picBox);
                    }
                    else
                    {
                        rhp.Controls.Add(btnRestart);
                    }
                    lhp.Controls.Add(btnCopyFile);
                    if (!string.IsNullOrEmpty(mfilename))
                    {
                        // PlayVideo(mfilename)
                    }
                }
            }
            // reader.Close()

            FilllvNames();
            lhp.Controls.Add(btnUpdate);

            connection = Manager.GetConnection();
            using (connection)
            {
                var command1 = new SQLiteCommand("select neName,neRelation from NamePhoto inner join NameEvent on ID = npID where neType = 'E' and npFilename = @filename", connection);
                command1.Parameters.AddWithValue("@filename", fileName);
                txtEvent.Text = "Add Event";
                txtEvent.Visible = true;
                using (var reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        txtEvent.Text = Conversions.ToString(reader1["neName"]);
                        string relationValue = reader1.IsDBNull(reader1.GetOrdinal("neRelation")) ? "" : reader1["neRelation"].ToString().Trim();

                        if (relationValue == "NULL")
                        {
                            txtEventDetails.Visible = false;
                        }
                        else
                        {
                            txtEventDetails.Text = reader1["neRelation"].ToString();
                            txtEventDetails.Visible = true;
                        }
                    }
                }
            }
            int xCenter = 0;
            int yPosition = 0;
            int xleft = 0;
            int xWidth = 0;
            if (TypeI == 1)
            {
                // Calculate horizontal center of picbox
                xCenter = picBox.Location.X + picBox.Width / 2 - 800 / 2;
                xleft= picBox.Location.X;
                xWidth = picBox.Width;
                // Set the vertical position just below picbox
                yPosition = picBox.Location.Y + picBox.Height + 10; // Adjust 10 for spacing
            }
            else
            {    // Calculate horizontal center of picbox
                xCenter = _videoView.Location.X + _videoView.Width / 2 - 800 / 2;
                xleft = _videoView.Location.X; 
                xWidth= _videoView.Width;
                // Set the vertical position just below picbox
                yPosition = _videoView.Location.Y + _videoView.Height + 10; // Adjust 10 for spacing
            }
                {
                    ref var withBlock17 = ref lblEvent;
                withBlock17.Height = 40;
                withBlock17.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock17.Location = new Point(xleft, yPosition + 70);
                withBlock17.AutoSize = true;
                withBlock17.Visible = true;
                withBlock17.Text = "Event:";
            }
            {
                ref var withBlock18 = ref lblDescription;
                withBlock18.Height = 40;
                withBlock18.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock18.Location = new Point(xleft , yPosition +20);
                withBlock18.AutoSize = true;
                withBlock18.Visible = true;
                withBlock18.Size = new Size(40, 80);
                withBlock18.Text = "Description:";
            }
            {
                var withBlock19 = txtDescription;
                withBlock19.Location = new Point();
                withBlock19.Visible = true;
                withBlock19.Width = xWidth -lblDescription.Width -20;
                withBlock19.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock19.Height = 60;
                withBlock19.Location = new Point(xleft + lblDescription.Width, yPosition);

                withBlock19.Multiline = true;
                withBlock19.AutoSize = true;
                withBlock19.BorderStyle = BorderStyle.FixedSingle;
            }
            {
                {
                    var withBlock21 = txtEvent;
                    withBlock21.Location = new Point(xleft + lblDescription.Width, yPosition + 70);
                    withBlock21.Visible = true;
                    withBlock21.Width = 750;
                    withBlock21.Font = new Font("Arial", 12f, FontStyle.Regular);
                    withBlock21.Height = 40;
                    withBlock21.ReadOnly = true;
                    withBlock21.AutoSize = true;
                    withBlock21.BorderStyle = BorderStyle.None;
                }
                {
                    ref var withBlock22 = ref txtEventDetails;
                    withBlock22.Location = new Point(xleft + lblDescription.Width, yPosition + 120);
                    withBlock22.Width = 750;
                    withBlock22.Font = new Font("Arial", 12f, FontStyle.Regular);
                    withBlock22.Height = 100;
                    // .AutoSize = True
                    withBlock22.BorderStyle = BorderStyle.None;
                    withBlock22.ReadOnly = true;
                    withBlock22.Multiline = true;
                }

                if (TypeI != 1)
                {
                    picBox.Visible = false;

                    mfilename = DDir + SFileName;

                    {
                        var withBlock24 = btnRestart;
                       withBlock24.Text = "Restart";
                        withBlock24.BackColor = Color.LightBlue;
                        withBlock24.ForeColor = Color.DarkBlue;
                        withBlock24.Font = new Font("Arial", 12f, FontStyle.Bold);
                        withBlock24.Size = new Size(220, 40);
                        withBlock24.Location = new Point(lpw / 2 - 110, txtEventDetails.Top + txtEventDetails.Height + 20);
                        withBlock24.Visible = false;
                        withBlock24.AutoSize = true;
                    }

                    btnRestart.Visible = true;
                    rhp.Controls.Add(btnRestart);
                    Playvideo(DDir + SFileName);
                }
                else
                {
                    {
                        ref var withBlock25 = ref picBox;
                        withBlock25.SizeMode = PictureBoxSizeMode.Zoom;
                        withBlock25.Width = (int)Math.Round(screenWidth / 2d);
                        withBlock25.Location = new Point(lhp.Width - picBox.Width, 50);
                        withBlock25.Height = (int)Math.Round(0.95d * (screenHeight / 2d));
                        withBlock25.Visible = true;
                    }
                    Image img;
                    try
                    {
                        using (var fs = new FileStream(DDir + SFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            img = Image.FromStream(fs);
                        }
                    }
                    catch
                    {
                        SharedCode.ShowTextInPictureBox(picBox, "Picture Deleted");
                    }



                    mfilename = "";
                }
                lhp.Controls.Add(BtnDelete);
                TxtMonth.TextChanged += Datechanged;
                txtYear.TextChanged += Datechanged;
                txtDescription.TextChanged += DescriptionChanged;
            }
        }

        private void FilllvNames(object sender, EventArgs e)
        {
            lvNames.Items.Clear();
            string qryfixname = "select neName,neRelation  from NameEvent where ID= @Name ";
            if (NameArray is null)
                return;
            connection = Manager.GetConnection();
            for (int I = 0, loopTo = NameArray.Count - 1; I <= loopTo; I++)
            {
                try
                {
                    if (!(NameArray[I] == "0"))
                    {
                        var command1 = new SQLiteCommand(qryfixname, connection);
                        command1.Parameters.AddWithValue("@Name", NameArray[I]);
                        var reader1 = command1.ExecuteReader();
                        reader1.Read();

                        var item1 = new ListViewItem(reader1["neName"].ToString());
                        if (!reader1.IsDBNull(reader1.GetOrdinal("neRelation")))
                        {
                            item1.SubItems.Add(Convert.ToString(reader1["neRelation"]));
                            lvNames.Items.Add(item1);
                            reader1.Close();
                        }
                        else
                        {
                            item1.SubItems.Add(" ");
                            lvNames.Items.Add(item1);
                            reader1.Close();
                        }
                    }
                }
                catch
                {
                    // MessageBox.Show(ex.Message)
                }
                Tposition.Text = NameArray.Count.ToString();
            }
            connection.Close();
        }
        private void MenuItemExit_Click(object sender, EventArgs e)
        {

            Close();
        }
        private void btnAdd_click(object sender, EventArgs e)
        {
            combobox1.Visible = true;
            combobox1.Focus();
            // combobox1.DroppedDown = True
            lblPosition.Visible = true;
            Tposition.Visible = true;

            FillNamesAvail();
        }
        private void FillNamesAvail()
        {
            combobox1.DataSource = null;
            combobox1.Items.Clear();
            dt = SharedCode.FillNames();
            combobox1.SelectionChangeCommitted -= NameCombo_SelectionChangeCommitted;

            combobox1.DataSource = dt;
            combobox1.DisplayMember = "Names";
            combobox1.ValueMember = "ID";
            lhp.Controls.Add(combobox1);
            lhp.Controls.Add(lblPosition);
            lhp.Controls.Add(Tposition);
            combobox1.SelectedIndex = -1;
            combobox1.SelectionChangeCommitted += NameCombo_SelectionChangeCommitted;
            combobox1.Focus();
        }
        private void NameCombo_SelectionChangeCommitted(object sender, EventArgs e)
        {

            // Cast the sender to a ComboBox
            ComboBox comboBox = (ComboBox)sender;
            DataRowView selectedRow = (DataRowView)comboBox.SelectedItem;
            if (selectedRow is null)
                return;
            string selectedPerson = selectedRow["ID"].ToString();
            int insertIndex;
            if (comboBox.SelectedItem is not null)
            {
                try
                {
                    insertIndex = Conversions.ToInteger(Tposition.Text);
                }
                catch
                {
                    MessageBox.Show("Position has to be an Integer  ");
                    return;
                }
                NameArray = SharedCode.ModifyPeopleList(string.Join(",", NameArray), insertIndex, 1, selectedPerson).Split(',').ToList();

                FilllvNames();
                Formchanged = true;
                Tposition.Text = (NameArray.Count + 1).ToString();
            }
            else
            {
                MessageBox.Show("No selection made.");
            }
        }

        private void Listbox_Mouseup(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Handle left-click event
                if (lvNames.SelectedItems.Count > 0)
                {
                    var selectedItem = lvNames.SelectedItems[0];
                    txtRelation.Text = selectedItem.SubItems[1].Text;
                    txtRelation.Visible = true;
                    lblRelate.Visible = true;
                    btnUpdate.Visible = true;
                    btnCancel.Visible = true;
                    TxtFullName.Visible = true;
                    TxtFullName.Text = selectedItem.Text;
                    lblName1.Visible = true;
                    MemberID = SharedCode.GetMemberID(TxtFullName.Text, "");
                    txtRelation.TextChanged += TxtReslationTextChanged;
                    TxtFullName.TextChanged += TxtReslationTextChanged;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                DeletePerson();
            }
        }
        private void Event_Mouseup(object sender, MouseEventArgs e)
        {

            if (txtEvent.Text != "Add Event")
            {
                string EventID = SharedCode.GeteventID(txtEvent.Text, txtEventDetails.Text);

                // Handle left-click event
                int result = (int)Interaction.MsgBox("Are you sure you want to delete the event from this picture?", MsgBoxStyle.YesNo, "Delete Event");
                if (result == (int)Constants.vbYes)
                {
                    SharedCode.DeleteAPerson(EventID, SFileName);
                    lblEvent.Visible = false;
                    txtEvent.Text = "Add Event";
                    txtEventDetails.Visible = false;
                }
            }
            else
            {


                string input = Interaction.InputBox("Enter the EventID:");

                if (string.IsNullOrEmpty(input))
                {
                    // User clicked Cancel or entered nothing
                    return;
                }
                else
                {
                    int EventID;
                    if (int.TryParse(input, out EventID))
                    {
                        var connection = Manager.GetConnection();
                        try
                        {
                            using (connection)
                            using (var command = new SQLiteCommand("INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)", connection))
                            {
                                command.Parameters.AddWithValue("@Filename", SFileName);
                                command.Parameters.AddWithValue("@ID", EventID);
                                command.ExecuteNonQuery();
                            }
                        }
                        catch (SQLiteException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        var Eventinfo = SharedCode.GetEvent(EventID.ToString());
                        txtEvent.Text = Eventinfo.EventName;
                        txtEventDetails.Text = Eventinfo.Eventdetail;
                        lblEvent.Visible = true;
                        txtEvent.Visible = true;
                        txtEventDetails.Visible = true;
                    }
                    else
                    {
                        MessageBox.Show("Invalid input. Please enter a number.");
                    }
                }
            }
        }

        private void TxtReslationTextChanged(object sender, EventArgs e)
        {
            btnUpdate.Visible = true;
        }
        private void DeletePerson()
        {
            string selectedname;
            var response = default(int);
            string newplist;
            if (lvNames.SelectedItems.Count > 0)
            {
                var selectedItem = lvNames.SelectedItems[0];
                response = (int)Interaction.MsgBox("Are you sure you want to delete - " + selectedItem.Text, Constants.vbYesNo, "Confirmation");
            }
            if (response == (int)Constants.vbNo)
            {
                return;
            }
            else
            {
                if (lvNames.SelectedItems.Count > 0)
                {
                    selectedname = SharedCode.GetMemberID(lvNames.SelectedItems[0].Text, "");
                    newplist = SharedCode.DeleteAPerson(selectedname, SFileName );
                    NameArray = newplist.Split(",").ToList();
                    connection = Manager.GetConnection();
                    using (connection)
                        try
                        {
                            var Command = new SQLiteCommand("Delete from NamePhoto where npID =@name and npFilename =@filename", connection);
                            Command.Parameters.AddWithValue("@name", Conversions.ToInteger(selectedname));
                            Command.Parameters.AddWithValue("@filename", SFileName);
                            int result = Command.ExecuteNonQuery();
                            Debug.WriteLine(result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                }
            }
            Tposition.Text = (NameArray.Count + 1).ToString();

            Formchanged = true;
            FilllvNames();
        }

        private void BtnNew_click(object sender, EventArgs e)
        {
            var New1 = new NewName(SFileName, "DisplayInfo");
            New1.TopLevel = false;
            New1.FormClosed += SubformClosed;
            lhp.Controls.Add(New1);
            New1.BringToFront();
            New1.Show();
        }


        private void SubformClosed(object sender, FormClosedEventArgs e)
        {
            NameArray = Label1.Text.Split(',').ToList();
            FilllvNames();
            FillNamesAvail();
        }
        private void btnUpdate_click(object sender, EventArgs e)
        {
            connection = Manager.GetConnection();
            string qryUpdate = "update NameEvent set neName =@name1, neRelation = @relation where ID=@ID";
            using (connection)
                try
                {
                    var command = new SQLiteCommand(qryUpdate, connection);
                    command.Parameters.AddWithValue("@name1", TxtFullName.Text);
                    command.Parameters.AddWithValue("@relation", txtRelation.Text);
                    command.Parameters.AddWithValue("@ID", MemberID);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Update error" + ex.Message);
                }
            FilllvNames();
            txtRelation.Visible = false;
            lblRelate.Visible = false;
            btnUpdate.Visible = false;
            btnCancel.Visible = false;
            TxtFullName.Visible = false;
            lblName1.Visible = false;
        }
        private void btnRestart_click(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Play();
        }
        private void btnDelete_click(object sender, EventArgs e)
        {
            int Response = (int)Interaction.MsgBox("Are you sure you want to delete this picture/Video., Deleteing a picture or video cannot be undone.", Constants.vbYesNo, "Confirmation");

            if (Response == (int)Constants.vbNo)
            {
                return;
            }
            else
            {
                try
                {
                    connection = Manager.GetConnection();
                    using (connection)
                    {
                        // Begin a transaction
                        var transaction = connection.BeginTransaction();

                        try
                        {
                            using (var command = new SQLiteCommand("Delete from Pictures  WHERE PfileName = @filename", connection))
                            {
                                command.Transaction = transaction;
                                System.Threading.Thread.Sleep(100);
                                command.Parameters.AddWithValue("@filename", SFileName);
                                command.ExecuteNonQuery();

                                // Delete all event and people records
                                command.CommandText = "Delete from NamePhoto where  npfilename=@filename";
                                command.ExecuteNonQuery();
                                // Delete Pictures record
                                if (picBox.Image is not null)
                                {
                                    picBox.Image.Dispose();
                                }
                                picBox.Dispose();
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                string filePath = SharedCode.GetDefaultDir() + SFileName; // Delete the image
                                if (File.Exists(filePath))
                                {
                                    File.Delete(filePath);
                                }
                                // Commit the transaction if all updates succeed
                                transaction.Commit();
                            }

                            Console.WriteLine("Transaction committed successfully.");
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if an error occurs
                            transaction.Rollback();
                            MessageBox.Show("Delete Name Transaction rolled back. Error: " + ex.Message);
                        }
                        Close();


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void FilllvNames()
        {
            lvNames.Items.Clear();
            string qryfixname = "select neName,neRelation  from NameEvent where ID= @Name ";
            if (NameArray is null)
                return;
            connection = Manager.GetConnection();

            for (int I = 0, loopTo = NameArray.Count - 1; I <= loopTo; I++)
            {
                try
                {
                    if (!(NameArray[I] == "0"))
                    {
                        var command1 = new SQLiteCommand(qryfixname, connection);
                        command1.Parameters.AddWithValue("@Name", NameArray[I]);

                        var reader1 = command1.ExecuteReader();
                        reader1.Read();

                        var item1 = new ListViewItem(reader1["neName"].ToString());
                        if (!reader1.IsDBNull(reader1.GetOrdinal("neRelation")))
                        {
                            item1.SubItems.Add(Convert.ToString(reader1["neRelation"]));
                            lvNames.Items.Add(item1);
                            reader1.Close();
                        }
                        else
                        {
                            item1.SubItems.Add(" ");
                            lvNames.Items.Add(item1);
                            reader1.Close();
                        }
                    }
                }
                catch
                {
                    // MessageBox.Show(ex.Message)
                }
                Tposition.Text = NameArray.Count.ToString();
            }


            connection.Close();
        }

      
       
        private void Datechanged(object sender, EventArgs e)
        {
           Formchanged = true;
        }
        private void DescriptionChanged(object sender, EventArgs e)
        {
            Formchanged = true;
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

            if (Formchanged)
            {
                Savephoto(this, EventArgs.Empty);
                SaveMetadata();
            }
        }


        private void SaveMetadata()
        {
            try
            {
                string jsonMetadata = jsonlist.BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames);
                jsonlist.WriteJsonMetadataToMediaFile(DDir + SFileName, jsonMetadata);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Metadata write failed: " + ex.Message);
            }
        }

        private void Playvideo(string videoPath)
        {
            if (File.Exists(videoPath))
            {
                var media = new Media(_libVLC, videoPath, FromType.FromPath);
                _mediaPlayer.Play(media);
            }
           // btnRestart.Location = new Point(_videoView.Left + _videoView.Width / 2 - btnRestart.Width / 2, _videoView.Bottom + 50);
        }


        private void btnCopyFile_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            string DDir = SharedCode.GetDefaultDir();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string destinationPath = Path.Combine(folderDialog.SelectedPath, Path.GetFileName(SFileName));
                try
                {
                    File.Copy(DDir + SFileName, destinationPath, true);
                    MessageBox.Show("File copied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnEmbed_Click(object sender, EventArgs e)
        {
            string jsonMetadata;
            if (TypeI == 1)
            {
                if (picBox.Image is not null)
                {
                    picBox.Image.Dispose();
                    picBox.Image = null;
                }
                jsonMetadata = jsonlist.BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames);
                jsonlist.WriteJsonMetadataToMediaFile(DDir + SFileName, jsonMetadata);
                byte[] imgBytes = File.ReadAllBytes(DDir + SFileName);
                using (var ms = new MemoryStream(imgBytes))
                {
                    var img = Image.FromStream(ms);
                    picBox.Image = new Bitmap(img);
                }
            }
            else
            {


  //              pl.PlayStateChange += (senderObj, eArgs) => { if (eArgs.newState == (int)WMPLib.WMPPlayState.wmppsMediaEnded) { pl.Ctlcontrols.stop(); pl.URL = string.Empty; pl.close(); try { jsonMetadata = jsonlist.BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames); jsonlist.WriteJsonMetadataToMediaFile(DDir + SFileName, jsonMetadata); } catch (Exception ex) { Debug.WriteLine("Metadata write failed: " + ex.Message); } pl.PlayStateChange -= null; } }; // Optional cleanup
            }
        }
        private void btnCancel_click(object sender, EventArgs e)
        {
            txtRelation.Visible = false;
            lblRelate.Visible = false;
            btnUpdate.Visible = false;
            btnCancel.Visible = false;
            TxtFullName.Visible = false;
            lblName1.Visible = false;
        }
        private void btnUpdateThumb_click(object sender, EventArgs e)
        {
            SharedCode.Updatethumb(DDir + SFileName);

        }
    
           private void Savephoto(object sender, EventArgs e)
        {
            string plist = createPeoplelist();
            var connection = Manager.GetConnection();


            using (connection)
            {


                var transaction = connection.BeginTransaction();

                try
                {
                    var command = new SQLiteCommand() { Connection = connection, Transaction = transaction };

                    // Create Picture Record
                    UpdatePictureRecord(command, plist);
                    // 🔎 Save thumbnail from DB for visual verification

                    UpdateNamePhotoRecords(command, plist);

                    // Handle Event Type Record

                    UpdateEventRecord(command);


                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Save Date Transaction rolled back. Error: " + ex.Message);
                }
            }

            string jsonMetadata = jsonlist.BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames);
            jsonlist.WriteJsonMetadataToMediaFile(DDir + SFileName, jsonMetadata);

        }

        // Encapsulated SQL Methods
        private void UpdatePictureRecord(SQLiteCommand command, string plist)
        {
            command.CommandText = @"
        UPDATE Pictures 
        SET 
            [PDescription] = @PDescription, 
            [PPeopleList] = @PPeopleList, 
            [PMonth] = @PMonth, 
            [PYear] = @PYear, 
            [PLastModifiedDate] = @PLastModifiedDate, 
            [PNameCount] = @PNameCount 
        WHERE 
            [PFilename] = @Pfilename";

            command.Parameters.AddWithValue("@Pfilename", SFileName);
            command.Parameters.AddWithValue("@PDescription", txtDescription.Text);
            command.Parameters.AddWithValue("@PPeopleList", plist);
            command.Parameters.AddWithValue("@PMonth", TxtMonth.Text);
            command.Parameters.AddWithValue("@PYear", txtYear.Text);
            command.Parameters.AddWithValue("@PLastModifiedDate", DateTime.Today);
            command.Parameters.AddWithValue("@PNameCount", lvNames.Items.Count);

            command.ExecuteNonQuery();
        }


        private void UpdateNamePhotoRecords(SQLiteCommand command, string pList)
        {
            var pitems = pList.Split(',').ToList();

            foreach (string pitem in pitems)
            {
                int id = Conversions.ToInteger(pitem);

                // Check if the record already exists
                command.CommandText = @"
            SELECT COUNT(*) 
            FROM NamePhoto 
            WHERE npID = @ID AND npFileName = @Filename";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@Filename", SFileName);

                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count == 0)
                {
                    // Insert only if it doesn't exist
                    command.CommandText = @"
                INSERT INTO NamePhoto (npID, npFileName) 
                VALUES (@ID, @Filename)";

                    command.ExecuteNonQuery();
                }
            }
        }


        private void UpdateEventRecord(SQLiteCommand command)
        {
            // First, check if the record already exists
            command.CommandText = @"
        SELECT COUNT(*) 
        FROM NamePhoto 
        WHERE npID = @ID AND npFileName = @Filename";

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ID", txtEvent.Text);
            command.Parameters.AddWithValue("@Filename", SFileName);

            int count = Convert.ToInt32(command.ExecuteScalar());

            if (count == 0)
            {
                // Insert only if it doesn't exist
                command.CommandText = @"
            INSERT INTO NamePhoto (npID, npFileName) 
            VALUES (@ID, @Filename)";

                command.ExecuteNonQuery();
            }
        }

        private string createPeoplelist()
        {
            string plist = "";
            string id;

            if (lvNames.Items.Count == 0)
            {
                plist = "1";
            }
            else
            {
                for (int i = 0; i < lvNames.Items.Count; i++)
                {
                    id = SharedCode.GetMemberID(lvNames.Items[i].Text, lvNames.Items[i].SubItems[1].Text);

                    if (string.IsNullOrEmpty(plist))
                    {
                        plist = id;
                    }
                    else
                    {
                        plist += "," + id;
                    }
                }
            }

            return plist;
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Play();
        }





    }
}