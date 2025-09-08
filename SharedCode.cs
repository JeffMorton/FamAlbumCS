using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FFMpegCore;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;

namespace FamAlbum
{
    static class SharedCode
    {
        private static ConnectionManager Manager = new ConnectionManager(@"Data Source=C:\Family Album\FamilyAlbum.db;Version=3;");
        private static SQLiteConnection connection = new SQLiteConnection();

        public static string GetDefaultDir()
        {
            // Open the registry key
            var key = Registry.CurrentUser.OpenSubKey(@"Software\FamilyAlbum");
            if (key is not null)
            {
                string value = key.GetValue("DefaultDir", "Default Value").ToString();
                key.Close();
                return value;

            }
            else
            {
                var GetDir = new GetDefaultFile();
                GetDir.Show();
                return null;
            }
        }
        public static string ModifyPeopleList(string oldList, int Pos, int D, string name)
        {
            // Normalize the input list
            if (oldList == "1")
                oldList = "";

            // Split and sanitize the list
            var items = oldList.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            string cleanName = name.Trim();

            if (D > 0)
            {
                // Add name at specified position
                if (!string.IsNullOrWhiteSpace(cleanName))
                {
                    if (Pos >= 0 && Pos <= items.Count)
                    {
                        items.Insert(Pos, cleanName);
                    }
                    else
                    {
                        items.Add(cleanName);
                    }
                }
            }
            else
            {
                // Remove matching name
                items = items.Where(s => !s.Equals(cleanName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Return the cleaned, comma-delimited string
            return string.Join(",", items);
        }



        public static DataTable FillNames()
        {
            var dt = new DataTable();
            dt.Columns.Add("Names", typeof(string));
            dt.Columns.Add("ID", typeof(string));
            dt.Clear();
            string qryNameCnt = "Select ID,neName from NameEvent where neType ='N' order by neName";
            try
            {
                connection = Manager.GetConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
            return dt;
        }
        public static DataTable FillEvents()
        {
            var dt = new DataTable();
            dt.Columns.Add("Event", typeof(string));
            dt.Columns.Add("ID", typeof(string));
            dt.Clear();
            string qryNameCnt = "Select ID,neName from NameEvent where neType ='E' order by neName";

            connection = Manager.GetConnection();

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
            return dt;
        }
        public static string GetPPeopleList(string FileName, ref int Namecount)
        {
            connection = Manager.GetConnection();
            string NL = "";
            var command = new SQLiteCommand("SELECT  PPeoplelist,PNameCount FROM Pictures WHERE Pfilename = @name", connection);
            command.Parameters.AddWithValue("@name", FileName);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    NL = Conversions.ToString(reader["PPeopleList"]);
                    Namecount += Conversions.ToInteger(reader["PNameCount"]);
                }
            }
            connection.Close();
            return NL;
        }
        public static void SavePPeopleList(string namelt, string selectedPerson, string filename, int Namecount)
        {
            connection = Manager.GetConnection();
            using (connection)
            {

                // Begin a transaction
                var transaction = connection.BeginTransaction();

                try
                {
                    // Create a command and associate it with the transaction
                    var command = new SQLiteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    // Update Statement 1

                    try
                    {
                        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        command.CommandText = "Update Pictures set PPeoplelist = @namelist, PlastModifiedDate = @currentdate,PNameCount = @namecount WHERE PfileName = @filename";
                        command.Parameters.AddWithValue("@namelist", namelt);
                        command.Parameters.AddWithValue("@filename", filename);
                        command.Parameters.AddWithValue("@namecount", Namecount);
                        command.Parameters.AddWithValue("@currentdate", currentDate);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (!Reexists(selectedPerson, filename))
                        {
                            // Update Statement 2
                            var command2 = new SQLiteCommand("Insert into NamePhoto (npID,npFilename) values (@selectedPerson,@filename1)", connection);
                            command2.Connection = connection;
                            command2.Transaction = transaction;
                            command2.Parameters.AddWithValue("@filename1", filename);
                            command2.Parameters.AddWithValue("@selectedPerson", selectedPerson);
                            command2.ExecuteNonQuery();

                        }

                        // Commit the transaction if all updates succeed
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if an error occurs
                        transaction.Rollback();
                        MessageBox.Show("Transaction rolled back. Error: " + ex.Message);
                    }
                }
                catch (Exception EX)
                {
                    MessageBox.Show(EX.Message);
                }
            }
        }
        public static bool Reexists(string ID, string filename)
        {
            string query = "SELECT EXISTS(SELECT 1 FROM NamePhoto WHERE npID=@ID AND npFileName=@filename)";

            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@filename", filename);
                    connection.Open();
                    int result = Convert.ToInt32(command.ExecuteScalar());
                    return result == 1;
                }
            }
        }
        public static string DeleteAPerson(string name, string filename, int Namecount)
        {

            connection = Manager.GetConnection();
            string plist = GetPPeopleList(filename, ref Namecount);
            string newplist = ModifyPeopleList(plist, 0, -1, name);

            try
            {
                connection = Manager.GetConnection();
                using (connection)
                {

                    // Begin a transaction
                    var transaction = connection.BeginTransaction();

                    try
                    {
                        // Create a command and associate it with the transaction
                        var command = new SQLiteCommand();
                        command.Connection = connection;
                        command.Transaction = transaction;

                        // Update Statement 1
                        command.CommandText = "UPDATE Pictures SET PPeopleList = @plst WHERE PfileName = @filename";
                        command.Parameters.AddWithValue("@filename", filename);
                        command.Parameters.AddWithValue("@plst", newplist);

                        command.ExecuteNonQuery();

                        // Update Statement 2
                        command.CommandText = "Delete from NamePhoto where npID = @name and npfilename=@filename";
                        command.Parameters.AddWithValue("@name", name);
                        command.ExecuteNonQuery();

                        // Commit the transaction if all updates succeed
                        transaction.Commit();
                        Console.WriteLine("Transaction committed successfully.");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if an error occurs
                        transaction.Rollback();
                        Console.WriteLine("Delete Name Transaction rolled back. Error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return newplist;
        }
        public static List<string> AddNametoPeopleList(string Name, int pos, string filename)
        {
            string updatednamelist;
            var Namecount = default(int);
            // Get the current list of names
            string namelist = GetPPeopleList(filename, ref Namecount);
            if (namelist == "1")
                namelist = "";
            var items = namelist.Split(',').ToList();

            // Insert the new name at the specified position
            if (pos >= 0 & pos <= items.Count)
            {
                items.Insert(pos, Name.ToString());
            }
            else
            {
                namelist = namelist + "," + Name;
            }
            Namecount = items.Count;
            // Convert the list back to an integer array

            // Update the name list
            try
            {
                updatednamelist = string.Join(",", items);
            }
            catch 
            {
                // Handle any exception and log it if necessary
                updatednamelist = string.Join(",", items) + "," + Name;
            }

            SavePPeopleList(updatednamelist, Name, filename, Namecount);
            return items;
        }
        public static List<string> AddNametoNamelist(string Name, int pos, List<string> items)
        {
            // Sanitize the incoming name
            string cleanName = Name.Trim();

            // Only add if it's not blank
            if (!string.IsNullOrWhiteSpace(cleanName))
            {
                if (items.Count == 0)
                {
                    items.Add(cleanName);
                }
                else if (pos >= 0 && pos <= items.Count)
                {
                    items.Insert(pos, cleanName);
                }
                else
                {
                    items.Add(cleanName);
                }
            }

            // Clean the entire list: trim and remove blanks
            var cleanedList = items.Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return cleanedList;
        }

        public static string GetConnectionString()
        {
            string DDir = GetDefaultDir();
            return "Data Source=" + DDir + "FamilyAlbum.db;Version=3;";
        }
        public static bool CMovie(string Name)
        {
            string[] movieExtensions = new[] { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv" };

            // Get the file extension
            string fileExtension = Path.GetExtension(Name).ToLower();

            if (movieExtensions.Contains(fileExtension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GetMemberID(string FullName, string Relation)
        {
            string query = "select ID from NameEvent where neName = @name";
            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", FullName);
                    connection.Open();
                    var result = command.ExecuteScalar();

                    if (result is not null && !Convert.IsDBNull(result))
                    {
                        return Conversions.ToString(result);
                    }
                    else
                    {
                        return AddNewName(FullName, Relation);
                    }
                }
            }
        }
        public static string AddNewName(string FullName, string Relation)
        {
            string qryInsertName = "INSERT INTO NameEvent(neName, neRelation, neType, neDatelastModified) " + "VALUES (@name, @relation, 'N', @currentdate)  returning ID;";
            int newKey;
            connection = Manager.GetConnection();
            using (connection)
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                try
                {
                    // First command: insert into NameEvent
                    var command = new SQLiteCommand(qryInsertName, connection);
                    command.Parameters.AddWithValue("@name", FullName);
                    command.Parameters.AddWithValue("@relation", Relation);
                    command.Parameters.AddWithValue("@currentdate", currentDate);
                    newKey = Conversions.ToInteger(command.ExecuteScalar());
                    return newKey.ToString();
                }
                catch (SQLiteException EX)
                {
                    MessageBox.Show("Add Name Failed " + EX.Message);
                    return "";
                }
            }
        }
        public static Tuple<int, int> GetPhotoMonthAndYear(string imagePath)
        {
            try
            {
                // Step 1: Try EXIF DateTaken
                using (var image = Image.FromFile(imagePath))
                {
                    if (image.PropertyIdList.Contains(0x9003))
                    {
                        var prop = image.GetPropertyItem(0x9003);
                        string dateTaken = System.Text.Encoding.ASCII.GetString(prop.Value).Trim('\0');
                        var dateTime = DateTime.ParseExact(dateTaken.Substring(0, 19), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return new Tuple<int, int>(dateTime.Month, dateTime.Year);
                    }
                }

                // Step 2: Fall back to Last Modified Date
                if (File.Exists(imagePath))
                {
                    var fileInfo = new FileInfo(imagePath);
                    var lastModified = fileInfo.LastWriteTime;
                    return new Tuple<int, int>(lastModified.Month, lastModified.Year);
                }

                // Step 3: Fall back to File Creation Date
                if (File.Exists(imagePath))
                {
                    var fileInfo = new FileInfo(imagePath);
                    var fileDate = fileInfo.CreationTime;
                    return new Tuple<int, int>(fileDate.Month, fileDate.Year);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Metadata fallback failed: " + ex.Message);
            }

            // Step 4: Graceful fallback
            return new Tuple<int, int>(0, 0);
        }
        public static string GeteventID(string FullName, string relation)
        {
            string query = "SELECT ID FROM NameEvent WHERE neName = @name";

            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", FullName);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result is not null && !(result is DBNull))
                    {
                        return result.ToString();
                    }
                    else
                    {
                        return SaveNewEvent(FullName, relation);
                    }
                }
            }
        }
        public static (string EventName, string Eventdetail) GetEvent(string EventId)
        {
            string query = "select neName,neRelation from NameEvent where ID=@ID";
            connection = Manager.GetConnection();
            string EVName = "";
            string EVDetail = "";
            using (connection)
            {
                var Command = new SQLiteCommand(query, connection);
                Command.Parameters.AddWithValue("@ID", EventId);
                var reader = Command.ExecuteReader();

                while (reader.Read())
                {
                    EVName = reader["neName"].ToString();
                    if (Strings.Trim(reader["neRelation"].ToString()) == "NULL")
                    {
                        EVDetail = "";
                    }
                    else
                    {
                        EVDetail = reader["neRelation"].ToString();
                    }
                }
                return (EVName, EVDetail);
            }
        }
        public static string SaveNewEvent(string Evnt, string EventDetails)
        {
            string EventID;
            connection = Manager.GetConnection();
            using (connection)
            {
                var d = DateTime.Today;
                try
                {
                    var command = new SQLiteCommand();
                    command.Connection = connection;

                    command.CommandText = @"Insert into NameEvent (neName, neRelation, neDateLastModified, neCount, neType) 
                        values (@Event, @EventDetails, @date, 0, 'E') RETURNING ID";
                    command.Parameters.AddWithValue("@Event", Evnt);
                    command.Parameters.AddWithValue("@EventDetails", EventDetails);
                    command.Parameters.AddWithValue("@date", d);
                    EventID = Conversions.ToString(command.ExecuteScalar());
                    return EventID;
                }

                catch (Exception ex)
                {

                    MessageBox.Show("Event not saved.  Err: " + ex.Message);
                    return "";
                }
            }

        }
        public static byte[] ImageToByteArray(Image img, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, format);
                return ms.ToArray();
            }
        }
        public static void FixImageOrientation(string imagePath)
        {
            byte[] imgData = File.ReadAllBytes(imagePath);
            using (var ms = new MemoryStream(imgData))
            {
                using (Bitmap img = (Bitmap)Image.FromStream(ms))
                {
                    bool changed = false;

                    foreach (PropertyItem p in img.PropertyItems)
                    {
                        if (p.Id == 274) // EXIF Orientation Tag
                        {
                            short orientation = BitConverter.ToInt16(p.Value, 0);
                            switch (orientation)
                            {
                                case 3:
                                    {
                                        img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                        changed = true;
                                        break;
                                    }
                                case 6:
                                    {
                                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                        changed = true;
                                        break;
                                    }
                                case 8:
                                    {
                                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        changed = true;
                                        break;
                                    }
                            }
                            break;
                        }
                    }

                    if (changed)
                    {
                        string tempPath = Path.GetTempFileName();
                        img.Save(tempPath);
                        File.Copy(tempPath, imagePath, true);
                        File.Delete(tempPath);
                    }
                }
            }
        }

        public static void ConfigureFFMpeg()
        {
            GlobalFFOptions.Configure(new FFOptions()
            {
                BinaryFolder = AppDomain.CurrentDomain.BaseDirectory,   // This should be your output folder.
                TemporaryFilesFolder = Path.GetTempPath()
            });
        }
        public static int GetPendingImageCount()
        {
            int count = 0;
            string sql = "SELECT COUNT(*) FROM UnindexedFiles WHERE uiStatus = 'N'"; // Example condition
            connection = Manager.GetConnection();
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                try
                {
                    var result = cmd.ExecuteScalar();
                    if (!ReferenceEquals(result, DBNull.Value) && result is not null)
                    {
                        count = Convert.ToInt32(result);
                    }
                    else
                    {
                        count = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error counting pending images: " + ex.Message);
                }
            }

            return count;
        }
        public static void VerifyPictureFilesExist(string rootFolder)
        {
            var wk = new working();
            wk.Show();
            Application.DoEvents();
            connection = Manager.GetConnection();
            var missingFiles = new List<string>();
            string query = "SELECT pfilename FROM pictures";

            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string relativePath = reader["pfilename"].ToString();
                        string fullPath = rootFolder + relativePath;

                        if (!File.Exists(fullPath))
                        {
                            missingFiles.Add(fullPath);
                        }
                    }
                }
                UpdateNameCountsManually(connection);
            }
            My.MyProject.Forms.working.Close();
            if (missingFiles.Count > 0)
            {
                MessageBox.Show($"Missing {missingFiles.Count} files. Example:{Environment.NewLine}{string.Join(Environment.NewLine, missingFiles.Take(10))}", "Missing Files");
                // Optionally log to file:
                File.WriteAllLines(rootFolder + "missing_files.log", missingFiles);
            }
            else
            {
                MessageBox.Show("All files are present!", "Verification Complete");
            }
            wk.Close();
        }
        public static void UpdateNameCountsManually(SQLiteConnection connection)
        {
            string selectQuery = "SELECT Pfilename, PPeopleList FROM pictures";
            string updateQuery = "UPDATE pictures SET PNameCount = @count WHERE Pfilename = @id";

            using (var cmdSelect = new SQLiteCommand(selectQuery, connection))
            using (var reader = cmdSelect.ExecuteReader())
            {

                while (reader.Read())
                {
                    string id = reader["Pfilename"].ToString();
                    string rawList = Convert.ToString(reader["PPeopleList"]);
                    var names = rawList.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

                    using (var cmdUpdate = new SQLiteCommand(updateQuery, connection))
                    {
                        cmdUpdate.Parameters.AddWithValue("@count", names.Count);
                        cmdUpdate.Parameters.AddWithValue("@id", id);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
            }
        }
        public static void Updatethumb(string filename)
        {
            byte[] bytes = null;
            var original = Image.FromFile(filename);

            int orientationId = 0x112;
            if (original.PropertyIdList.Contains(orientationId))
            {
                var prop = original.GetPropertyItem(orientationId);
                ushort orientation = BitConverter.ToUInt16(prop.Value, 0);
                var rotateFlipType = RotateFlipType.RotateNoneFlipNone;

                switch (orientation)
                {
                    case 3:
                        {
                            rotateFlipType = RotateFlipType.Rotate180FlipNone;
                            break;
                        }
                    case 6:
                        {
                            rotateFlipType = RotateFlipType.Rotate90FlipNone;
                            break;
                        }
                    case 8:
                        {
                            rotateFlipType = RotateFlipType.Rotate270FlipNone;
                            break;
                        }
                }

                original.RotateFlip(rotateFlipType);
                original.RemovePropertyItem(orientationId);
            }

            int thumbSize = 150;
            string DDir = GetDefaultDir();
            // Calculate scale while preserving aspect ratio
            double ratio = Math.Min(thumbSize / (double)original.Width, thumbSize / (double)original.Height);
            int newWidth = (int)Math.Round(original.Width * ratio);
            int newHeight = (int)Math.Round(original.Height * ratio);
            string sfilename = filename.Replace(DDir, "");
            // Center the image on a 200x200 canvas
            var thumb = new Bitmap(thumbSize, thumbSize);
            using (var g = Graphics.FromImage(thumb))
            {
                g.Clear(Color.White); // Change to Color.White or any background you want
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                int x = (thumbSize - newWidth) / 2;
                int y = (thumbSize - newHeight) / 2;
                g.DrawImage(original, x, y, newWidth, newHeight);
            }
            bytes = ImageToByteArray(thumb, ImageFormat.Jpeg);
            using (var connection = new SQLiteConnection())
            {
                try
                {
                    var command = new SQLiteCommand("Update pictures set Pthumb=@thumb where Pfilename =@filename", connection);
                    command.Parameters.AddWithValue("@thumb", bytes);
                    command.Parameters.AddWithValue("@filename", sfilename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating thumb -" + ex.Message);
                }
            }
            original?.Dispose();
            thumb?.Dispose();


        }
        public static void ShowTextInPictureBox(PictureBox picBox, string message)
        {
            int width = picBox.Width;
            int height = picBox.Height;

            // Create a blank bitmap
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White); // Background color

                // Set up font and brush
                var font = new Font("Arial", 14f, FontStyle.Bold);
                var brush = new SolidBrush(Color.Black);

                // Center the text
                var textSize = g.MeasureString(message, font);
                float x = (width - textSize.Width) / 2f;
                float y = (height - textSize.Height) / 2f;

                // Draw the text
                g.DrawString(message, font, brush, x, y);
            }

            // Assign the image to the PictureBox
            picBox.Image = bmp;
        }
        public static void CleanPpeoplelistAndUpdateCount(SQLiteConnection connection)
        {
            var wk = new working();
            wk.Show();
            Application.DoEvents();

            try
            {
                var cmdSelect = new SQLiteCommand("SELECT rowid, Ppeoplelist FROM pictures", connection);
                using (var reader = cmdSelect.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int rowId = reader.GetInt32(0);
                        string rawList = reader.GetString(1);

                        // Clean and split the list
                        var cleanedItems = rawList.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                        // Rejoin into cleaned string
                        string cleanedList = string.Join(",", cleanedItems);
                        int nameCount = cleanedItems.Count;

                        // Update both fields
                        var cmdUpdate = new SQLiteCommand("UPDATE pictures SET Ppeoplelist = @cleaned, PNamecount = @count WHERE rowid = @id", connection);
                        cmdUpdate.Parameters.AddWithValue("@cleaned", cleanedList);
                        cmdUpdate.Parameters.AddWithValue("@count", nameCount);
                        cmdUpdate.Parameters.AddWithValue("@id", rowId);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cleaning Ppeoplelist and updating PNamecount: " + ex.Message);
            }
            wk.Close();
        }

    }
}