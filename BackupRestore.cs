using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FamAlbum
{

    public static class BackupRest
    {
        // Private Sub BackupRestore_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        // Dim Lbox As New Label With {
        // .Text = "Select a directory for storing the database backsups",
        // .Font = New Font("Arial", 24)
        // },
        // CenterControl(Lbox, 0)
        // Me.Controls.Add(Lbox)
        // Lbox.Show()
        // GetBackupDirectroy()
        // End Sub
        public static string GetBackupPath()
        {
            // Open the registry key
            var key = Registry.CurrentUser.OpenSubKey(@"Software\FamilyAlbum", writable: false);

            if (key is not null)
            {
                var value = key.GetValue("BackupPath", null);
                key.Close();

                if (value is not null)
                {
                    return value.ToString();
                }
            }

            // If key or value not found, fallback to prompt
            return getpath();

        }


        public static void BackupSQLiteDatabase()
        {

            string sourcePath = Path.Combine(SharedCode.GetDefaultDir(), "FamilyAlbum.db");
            var sourceConn = new SQLiteConnection($"Data Source={sourcePath}; Version=3;");
            string BackupPath = GetBackupPath() + "FamAlbum" + DateTime.Now.ToString("MMddyyyy") + ".bak";

            var destConn = new SQLiteConnection("Data Source=" + BackupPath + "; Version=3;");

            try
            {
                sourceConn.Open();
                destConn.Open();

                // Perform the backup from source to destination
                sourceConn.BackupDatabase(destConn, "main", "main", -1, null, 0);

                MessageBox.Show("Backup completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Backup failed: " + ex.Message);
            }
            finally
            {
                sourceConn.Close();
                destConn.Close();
            }
        }

        public static void RestoreSQLiteDatabase()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Backup File";
                ofd.Filter = "FamilyAlbum(*.bak)|*.bak|All Files (*.*)|*.*";
                ofd.InitialDirectory = GetBackupPath();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string backupPath = ofd.FileName;
                    string restorePath = Path.Combine(SharedCode.GetDefaultDir(), "FamilyAlbum.db");

                    var backupConn = new SQLiteConnection($"Data Source={backupPath}; Version=3;");
                    var restoreConn = new SQLiteConnection($"Data Source={restorePath}; Version=3;");

                    try
                    {
                        backupConn.Open();
                        restoreConn.Open();

                        backupConn.BackupDatabase(restoreConn, "main", "main", -1, null, 0);

                        MessageBox.Show("Restore completed successfully to: " + restorePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Restore failed: " + ex.Message);
                    }
                    finally
                    {
                        backupConn.Close();
                        restoreConn.Close();
                    }
                }
            }
        }


        public static void GetBackupDirectroy()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a folder to store database backups";
                fbd.ShowNewFolderButton = true;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = fbd.SelectedPath;
                    SaveFilePathToRegistry(selectedPath);
                }
            }

        }
        private static void SaveFilePathToRegistry(string filePath)
        {
            try
            {
                // Access the CurrentUser registry key and create a subkey
                var key = Registry.CurrentUser.CreateSubKey(@"Software\FamilyAlbum");
                key.SetValue("BackupPath", filePath);
                key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to registry: " + ex.Message);
            }
        }

        public static string getpath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a folder for Database backups";
                fbd.ShowNewFolderButton = true;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = fbd.SelectedPath + @"\";
                    SaveFilePathToRegistry(selectedPath);
                    return selectedPath;
                }
            }

            return null; // If user cancels dialog
        }

    }
}