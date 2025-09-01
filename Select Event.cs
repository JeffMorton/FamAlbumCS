using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{
    public partial class Select_Event
    {
        public string Etype { get; set; }
        private TextBox txtEvent = new TextBox();
        private TextBox txtEventDetails = new TextBox();
        private string[] NamesSelected;
        private Button btnContinue;
        private ComboBox cbEventsOnFile;
        public int cnt = 0;
        public string DefaultDir;
        private ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private SQLiteConnection connection = new SQLiteConnection();
        private string strEvent;
        private MenuStrip menuStrip = new MenuStrip();
        private ListBox lvNamesSelected = new ListBox();
        private Panel selPanel = new Panel() { Dock = DockStyle.Fill };

        public Select_Event()
        {
            btnContinue = new Button()
            {
                Text = "Continue",
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                Font = new Font("Arial", 12f, FontStyle.Bold),
                Size = new Size(250, 40),
                Enabled = false
            };
            cbEventsOnFile = new ComboBox();
            InitializeComponent();
        }

        private void Selected_Event_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            WindowState = FormWindowState.Maximized;
            cnt = 0;
            NamesSelected = new string[6];
            NamesSelected[0] = "-2";
            NamesSelected[1] = "99999";
            NamesSelected[2] = "99999";
            NamesSelected[3] = "99999";
            NamesSelected[4] = "99999";
            NamesSelected[5] = "99999";

            Controls.Add(selPanel);
            // Me.AutoSize = True
            var title = new Label()
            {
                Text = "Family Album",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("segoe", 40),
                Size = new Size(600, 65),
                Location = new Point((int)Math.Round(screenWidth / 2d - 300d), 22),
                
            };
            title.BringToFront();
            Controls.Add(title); 
            title.BringToFront();
            var subtitle = new Label()
            {
                Text = "Select People",
                Font = new Font("segoe", 24f),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(600, 45),
                Location = new Point((int)Math.Round(screenWidth / 2d - 300d), 100)
            };
            Controls.Add(subtitle);
            switch (Etype ?? "")
            {
                case "Old":
                    {
                        subtitle.Text = "Select Event for New Images";
                        break;
                    }
                case "New":
                    {
                        subtitle.Text = "Enter New Event";
                        btnContinue.Enabled = true;
                        break;
                    }

                default:
                    {
                        subtitle.Text = "Select Events";
                        break;
                    }
            }
            selPanel.Controls.Add(subtitle);
            menuStrip = fmmenus.fmenus();
            selPanel.Controls.Add(menuStrip);

            if (Etype == "Ign")
            {
                menuStrip.Items.RemoveAt(2);
                var menuItemSelectEvent = new ToolStripMenuItem("Select Event") { Font = new Font("Segoe UI", 9.0f, FontStyle.Bold) };
                menuStrip.Items.Insert(2, menuItemSelectEvent);
                var ClearSelected = new ToolStripMenuItem("Clear Selected Events");
                ClearSelected.Click += ClearSelected_Click;
                menuItemSelectEvent.DropDownItems.Add(ClearSelected);
            }

            {
                var withBlock = cbEventsOnFile;
                withBlock.Location = new Point((int)Math.Round(screenWidth / 2d - 400d), 150);
                withBlock.Size = new Size(800, 18);
                withBlock.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock.MaxDropDownItems = 6;
                withBlock.DropDownHeight = 150;
            }

            {
                ref var withBlock1 = ref lvNamesSelected;
                withBlock1.ForeColor = Color.DarkBlue;
                withBlock1.Font = new Font("Arial", 12f, FontStyle.Bold);
                withBlock1.Size = new Size(800, 130);
                withBlock1.Location = new Point((int)Math.Round(screenWidth / 2d - 400d), 400);
            }
            selPanel.Controls.Add(lvNamesSelected);

            var lb1 = new Label()
            {
                Text = "Choose up to 5 Events",
                Location = new Point((int)Math.Round((screenWidth - 400) / 2d), 350),
                Font = new Font("Arial", 16f, FontStyle.Bold),
                Size = new Size(400, 39),
                TextAlign = ContentAlignment.MiddleCenter
            };

            CenterControl(btnContinue, 580);
            selPanel.Controls.Add(btnContinue);

            {
                ref var withBlock2 = ref txtEvent;
                withBlock2.Location = new Point((int)Math.Round(screenWidth / 2d - 400d), 150);
                withBlock2.Size = new Size(800, 30);
                withBlock2.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock2.Multiline = false;
                withBlock2.Visible = false;
                withBlock2.AutoSize = true;

            }
            var lblEvent = new Label()
            {
                Text = "Event:",
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point((int)Math.Round(screenWidth / 2d - 600d), 150),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12f, FontStyle.Regular),
                Visible = false,
                AutoSize = false
            };
            {
                ref var withBlock3 = ref txtEventDetails;
                withBlock3.Location = new Point((int)Math.Round(screenWidth / 2d - 400d), 230);
                withBlock3.Size = new Size(800, 350);
                withBlock3.Font = new Font("Arial", 12f, FontStyle.Regular);
                withBlock3.Multiline = true;
                withBlock3.Visible = false;
                withBlock3.AutoSize = true;
            }
            var lblEventDetails = new Label()
            {
                Text = "Event Details:",
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point((int)Math.Round(screenWidth / 2d - 600d), 230),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12f, FontStyle.Regular),
                Visible = false,
                AutoSize = false
            };
            selPanel.Controls.Add(txtEvent);
            selPanel.Controls.Add(txtEventDetails);
            selPanel.Controls.Add(lblEvent);
            selPanel.Controls.Add(lblEventDetails);
            btnContinue.Click += btnContinue_click;
            cbEventsOnFile.SelectionChangeCommitted += cbEventsOnFile_SelectedIndexChanged;
            if (Etype == "New")
            {
                cbEventsOnFile.Visible = false;
                txtEvent.Visible = true;
                txtEventDetails.Visible = true;
                lblEvent.Visible = true;
                lblEventDetails.Visible = true;
            }

            selPanel.Controls.Add(lb1);
            if (Etype == "Old")
            {
                lvNamesSelected.Visible = false;
                btnContinue.Visible = false;
                lb1.Visible = false;
            }
            else if (Etype == "New")
            {
                lvNamesSelected.Visible = false;
                btnContinue.Visible = true;
                lb1.Visible = false;
            }

            selPanel.Controls.Add(cbEventsOnFile);


            var dt = new DataTable();
            dt = SharedCode.FillEvents();

            cbEventsOnFile.DataSource = dt;
            cbEventsOnFile.DisplayMember = "Event";
            cbEventsOnFile.ValueMember = "ID";
            cbEventsOnFile.Focus();
            // cbEventsOnFile.DroppedDown = True
            cnt = 0;
        }
        private void cbEventsOnFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            cnt += 1;
            if (cnt >= 6)
            {
                MessageBox.Show("Only 5 events can be selected");
                return;
            }

            DataRowView drv = cbEventsOnFile.SelectedItem as DataRowView;
            if (drv is not null)
            {
                if (!string.IsNullOrEmpty(Strings.Trim(drv[0].ToString())))
                {
                    if (Etype == "Old")
                    {
                        string EventID = drv[1].ToString();
                        var ad = new AddPhoto();
                        ad.EventID = Conversions.ToInteger(EventID);
                        ad.Show();
                        Close();
                    }

                    NamesSelected[cnt] = Conversions.ToString(drv[1]);
                    lvNamesSelected.Items.Add(Conversions.ToString(drv[0]));
                    btnContinue.Enabled = true;
                }
            }


        }

        private void btnContinue_click(object sender, EventArgs e)
        {
            if (Etype == "New")
            {
                SharedCode.SaveNewEvent(txtEvent.Text, txtEventDetails.Text);
            }
            // Dim NewImage As New ImagesNotinDatabase
            // NewImage.Show()
            else
            {
                var thumbForm = new Sthumb() { NamesSelected = NamesSelected };
                thumbForm.Show();
            }
        }
        private void CenterControl(Control ctrl, int y)
        {
            // Calculate the position to center the control
            int x = (ClientSize.Width - ctrl.Width) / 2;
            // Dim y As Integer = (Me.ClientSize.Height - ctrl.Height) \ 2

            // Set the control's position

            ctrl.Location = new Point(x, y);
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
        private void MenuItemPeople_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}