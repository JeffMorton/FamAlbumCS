using System;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{

    public partial class working
    {
        public working()
        {
            InitializeComponent();
        }
        private void Working_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            Size = new Size(300, 100);
            BackColor = Color.DarkOrange;
            var lb = new Label()
            {
                Text = "Please wait...",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(90, 40)
            };
            Controls.Add(lb);
        }
    }
}