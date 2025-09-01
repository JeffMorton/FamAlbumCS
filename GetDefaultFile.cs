using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace FamAlbum
{
    public partial class GetDefaultFile
    {
        public GetDefaultFile()
        {
            InitializeComponent();
        }

        private void GetDefaultFile_Load(object sender, EventArgs e)
        {
            var Lbox = new Label()
            {
                Text = "Select The Family Album Database",
                Font = new Font("Arial", 24f)
            };
            CenterControl(Lbox, 0);
            Controls.Add(Lbox);
            Lbox.Show();
            FindDefaultDir();
        }
        private void FindDefaultDir()
        {
            // Create and configure an OpenFileDialog
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Select a File",
                Filter = "All Files|*.*"
            };

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                int X = filePath.IndexOf("FamilyAlbum.db");
                filePath = Strings.Mid(filePath, 1, X);

                // Save the file path to the registry
                SaveFilePathToRegistry(filePath);

                // Display confirmation
                var Strt = new Start();
                Strt.Show();
            }
        }
        private void SaveFilePathToRegistry(string filePath)
        {
            try
            {
                // Access the CurrentUser registry key and create a subkey
                var key = Registry.CurrentUser.CreateSubKey(@"Software\FamilyAlbum");
                key.SetValue("DefaultDir", filePath);
                key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to registry: " + ex.Message);
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

    }
}