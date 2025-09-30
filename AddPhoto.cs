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
using System.Xml.Linq;

namespace FamAlbum
{

    public partial class AddPhoto
    {
        private ListView lvNames = new ListView();
        public Label Label1 { get; set; } = new Label();
        private NewName New1;
        public string SFileName { get; set; }
        private PictureBox picBox = new PictureBox();
        private MenuStrip menuStrip = new MenuStrip();
        private TextBox Tposition = new TextBox();
        private List<string> NameArray = new List<string>();
        private Button btnAdd;
        private Button btnNewPerson;
        private Button btnNext;
        private Button btnPrior;
        private Button btnDelete;
        private Button btnSave;
        private Button btnCopy;

        private Button btnSavePhoto;
        private Button btnFixOrient;
        private MediaPlayer _mediaPlayer;
        private VideoView _videoView;
        private LibVLC _libVLC;

        private Button btnRestart;
        private ComboBox cbNamesOnFile = new ComboBox();
        private DataTable dt = new DataTable();
        private TextBox txtRelation;
        private Label lblPosition = new Label();
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();
        private string mfilename;
        private string Dfilename;
        private string FileDirectory;
        private Label lbldimen = new Label();
        private int Mheight;
        private int Mwidth;
        private int Playtime;
        private byte[] thumb;
        private int TypeI;
        private TextBox txtFilename = new TextBox();
        public string Etype { get; set; }
        public int EventID { get; set; }
        private string I;
        public AddPhoto()
        {
            btnAdd = new Button();
            btnNewPerson = new Button();
            btnNext = new Button();
            btnPrior = new Button();
            btnDelete = new Button();
            btnSave = new Button();
            btnCopy = new Button();
            btnSavePhoto = new Button();
            btnFixOrient = new Button()
            {
                Text = "Fixo",
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                Font = new Font("Arial", 12f, FontStyle.Bold),
                Size = new Size(220, 40),
                Location = new Point(410, 1540),
                Visible = true,
                AutoSize = true
            };
            btnRestart = new Button();
            txtRelation = new TextBox()
            {
                Location = new Point(20, 1120),
                Size = new Size(800, 400),
                Font = new Font("Arial", 12f, FontStyle.Regular),
                Multiline = true,
                BorderStyle = BorderStyle.None,
                Visible = false,
                AutoSize = true
            };
            TxtFullName = new TextBox()
            {
                Location = new Point(20, 1050),
                Size = new Size(800, 30),
                Font = new Font("Arial", 12f, FontStyle.Bold),
                Multiline = false,
                Visible = false,
                AutoSize = true
            };
            InitializeComponent();
            _videoView = new VideoView
            {
                MediaPlayer = _mediaPlayer,
                Name = "videoView",
                Size = new Size(lpw - 50, lph / 2), // Adjust as needed
                Location = new Point(40, 50) // Adjust as needed
            };

            rhp.Controls.Add(_videoView);

        }

        static Label initLblDimension()
        {
            var init = new Label();
            return (init.Font = new Font(init.Font.FontFamily, 12f), init.Location = new Point(50, 150), init.AutoSize = true, init).init;
        }

        private Label lblDimension = initLblDimension();


        static Label initLblRelate()
        {
            var init = new Label();
            return (init.Font = new Font(init.Font.FontFamily, 12f), init.Text = "Relation to Morton Family ", init.Location = new Point(20, 1090), init.Visible = false, init.AutoSize = true, init).init;
        }

        private Label lblRelate = initLblRelate();
        private TextBox TxtFullName;
        private Label lblFileName = new Label();
        private TextBox TxtMonth = new TextBox();
        private TextBox txtYear = new TextBox();
        private Label lblDescription = new Label();
        private TextBox txtDescription = new TextBox();
        private TextBox txtEvent = new TextBox();
        private TextBox txtEventDetails = new TextBox();
        private Label lblEvent = new Label();
        private string DDir;
        private Panel lhp = new Panel();
        private Panel rhp = new Panel();
        private int lpw;
        private int lph;
        private int offset = 0; // Start at the first record
        private int TotalCount = 0;

        public void AddPhoto_Load(object sender, EventArgs e)
        {

            WindowState = FormWindowState.Maximized;
            AutoScaleMode = AutoScaleMode.Font;
            IsMdiContainer = true;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            var New1 = new NewName(SFileName, "AddPhoto");
            New1.FormClosed += SubformClosed;

            menuStrip = fmmenus.fmenus();
            var menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += MenuItemExit_Click;
            // Add the Exit item to the MenuStrip
            menuStrip.Items.Add(menuItemExit);
            MainMenuStrip = menuStrip;
            menuStrip.Items.RemoveAt(0);
            menuStrip.Items.Insert(0, menuItemExit);
            Controls.Add(menuStrip);


            Core.Initialize(); // Required once per app

            _libVLC = new LibVLC(); // This must be done before creating Media
                                    //var media = new Media(_libVLC, videoPath, FromType.FromPath);
            _mediaPlayer = new MediaPlayer(_libVLC);

            connection = Manager.GetConnection();
            using (var connection = Manager.GetConnection())
            {
                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Unindexedfiles", connection))
                {
                    TotalCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

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

            lpw = lhp.Width;
            lph = lhp.Height;
            Controls.Add(rhp);

            DDir = SharedCode.GetDefaultDir();

            // Initialize the lvNames control
            {
                ref var withBlock9 = ref lvNames;
                withBlock9.Size = new Size((int)Math.Round(lpw * 0.55d), 450);
                withBlock9.Location = new Point(30, 235);
                withBlock9.View = View.Details;
                withBlock9.FullRowSelect = true;
                withBlock9.Font = new Font("Arial", 12f, FontStyle.Regular);
            }


            int Rpont = lvNames.Left + lvNames.Width + 20;

            var title = new Label()
            {
                Text = "Family Album",
                Font = new Font("segoe", 40f),
                Size = new Size(screenWidth, 60),
                Location = new Point(0, 20),
                TextAlign = (System.Drawing.ContentAlignment)System.Windows.Forms.VisualStyles.ContentAlignment.Center,
                AutoSize = false
            };

            _videoView = new VideoView
            {
                MediaPlayer = _mediaPlayer,
                Name = "videoView",
                Size = new Size(lpw - 50, lph / 2), // Adjust as needed
                Location = new Point(40, 50) // Adjust as needed
            };
            rhp.Controls.Add(_videoView);

            {
                var withBlock2 = btnAdd;
                withBlock2.Text = "Add Names";
                withBlock2.BackColor = Color.LightBlue;
                withBlock2.ForeColor = Color.DarkBlue;
                withBlock2.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock2.Size = new Size(200, 40);
                withBlock2.Location = new Point(Rpont, 300);
            }
            lhp.Controls.Add(btnAdd);

            {
                var withBlock3 = btnNewPerson;
                withBlock3.Text = "Add Name not in List";
                withBlock3.BackColor = Color.LightBlue;
                withBlock3.ForeColor = Color.DarkBlue;
                withBlock3.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock3.Size = new Size(200, 40);
                withBlock3.Location = new Point(Rpont, 340);
            }
            lhp.Controls.Add(btnNewPerson);

            {
                var withBlock4 = btnFixOrient;
                withBlock4.Text = "Fix Orientation";
                withBlock4.BackColor = Color.LightBlue;
                withBlock4.ForeColor = Color.DarkBlue;
                withBlock4.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock4.Size = new Size(200, 40);
                withBlock4.Location = new Point(Rpont, 380);

                withBlock4.Visible = true;
            }


            {
                var withBlock5 = btnNext;
                withBlock5.Text = "Next Image";
                withBlock5.Location = new Point(Rpont, 420);
                withBlock5.BackColor = Color.LightBlue;
                withBlock5.ForeColor = Color.DarkBlue;
                withBlock5.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock5.Size = new Size(200, 40);
            }

            {
                var withBlock6 = btnPrior;
                withBlock6.Text = "Prior Image";
                withBlock6.Location = new Point(Rpont, 460);
                withBlock6.BackColor = Color.LightBlue;
                withBlock6.ForeColor = Color.DarkBlue;
                withBlock6.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock6.Size = new Size(200, 40);
                withBlock6.Enabled = false;
            }

            {
                var withBlock7 = btnSavePhoto;
                withBlock7.Text = "Save";
                withBlock7.BackColor = Color.LightBlue;
                withBlock7.ForeColor = Color.DarkBlue;
                withBlock7.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock7.Size = new Size(200, 40);
                withBlock7.Location = new Point(Rpont, 500);
                withBlock7.Visible = true;
            }
            {
                var withBlock8 = btnDelete;
                withBlock8.Text = "Delete";
                withBlock8.BackColor = Color.LightBlue;
                withBlock8.ForeColor = Color.DarkBlue;
                withBlock8.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock8.Size = new Size(200, 40);
                withBlock8.Location = new Point(Rpont, 620);
                withBlock8.Visible = true;
            }
            mfilename = "";


            Label @init = new Label();
            var lblPeoplelist = (@init.Font = new Font(@init.Font.FontFamily, 12f), @init.Text = "People In Picture", @init.Location = new Point(50, 200), @init.Width = 100, @init.Height = 30, @init.AutoSize = true, @init).@init;


            lvNames.Columns.Add("FullName", (int)Math.Round(lvNames.Width / 2d - 3d), HorizontalAlignment.Left);
            lvNames.Columns.Add("Relation", (int)Math.Round(lvNames.Width / 2d - 3d), HorizontalAlignment.Left);

            cbNamesOnFile.Location = new Point(lvNames.Left, 690);
            cbNamesOnFile.Size = new Size(lvNames.Width, 690);
            cbNamesOnFile.Font = new Font(cbNamesOnFile.Font.FontFamily, 12f);
            dt.Columns.Add("Names", typeof(string));
            dt.Columns.Add("Relation", typeof(string));

            {
                ref var withBlock11 = ref lblPosition;
                withBlock11.Location = new Point(Rpont, 690);
                withBlock11.Size = new Size(187, 40);
                withBlock11.Text = "Position of new Person";
                withBlock11.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock11.Visible = false;
            }

            {
                ref var withBlock10 = ref Tposition;
                withBlock10.Location = new Point(Rpont+lblPosition.Width, 685);
                withBlock10.Size = new Size(40, 40);
                withBlock10.Visible = false;
                withBlock10.TextAlign = HorizontalAlignment.Left;
                withBlock10.Text = "1";
                withBlock10.Font = new Font("Segoe UI", 14f, FontStyle.Regular);
            }

            Label @init1 = new Label();
            var lblMonth = (@init1.Font = new Font(@init1.Font.FontFamily, 12f), @init1.Text = "Month of Picture: ", @init1.Location = new Point(50, 40), @init1.Width = 100, @init1.Height = 30, @init1.AutoSize = true, @init1).@init1;
            {
                ref var withBlock12 = ref TxtMonth;
                withBlock12.Font = new Font(withBlock12.Font.FontFamily, 12f);
                // .Text = IIf(reader.IsDBNull(reader.GetOrdinal("PMonth")), "0", reader("PMonth").ToString())
                withBlock12.Location = new Point(260, 40);
                withBlock12.Width = 100;
                withBlock12.Height = 30;
                withBlock12.AutoSize = true;
            }

            Label @init2 = new Label();
            var lblYear = (@init2.Font = new Font(@init2.Font.FontFamily, 12f), @init2.Text = "Year of Picture:", @init2.Location = new Point(50, 80), @init2.Width = 200, @init2).@init2;

            {
                ref var withBlock13 = ref txtYear;
                withBlock13.Font = new Font(withBlock13.Font.FontFamily, 12f);
                // .Text = IIf(reader.IsDBNull(reader.GetOrdinal("Pyear")), "0", reader("Pyear").ToString())
                withBlock13.Location = new Point(260, 80);
                withBlock13.Size = new Size(100, 30);
            }

            Label @init3 = new Label();
            var lblFilename = (@init3.Font = new Font(@init3.Font.FontFamily, 12f), @init3.Text = "File name & Location", @init3.Location = new Point(50, 120), @init3.Width = 200, @init3).@init3;



            {
                ref var withBlock14 = ref txtFilename;
                withBlock14.Font = new Font(withBlock14.Font.FontFamily, 12f);
                // .Text = Sfilename
                withBlock14.Location = new Point(260, 120);
                withBlock14.Size = new Size(600, 40);
            }


            var LABEL1 = new Label();


            lvNames.MouseUp += DeletePerson;

            Controls.Add(title);
            lhp.Controls.Add(lblPeoplelist);
            lhp.Controls.Add(lvNames);
            lhp.Controls.Add(lblMonth);
            lhp.Controls.Add(lblYear);
            lhp.Controls.Add(lblDimension);
            lhp.Controls.Add(txtRelation);
            lhp.Controls.Add(lblRelate);
            lhp.Controls.Add(txtFilename);
            lhp.Controls.Add(lblFilename);

            lhp.Controls.Add(lblPosition);
            lhp.Controls.Add(Tposition);
            rhp.Controls.Add(txtEvent);
            rhp.Controls.Add(lblEvent);
            rhp.Controls.Add(txtEventDetails);
            lhp.Controls.Add(TxtMonth);
            lhp.Controls.Add(txtYear);
            lhp.Controls.Add(btnNext);
            lhp.Controls.Add(btnFixOrient);
            lhp.Controls.Add(btnSavePhoto);
            lhp.Controls.Add(btnDelete);
            lhp.Controls.Add(btnPrior);
            rhp.Controls.Add(lblDescription);
            rhp.Controls.Add(txtDescription);
            if (TypeI == 1)
            {
                rhp.Controls.Add(picBox);
            }
            else
            {
                rhp.Controls.Add(btnRestart);
            }
            if (!string.IsNullOrEmpty(mfilename))
            {
                // PlayVideo(mfilename)
            }
            btnRestart.Click += btnRestart_click;
            btnFixOrient.Click += btnFixOrient_click;
            btnNext.Click += btnNext_click;
            btnSavePhoto.Click += btnSavePhoto_click;
            btnDelete.Click += btnDelete_click;
            btnPrior.Click += btnPrior_click;
            btnAdd.Click += btnAdd_click;
            string EventName = "";
            connection = Manager.GetConnection();
            using (connection)
            {
                var command1 = new SQLiteCommand("select neName,neRelation from NameEvent where ID = @eventid", connection);
                command1.Parameters.AddWithValue("@eventid", EventID);

                using (var reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        EventName = reader1["neName"].ToString();
                        txtEvent.Visible = true;
                        if (Strings.Trim(reader1["neRelation"].ToString()) == "NULL")
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
                xleft = picBox.Location.X;
                xWidth = picBox.Width;
                // Set the vertical position just below picbox
                yPosition = picBox.Location.Y + picBox.Height + 10; // Adjust 10 for spacing
            }
            else
            {    // Calculate horizontal center of picbox
                xCenter = _videoView.Location.X + _videoView.Width / 2 - 800 / 2;
                xleft = _videoView.Location.X;
                xWidth = _videoView.Width;
                // Set the vertical position just below picbox
                yPosition = _videoView.Location.Y + _videoView.Height + 10; // Adjust 10 for spacing
            }
            {
                ref var withBlock15 = ref lblDescription;
                withBlock15.Height = 40;
                withBlock15.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock15.Location = new Point(xleft, yPosition + 20);
                withBlock15.AutoSize = true;
                withBlock15.Visible = true;
                withBlock15.Size = new Size(40, 80);
                withBlock15.Text = "Description:";
            }
            {
                ref var withBlock16 = ref txtDescription;
                withBlock16.Location = new Point();
                withBlock16.Visible = true;
                withBlock16.Width = xWidth -lblDescription.Width -20;
                withBlock16.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock16.Height = 60;
                withBlock16.Location = new Point(xleft + lblDescription.Width, yPosition);

                withBlock16.Multiline = true;
                withBlock16.AutoSize = true;
                withBlock16.BorderStyle = BorderStyle.FixedSingle;
                withBlock16.Text = "";
            }


            {
                ref var withBlock17 = ref lblEvent;
                withBlock17.Height = 40;
                withBlock17.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock17.Location = new Point(xleft , yPosition + 100);
                withBlock17.AutoSize = true;
                withBlock17.Visible = false;
                withBlock17.Size = new Size(40, 80);
                withBlock17.Text = "Event:";
            }
            {
                ref var withBlock18 = ref txtEvent;
                withBlock18.Location = new Point();
                withBlock18.Visible = false;
                withBlock18.Width = 750;
                withBlock18.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock18.Height = 40;
                withBlock18.Location = new Point(xleft + lblDescription.Width, yPosition + 100);

                withBlock18.ReadOnly = true;
                withBlock18.AutoSize = true;
                withBlock18.BorderStyle = BorderStyle.None;
                withBlock18.Text = EventName;
            }
            {
                ref var withBlock19 = ref txtEventDetails;
                withBlock19.Location = new Point(xleft + lblDescription.Width, yPosition + 150);
                withBlock19.Width = 750;
                withBlock19.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock19.Height = 100;
                // .AutoSize = True
                withBlock19.BorderStyle = BorderStyle.None;
                withBlock19.ReadOnly = true;
                withBlock19.Multiline = true;
                withBlock19.Visible = false;
            }


            ProcessNextImage();


        }

        private void ProcessNextImage()
        {
            DDir = SharedCode.GetDefaultDir();

            string qryPhotos = $"SELECT * FROM Unindexedfiles WHERE uiStatus = 'N' ORDER BY uiFilename LIMIT 1 OFFSET {offset}";
            connection = Manager.GetConnection();
            using (var command = new SQLiteCommand(qryPhotos, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        TypeI =  Convert.ToInt32(reader["uiType"]);
                        Dfilename = reader["uiFilename"].ToString();
                        byte[] imageData = reader["uiThumb"] as byte[];
                        if (imageData is not null && imageData.Length > 0)
                        {
                            using (var ms = new MemoryStream(imageData))
                            {
                                // yourImage.Save(ms, Imaging.ImageFormat.Png) ' Or use .Jpeg, .Bmp, etc.
                                thumb = ms.ToArray();
                            }
                        }

                        FileDirectory = reader["uiDirectory"].ToString();
                        SFileName = Dfilename.Remove(0, DDir.Length);
                        txtFilename.Text = SFileName;
                        Playtime = Conversions.ToInteger(reader["uiVtime"]);
                        Mwidth = Conversions.ToInteger(reader["uiWidth"]);
                        Mheight = Conversions.ToInteger(reader["uiHeight"]);
                    }
                    else
                    {
                        MessageBox.Show("All Images Processed");
                    }
                    reader.Close();
                    reader.Dispose();
                    txtDescription.Text = "";
                }
                if (TypeI == 1)
                {
                    _videoView.Visible = false;
                    btnRestart.Visible = false;
                    picBox.Visible = true;
                    {
                        ref var withBlock = ref picBox;
                        withBlock.SizeMode = PictureBoxSizeMode.Zoom;
                        withBlock.Width = lpw;
                        withBlock.Location = new Point(lhp.Width - picBox.Width, 50);
                        withBlock.Height = (int)Math.Round(0.95d * lph / 2d);
                        withBlock.Visible = true;
                    }
                    rhp.Controls.Add(picBox);
                    Image img;
                    try
                    {
                        using (var fs = new FileStream(Dfilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            img = Image.FromStream(fs);
                        }
                        picBox.Image = img;
                        lblDimension.Text = $"Width: {Mwidth} pixels" + Constants.vbCrLf + $"Height: {Mheight} pixels";
                    }
                    catch
                    {
                        DeleteUnindexedFile(Dfilename);
                        return;
                    }

                    var dateInfo = SharedCode.GetPhotoMonthAndYear(Dfilename);
                    if (dateInfo.Item1 > 0 && dateInfo.Item2 > 0)
                    {
                        TxtMonth.Text = dateInfo.Item1.ToString();
                        txtYear.Text = dateInfo.Item2.ToString();
                    }
                    else
                    {
                        TxtMonth.Text = "0";
                        txtYear.Text = "0";
                    }
                    LoadMetadataToUI(Dfilename);
                    if (!string.IsNullOrEmpty(txtEvent.Text))
                    {
                        txtEvent.Visible = true;
                        txtEventDetails.Visible = true;
                        lblEvent.Visible = true;
                    }
                }
                else if (TypeI == 2)
                {
                    picBox.Visible = false;

                    {
                        picBox.Visible = false;
                        _videoView.Visible = true;
                        mfilename = DDir + SFileName;
                    }

                    {
                        var withBlock2 = btnRestart;
                        withBlock2.Text = "Restart";
                        withBlock2.BackColor = Color.LightBlue;
                        withBlock2.ForeColor = Color.DarkBlue;
                        withBlock2.Font = new Font("Arial", 12f, FontStyle.Bold);
                        withBlock2.Size = new Size(220, 40);
                        withBlock2.Location = new Point(lpw / 2 - 110, lph / 2);
                        withBlock2.Visible = false;
                        withBlock2.AutoSize = true;
                    }


                    lbldimen.Text = $"Width: {Mwidth} pixels" + Constants.vbCrLf + Constants.vbCrLf + $"Height: {Mheight} pixels";
                    btnRestart.Visible = true;
                    rhp.Controls.Add(btnRestart);
                    _videoView.Visible = true;

                    Playvideo(DDir + SFileName);
                }
                else
                {


                }

            }
        }
   

        private void FillListview()
        {
            lvNames.Items.Clear();
            string qryfixname = "select ID,neName,neRelation  from NameEvent where ID= @Name ";
            if (NameArray is null)
                return;
            connection = Manager.GetConnection();

            for (int I = 0, loopTo = NameArray.Count - 1; I <= loopTo; I++)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(NameArray[I]))
                    {
                        var command1 = new SQLiteCommand(qryfixname, connection);
                        command1.Parameters.AddWithValue("@Name", NameArray[I]);

                        var reader1 = command1.ExecuteReader();
                        reader1.Read();
                        var item1 = new ListViewItem(reader1["neName"].ToString());
                        item1.SubItems.Add(reader1["neRelation"].ToString());
                        lvNames.Items.Add(item1);
                        reader1.Close();
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
        private void btnAdd_click(object sender, EventArgs e)
        {
            Tposition.Visible = true;

            cbNamesOnFile.Visible = true;
            // cbNamesOnFile.DroppedDown = True
            lblPosition.Visible = true;
            FillNamesAvail();
            cbNamesOnFile.Focus();
        }
        private void FillNamesAvail()
        {

            dt = SharedCode.FillNames();
            cbNamesOnFile.SelectionChangeCommitted -= NameCombo_SelectionChangeCommitted;

            cbNamesOnFile.DataSource = dt;
            cbNamesOnFile.DisplayMember = "Names";
            cbNamesOnFile.ValueMember = "ID";
            cbNamesOnFile.Visible = true;

            lhp.Controls.Add(cbNamesOnFile);
            lhp.Controls.Add(lblPosition);
            lhp.Controls.Add(Tposition);
            if (!Information.IsNumeric(Tposition.Text))
                Tposition.Text = "1";
            cbNamesOnFile.SelectedIndex = -1;
            cbNamesOnFile.SelectionChangeCommitted += NameCombo_SelectionChangeCommitted;

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
                FillListview();


                // cbNamesOnFile.Visible = False
                cbNamesOnFile.SelectedIndex = -1;
                // connection.Close()
                Tposition.Text = (NameArray.Count + 1).ToString();
            }
            else
            {
                MessageBox.Show("No selection made.");
            }
        }

        private void btnNewPerson_Click(object sender, EventArgs e)
        {
            New1 = new NewName(SFileName, "AddPhoto");
            New1.FormClosed += SubformClosed;
            New1.TopLevel = false;
            lhp.Controls.Add(New1);
            New1.BringToFront();
            New1.Show();
        }

        private void btnNext_click(object sender, EventArgs e)
        {
            offset += 1;
            if (offset > TotalCount - 1)
            {
                btnNext.Enabled = false;
                offset -= 1;
            }
            if (offset > 0)
                btnPrior.Enabled = true;
            lvNames.Items.Clear();

            ProcessNextImage();
        }

        private void btnPrior_click(object sender, EventArgs e)
        {
            offset -= 1;
            if (offset < 0)
            {
                btnPrior.Enabled = false;
                offset = 0;
            }
            if (offset < TotalCount - 1)
            {
                btnNext.Enabled = true;
            }
            lvNames.Items.Clear();

            ProcessNextImage();
        }
        private void btnSavePhoto_click(object sender, EventArgs e)
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
                    InsertPictureRecord(command, plist);
                    // 🔎 Save thumbnail from DB for visual verification

                    InsertNamePhotoRecords(command, plist);

                    // Handle Event Type Record
                    if (Etype != "No")
                        InsertEventRecord(command);

                    // Remove from unindexedFiles
                    command.CommandText = "DELETE FROM unindexedFiles WHERE uiFilename=@Filename";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Filename",DDir + SFileName);
                    int result=command.ExecuteNonQuery();

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

            // Cleanup UI
            ResetUI();
        }

        // Encapsulated SQL Methods
        private void InsertPictureRecord(SQLiteCommand command, string plist)
        {
            command.CommandText = @"INSERT INTO Pictures ([Pfilename], [PfileDirectory], [PDescription], [PHeight], [PWidth], 
        [PPeopleList], [PMonth], [PYear], [PSoundFile], [PDateEntered], [PType], [PLastModifiedDate], 
        [PReviewed], [PTime], [PThumbnail], [PNameCount]) 
        VALUES (@Pfilename, @PfileDirectory,@PDescription, @PHeight, @PWidth, @PPeopleList, @PMonth, @PYear, 
        @PSoundFile, @PDateEntered, @PType, @PLastModifiedDate, @PReviewed, @PTime, @PThumbnail, @PNameCount)";

            {
                var withBlock = command.Parameters;
                withBlock.AddWithValue("@Pfilename", SFileName);
                withBlock.AddWithValue("@PfileDirectory", FileDirectory);
                withBlock.AddWithValue("@PDescription", txtDescription.Text);
                withBlock.AddWithValue("@PHeight", Mheight);
                withBlock.AddWithValue("@PWidth", Mwidth);
                withBlock.AddWithValue("@PPeopleList", plist);
                withBlock.AddWithValue("@PMonth", TxtMonth.Text);
                withBlock.AddWithValue("@PYear", txtYear.Text);
                withBlock.AddWithValue("@PSoundFile", "");
                withBlock.AddWithValue("@PDateEntered", DateTime.Today);
                withBlock.AddWithValue("@PType", TypeI);
                withBlock.AddWithValue("@PLastModifiedDate", DateTime.Today);
                withBlock.AddWithValue("@PReviewed", 0);
                withBlock.AddWithValue("@PTime", Playtime);
                withBlock.Add(new SQLiteParameter("@PThumbnail", DbType.Binary) { Value = thumb });
                withBlock.AddWithValue("@PNameCount", lvNames.Items.Count);
            }
            command.ExecuteNonQuery();
        }

        private void InsertNamePhotoRecords(SQLiteCommand command, string pList)
        {
            var pitems = pList.Split(',').ToList();
            command.CommandText = "INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)";
            command.Parameters.AddWithValue("@Filename", SFileName);
            command.Parameters.Add("@ID", DbType.Int32);

            foreach (string pitem in pitems)
            {
                command.Parameters["@ID"].Value = Conversions.ToInteger(pitem);
                command.ExecuteNonQuery();
            }
        }

        private void InsertEventRecord(SQLiteCommand command)
        {
            command.CommandText = "INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)";
            command.Parameters["@ID"].Value = EventID;
            command.ExecuteNonQuery();
        }

        private void DeleteUnindexedFile(string Filename)
        {
            string qry = "DELETE FROM unindexedFiles WHERE uiFilename=@Filename";
            using (var conn = Manager.GetConnection())
            {
                var command = new SQLiteCommand(qry, conn);
                command.Parameters.AddWithValue("@Filename", Filename);
                command.ExecuteNonQuery();
            }
        }

        private void ResetUI()
        {
            NameArray.Clear();
            lvNames.Items.Clear();
            Tposition.Text = "1";
            ProcessNextImage();
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
                for (int i = 0, loopTo = lvNames.Items.Count - 1; i <= loopTo; i++)
                {
                    id = SharedCode.GetMemberID(lvNames.Items[i].Text, lvNames.Items[i].SubItems[1].Text);

                    if (string.IsNullOrEmpty(plist))
                    {
                        plist = id;
                    }
                    else
                    {
                        plist = plist + "," + id;
                    }
                }
            }

            return plist;
        }
        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            connection = Manager.GetConnection();
            int re;
            using (connection)
            {
                var d = DateTime.Today;
                try
                {
                    var command = new SQLiteCommand();
                    command.Connection = connection;
                    command.CommandText = "Update Unindexedfiles set uiStatus ='N' ";
                    re = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Status not saved.  Err: " + ex.Message);
                }
            }
            Close();
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
                            var command = new SQLiteCommand();
                            command.Connection = connection;
                            command.Transaction = transaction;


                            System.Threading.Thread.Sleep(100);
                            //command.CommandText = "Delete from Pictures  WHERE PfileName = @filename";
                            //command.Parameters.AddWithValue("@filename", SFileName);
                            //command.ExecuteNonQuery();

                            //// Delete all event and people records
                            //command.CommandText = "Delete from NamePhoto where  npfilename = @filename";
                            //command.ExecuteNonQuery();
                            // Delete Pictures record
                            if (picBox.Image is not null)
                            {
                                picBox.Image.Dispose();
                            }
                            picBox.Image = null;
                            // GC.Collect()
                            // GC.WaitForPendingFinalizers()
                            string filePath = SharedCode.GetDefaultDir() + SFileName; // Delete the image
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }

                            command.CommandText = "Delete from Unindexedfiles where uiFilename = @filename";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@filename",filePath);
                            int result=command.ExecuteNonQuery();

                            // Commit the transaction if all updates succeed
                            transaction.Commit();
                            Console.WriteLine("Transaction committed successfully.");
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if an error occurs
                            transaction.Rollback();
                            MessageBox.Show("Delete Name Transaction rolled back. Error: " + ex.Message);
                        }


                    }
                    TotalCount -= 1;
                    offset += 1;

                    ProcessNextImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void LoadMetadataToUI(string filePath)
        {
            var metadata = jsonlist.ReadJsonFromMediaFile(filePath);

            if (metadata is not null)
            {
                txtEvent.Text = metadata.EventName;
                if (string.IsNullOrEmpty(metadata.EventName))
                {
                    Etype = "No";
                }
                else
                {
                    EventID = Conversions.ToInteger(SharedCode.GeteventID(metadata.EventName, metadata.EventDetails));
                    Etype = "Yes";
                }
                txtEventDetails.Text = metadata.EventDetails;
                TxtMonth.Text = metadata.imMonth.ToString();
                txtYear.Text = metadata.imYear.ToString();
                txtDescription.Text = metadata.Description;
                lvNames.Items.Clear();
                foreach (var person in metadata.People)
                {
                    var item = new ListViewItem(person.Name);
                    item.SubItems.Add(person.Relationship);
                    lvNames.Items.Add(item);
                }
            }
            else
            {
                // Do nothing — metadata is absent or unreadable
            }
        }
        private void btnRestart_click(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Play();
        }



        private void btnFixOrient_click(object sender, EventArgs e)
        {
            SharedCode.FixImageOrientation(Dfilename);
            picBox.Image = Image.FromFile(Dfilename);
            SharedCode.Updatethumb(Dfilename);
        }
        private void SubformClosed(object sender, FormClosedEventArgs e)
        {

            string selectedPerson = Label1.Text;
            if (Strings.Len(selectedPerson) > 3)
            {
                int i = Label1.Text.IndexOf("|");
                selectedPerson = Strings.Mid(Label1.Text, 1, i);
                string insertindex = Strings.Mid(Label1.Text, i + 2);
                if (NameArray.Count == 0)
                {
                    NameArray.Add(selectedPerson);
                }
                else
                {
                    NameArray = SharedCode.ModifyPeopleList(string.Join(",", NameArray), Conversions.ToInteger(insertindex), 1, selectedPerson).Split(',').ToList();
                }
            }
            FillListview();
        }
        private void lvNames_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = lvNames.HitTest(e.Location).Item.Index;
                if (index >= 0)
                {
                    lvNames.Items[index].Selected = true; // Highlights the item
                    DeletePerson(sender, e);
                }
            }
        }


        private void DeletePerson(object sender, MouseEventArgs e)
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
                selectedname = SharedCode.GetMemberID(lvNames.SelectedItems[0].Text, "");
                newplist = SharedCode.DeleteAPerson(selectedname, SFileName);
                NameArray.Remove(selectedname);
                Debug.WriteLine($"Attempting delete: [{SFileName}]");
                Debug.WriteLine($"Length: {SFileName.Length}");

                connection = Manager.GetConnection();
                using (connection)
                {
                    var Command = new SQLiteCommand("Delete from NamePhoto where npID =@name and npFilename =@filename", connection);
                    Command.Parameters.AddWithValue("@name", Conversions.ToInteger(selectedname));
                    Command.Parameters.AddWithValue("@filename", SFileName);
                    int Result = Command.ExecuteNonQuery();
                    Console.WriteLine(Result);
                }
            }
            FillListview();
        }


    }
}