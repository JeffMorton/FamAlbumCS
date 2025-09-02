<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamAlbum
{


    static class unindexedfiles
    {
        private static ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private static SQLiteConnection connection = new SQLiteConnection();
        private static byte[] thumbnailBytes;
        private static int itype;
        private readonly static string _connectionString;
        private static List<string> convertedFiles = new List<string>();
        private static readonly HashSet<string> ExcludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "SourceCode"
};
        // Constants
        private const string INDEX_MARKER = "_index";

        // SQL Queries
        private const string SQL_CHECK_EXISTS = "SELECT COUNT(*) FROM Pictures WHERE PFileName = @FileName";
        private const string SQL_SAVE_UNINDEXED = @"INSERT INTO UnindexedFiles (uiFileName, uiDirectory, uiThumb,uiType,uiWidth,uiHeight,uiVtime,uiStatus) 
         VALUES (@filename, @path, @thumb,@type,@width,@height,@time,@Status)";
        private static string lpath;
        private static int y = 0;
        private static string filename;
        private static string fname;
        private static string dir = SharedCode.GetDefaultDir();

        public static void RunUnindexedFileSearchWithSplash()
        {
            var splash = new working();
            splash.Location = new Point(500, 800);
            splash.Show();
            Application.DoEvents();

            Task.Run(() =>
                {
                    HeavyWorkResult result = DoHeavyWork();

                    splash.Invoke(() =>
           {
                        splash.Close();
                        MessageBox.Show($"Search complete  {result} images found");
                    });
                });
        }

        public static HeavyWorkResult DoHeavyWork()
        {
            int skipped = 0;
            int processed = 0;
            convertedFiles.Clear();

            using (var connection = Manager.GetConnection())
            {
                SharedCode.ConfigureFFMpeg();

                // Load existing filenames from DB
                var existingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var preloadCmd = new SQLiteCommand("SELECT PFileName FROM Pictures", connection))
                using (var reader = preloadCmd.ExecuteReader())
                {
                    while (reader.Read())
                        existingFiles.Add(reader.GetString(0).Trim());
                }

                foreach (var imagePath in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    if (IsInExcludedFolder(imagePath, dir))
                        continue;

                    string currentFile = imagePath; // Safe copy for mutation
                    string ext = Path.GetExtension(currentFile).ToLowerInvariant();
                    string fname = currentFile.Replace(dir, "").Trim();

                    if (existingFiles.Contains(fname))
                    {
                        skipped++;
                        continue;
                    }

                    if (ext == ".jpg" || ext == ".jpeg")
                    {
                        if (jsonlist.IsPngDisguisedAsJpg(currentFile))
                        {
                            string tempJpeg = jsonlist.ConvertPngToJpegSilent(currentFile);
                            if (!string.IsNullOrEmpty(tempJpeg))
                            {
                                File.Delete(currentFile);
                                File.Move(tempJpeg, currentFile);
                                convertedFiles.Add(currentFile);
                                currentFile = tempJpeg; // Update reference safely
                            }
                        }

                        try
                        {
                            SharedCode.FixImageOrientation(currentFile);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Orientation fix failed for {currentFile}: {ex.Message}");
                        }
                    }

                    string filename = Path.GetFileName(currentFile);
                    string ldir = Path.GetDirectoryName(currentFile);
                    string lpath = ldir.Replace(dir, "").Trim();

                    if (!ShouldSkipFile(filename, ref itype))
                    {
                        ProcessImageFile(fname, currentFile, lpath, ref itype, ref skipped, ref processed, connection);
                    }
                }

                if (convertedFiles.Count > 0)
                {
                    string msg = $"Converted {convertedFiles.Count} disguised PNGs:" + Environment.NewLine +
                                 string.Join(Environment.NewLine, convertedFiles.Take(10));
                    MessageBox.Show(msg, "Format Fix Report");
                }

                return new HeavyWorkResult { Skipped = skipped, Processed = processed };
            }
        }
        
        private static bool IsInExcludedFolder(string filePath, string rootDir)
        {
            string relativePath = Path.GetDirectoryName(filePath).Replace(rootDir, "").TrimStart(Path.DirectorySeparatorChar);
            string[] segments = relativePath.Split(Path.DirectorySeparatorChar);

            return segments.Any(segment => ExcludedFolders.Contains(segment));
        }

        public static bool ProcessImageFile(string filename, string imageFile, string lPath, ref int itype, ref int x, ref int y, SQLiteConnection connection)
        {
            // If ShouldSkipFile(filename, itype) Then
            // Return True
            // End If


            // If FileExistsInDatabase(connection, filename) Then
            // x += 1
            // Return True
            // End If

            y += 1;
            CreateThumbnail(imageFile, itype, connection);
            return true;

        }

        private static bool ShouldSkipFile(string filename, ref int itype)
        {
            if (filename.ToLower().Contains(INDEX_MARKER))
            {
                return true;
            }
            string extension = Path.GetExtension(filename).ToLower();
            // Remove the dot if you want

            extension = extension.TrimStart('.');
            string[] picArray = new string[] { "jpg", "png", "jpeg" };
            string[] mArray = new string[] { "mov", "mp4", "avi" };
            if (picArray.Contains(extension))
            {
                itype = 1;
                return false;
            }
            else if (mArray.Contains(extension))
            {
                itype = 2;
                return false;
            }
            else
            {
                return true;
            }

        }
        private static void CreateThumbnail(string imageFile, int itype, SQLiteConnection connection)
        {
            try
            {
                if (itype == 1)
                {
                    // handles photos
                    CreateThumb(imageFile, connection);
                }
                else
                {
                    // Handle video type 
                    createMthumb(imageFile, connection);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating thumbnail: {ex.Message}");

            }
        }
        private static bool SaveToDatabase(SQLiteConnection connection, string filename, string lPath, byte[] thumbnailBytes, ref int y, ref int itype, int width, int height, int tm)
        {
            try
            {
                // Guard against overly long strings
                if (filename.Length > 200 || lPath.Length > 200)
                {
                    MessageBox.Show("Filename or path exceeds 200 characters.");
                    return false;
                }

                filename = WebUtility.HtmlDecode(filename);
                using (var command = new SQLiteCommand(SQL_SAVE_UNINDEXED, connection))
                {
                    command.Parameters.Add(new SQLiteParameter("@filename", DbType.String) { Value = filename });
                    command.Parameters.Add(new SQLiteParameter("@path", DbType.String) { Value = lPath });
                    command.Parameters.Add(new SQLiteParameter("@type", DbType.Int32) { Value = itype });
                    command.Parameters.Add(new SQLiteParameter("@Status", DbType.String) { Value = "N" });
                    command.Parameters.Add(new SQLiteParameter("@width", DbType.Int32) { Value = width });
                    command.Parameters.Add(new SQLiteParameter("@height", DbType.Int32) { Value = height });
                    command.Parameters.Add(new SQLiteParameter("@time", DbType.Int32) { Value = tm });
                    Debug.WriteLine($"ThumbnailBytes: {(thumbnailBytes is null ? "null" : thumbnailBytes.Length.ToString())}");

                    if (thumbnailBytes is null || thumbnailBytes.Length == 0)
                    {
                        command.Parameters.Add(new SQLiteParameter("@thumb", DbType.Binary) { Value = DBNull.Value });
                    }
                    else
                    {
                        command.Parameters.Add(new SQLiteParameter("@thumb", DbType.Binary) { Value = thumbnailBytes });
                    }

                    command.ExecuteNonQuery();
                    // File.WriteAllBytes("c:\thumnails\test_image.jpg", thumbnailBytes)
                    // Process.Start("c:\thumnails\test_image.jpg")
                    return true;

                }
            }




            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint || ex.ResultCode == SQLiteErrorCode.Constraint_PrimaryKey)
                {
                    y -= 1;
                    return true;
                }
                MessageBox.Show($"SQLite error:   {ex.Message} (#{ex.ResultCode})");
                return false;
            }

            catch (Exception ex)
            {
                MessageBox.Show($"General error saving '{filename}': {ex.Message}");
                return false;
            }
        }
        public static byte[] CreateThumb(string filename, SQLiteConnection connection)
        {
            int thumbSize = 150;
            byte[] bytes = null;

            try
            {
                using (var original = Image.FromFile(filename))
                {
                    // Calculate scale ratio
                    double ratio = Math.Min(thumbSize / (double)original.Width, thumbSize / (double)original.Height);
                    int newWidth = (int)Math.Round(original.Width * ratio);
                    int newHeight = (int)Math.Round(original.Height * ratio);

                    // Create 150x150 canvas
                    using (var canvas = new Bitmap(thumbSize, thumbSize))
                    {
                        using (var g = Graphics.FromImage(canvas))
                        {
                            g.Clear(Color.White); // Optional: background color
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                            // Center the scaled image
                            int xOffset = (thumbSize - newWidth) / 2;
                            int yOffset = (thumbSize - newHeight) / 2;

                            g.DrawImage(original, xOffset, yOffset, newWidth, newHeight);
                        }

                        bytes = SharedCode.ImageToByteArray(canvas, System.Drawing.Imaging.ImageFormat.Jpeg);
                        Console.WriteLine("Thumbnail byte length: " + bytes.Length);
                        string ldir = Path.GetDirectoryName(filename);
                        string lpath = ldir.Replace(dir, "").Trim();

                        // Save to DB (assuming y, itype are defined elsewhere)
                        SaveToDatabase(connection, filename, lpath, bytes, ref y, ref itype, original.Width, original.Height, 0);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Thumbnail creation failed: " + ex.Message);
            }

            return bytes;
        }

        private static void createMthumb(string imagepath, SQLiteConnection connection)
        {
            int width = 240;
            int height = 320;
            int t = 5;
            string ldir = Path.GetDirectoryName(imagepath);
            string lpath = ldir.Replace(dir, "").Trim();

            // Corrected function call
            var videoData = ThumbnailExtractor.GetVideoData(imagepath, width, height, t);
            int argy = y;
            int argitype = itype;
            SaveToDatabase(connection, imagepath, lpath, videoData.ThumbnailBytes, ref argy, ref argitype, videoData.Width, videoData.Height, (int)Math.Round(videoData.Duration));

        }
        private static bool FileExistsInDatabase(SQLiteConnection connection, string filename)
        {
            using (var command = new SQLiteCommand(SQL_CHECK_EXISTS, connection))
            {
                command.Parameters.AddWithValue("@FileName", filename);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }
=======
﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamAlbum
{


    static class unindexedfiles
    {
        private static ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());
        private static SQLiteConnection connection = new SQLiteConnection();
        private static byte[] thumbnailBytes;
        private static int itype;
        private readonly static string _connectionString;
        private static List<string> convertedFiles = new List<string>();
        private static readonly HashSet<string> ExcludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "SourceCode"
};
        // Constants
        private const string INDEX_MARKER = "_index";

        // SQL Queries
        private const string SQL_CHECK_EXISTS = "SELECT COUNT(*) FROM Pictures WHERE PFileName = @FileName";
        private const string SQL_SAVE_UNINDEXED = @"INSERT INTO UnindexedFiles (uiFileName, uiDirectory, uiThumb,uiType,uiWidth,uiHeight,uiVtime,uiStatus) 
         VALUES (@filename, @path, @thumb,@type,@width,@height,@time,@Status)";
        private static string lpath;
        private static int y = 0;
        private static string filename;
        private static string fname;
        private static string dir = SharedCode.GetDefaultDir();

        public static void RunUnindexedFileSearchWithSplash()
        {
            var splash = new working();
            splash.Location = new Point(500, 800);
            splash.Show();
            Application.DoEvents();

            Task.Run(() =>
                {
                    HeavyWorkResult result = DoHeavyWork();

                    splash.Invoke(() =>
           {
                        splash.Close();
                        MessageBox.Show($"Search complete  {result} images found");
                    });
                });
        }

        public static HeavyWorkResult DoHeavyWork()
        {
            int skipped = 0;
            int processed = 0;
            convertedFiles.Clear();

            using (var connection = Manager.GetConnection())
            {
                SharedCode.ConfigureFFMpeg();

                // Load existing filenames from DB
                var existingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var preloadCmd = new SQLiteCommand("SELECT PFileName FROM Pictures", connection))
                using (var reader = preloadCmd.ExecuteReader())
                {
                    while (reader.Read())
                        existingFiles.Add(reader.GetString(0).Trim());
                }

                foreach (var imagePath in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    if (IsInExcludedFolder(imagePath, dir))
                        continue;

                    string currentFile = imagePath; // Safe copy for mutation
                    string ext = Path.GetExtension(currentFile).ToLowerInvariant();
                    string fname = currentFile.Replace(dir, "").Trim();

                    if (existingFiles.Contains(fname))
                    {
                        skipped++;
                        continue;
                    }

                    if (ext == ".jpg" || ext == ".jpeg")
                    {
                        if (jsonlist.IsPngDisguisedAsJpg(currentFile))
                        {
                            string tempJpeg = jsonlist.ConvertPngToJpegSilent(currentFile);
                            if (!string.IsNullOrEmpty(tempJpeg))
                            {
                                File.Delete(currentFile);
                                File.Move(tempJpeg, currentFile);
                                convertedFiles.Add(currentFile);
                                currentFile = tempJpeg; // Update reference safely
                            }
                        }

                        try
                        {
                            SharedCode.FixImageOrientation(currentFile);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Orientation fix failed for {currentFile}: {ex.Message}");
                        }
                    }

                    string filename = Path.GetFileName(currentFile);
                    string ldir = Path.GetDirectoryName(currentFile);
                    string lpath = ldir.Replace(dir, "").Trim();

                    if (!ShouldSkipFile(filename, ref itype))
                    {
                        ProcessImageFile(fname, currentFile, lpath, ref itype, ref skipped, ref processed, connection);
                    }
                }

                if (convertedFiles.Count > 0)
                {
                    string msg = $"Converted {convertedFiles.Count} disguised PNGs:" + Environment.NewLine +
                                 string.Join(Environment.NewLine, convertedFiles.Take(10));
                    MessageBox.Show(msg, "Format Fix Report");
                }

                return new HeavyWorkResult { Skipped = skipped, Processed = processed };
            }
        }
        
        private static bool IsInExcludedFolder(string filePath, string rootDir)
        {
            string relativePath = Path.GetDirectoryName(filePath).Replace(rootDir, "").TrimStart(Path.DirectorySeparatorChar);
            string[] segments = relativePath.Split(Path.DirectorySeparatorChar);

            return segments.Any(segment => ExcludedFolders.Contains(segment));
        }

        public static bool ProcessImageFile(string filename, string imageFile, string lPath, ref int itype, ref int x, ref int y, SQLiteConnection connection)
        {
            // If ShouldSkipFile(filename, itype) Then
            // Return True
            // End If


            // If FileExistsInDatabase(connection, filename) Then
            // x += 1
            // Return True
            // End If

            y += 1;
            CreateThumbnail(imageFile, itype, connection);
            return true;

        }

        private static bool ShouldSkipFile(string filename, ref int itype)
        {
            if (filename.ToLower().Contains(INDEX_MARKER))
            {
                return true;
            }
            string extension = Path.GetExtension(filename).ToLower();
            // Remove the dot if you want

            extension = extension.TrimStart('.');
            string[] picArray = new string[] { "jpg", "png", "jpeg" };
            string[] mArray = new string[] { "mov", "mp4", "avi" };
            if (picArray.Contains(extension))
            {
                itype = 1;
                return false;
            }
            else if (mArray.Contains(extension))
            {
                itype = 2;
                return false;
            }
            else
            {
                return true;
            }

        }
        private static void CreateThumbnail(string imageFile, int itype, SQLiteConnection connection)
        {
            try
            {
                if (itype == 1)
                {
                    // handles photos
                    CreateThumb(imageFile, connection);
                }
                else
                {
                    // Handle video type 
                    createMthumb(imageFile, connection);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating thumbnail: {ex.Message}");

            }
        }
        private static bool SaveToDatabase(SQLiteConnection connection, string filename, string lPath, byte[] thumbnailBytes, ref int y, ref int itype, int width, int height, int tm)
        {
            try
            {
                // Guard against overly long strings
                if (filename.Length > 200 || lPath.Length > 200)
                {
                    MessageBox.Show("Filename or path exceeds 200 characters.");
                    return false;
                }

                filename = WebUtility.HtmlDecode(filename);
                using (var command = new SQLiteCommand(SQL_SAVE_UNINDEXED, connection))
                {
                    command.Parameters.Add(new SQLiteParameter("@filename", DbType.String) { Value = filename });
                    command.Parameters.Add(new SQLiteParameter("@path", DbType.String) { Value = lPath });
                    command.Parameters.Add(new SQLiteParameter("@type", DbType.Int32) { Value = itype });
                    command.Parameters.Add(new SQLiteParameter("@Status", DbType.String) { Value = "N" });
                    command.Parameters.Add(new SQLiteParameter("@width", DbType.Int32) { Value = width });
                    command.Parameters.Add(new SQLiteParameter("@height", DbType.Int32) { Value = height });
                    command.Parameters.Add(new SQLiteParameter("@time", DbType.Int32) { Value = tm });
                    Debug.WriteLine($"ThumbnailBytes: {(thumbnailBytes is null ? "null" : thumbnailBytes.Length.ToString())}");

                    if (thumbnailBytes is null || thumbnailBytes.Length == 0)
                    {
                        command.Parameters.Add(new SQLiteParameter("@thumb", DbType.Binary) { Value = DBNull.Value });
                    }
                    else
                    {
                        command.Parameters.Add(new SQLiteParameter("@thumb", DbType.Binary) { Value = thumbnailBytes });
                    }

                    command.ExecuteNonQuery();
                    // File.WriteAllBytes("c:\thumnails\test_image.jpg", thumbnailBytes)
                    // Process.Start("c:\thumnails\test_image.jpg")
                    return true;

                }
            }




            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint || ex.ResultCode == SQLiteErrorCode.Constraint_PrimaryKey)
                {
                    y -= 1;
                    return true;
                }
                MessageBox.Show($"SQLite error:   {ex.Message} (#{ex.ResultCode})");
                return false;
            }

            catch (Exception ex)
            {
                MessageBox.Show($"General error saving '{filename}': {ex.Message}");
                return false;
            }
        }
        public static byte[] CreateThumb(string filename, SQLiteConnection connection)
        {
            int thumbSize = 150;
            byte[] bytes = null;

            try
            {
                using (var original = Image.FromFile(filename))
                {
                    // Calculate scale ratio
                    double ratio = Math.Min(thumbSize / (double)original.Width, thumbSize / (double)original.Height);
                    int newWidth = (int)Math.Round(original.Width * ratio);
                    int newHeight = (int)Math.Round(original.Height * ratio);

                    // Create 150x150 canvas
                    using (var canvas = new Bitmap(thumbSize, thumbSize))
                    {
                        using (var g = Graphics.FromImage(canvas))
                        {
                            g.Clear(Color.White); // Optional: background color
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                            // Center the scaled image
                            int xOffset = (thumbSize - newWidth) / 2;
                            int yOffset = (thumbSize - newHeight) / 2;

                            g.DrawImage(original, xOffset, yOffset, newWidth, newHeight);
                        }

                        bytes = SharedCode.ImageToByteArray(canvas, System.Drawing.Imaging.ImageFormat.Jpeg);
                        Console.WriteLine("Thumbnail byte length: " + bytes.Length);
                        string ldir = Path.GetDirectoryName(filename);
                        string lpath = ldir.Replace(dir, "").Trim();

                        // Save to DB (assuming y, itype are defined elsewhere)
                        SaveToDatabase(connection, filename, lpath, bytes, ref y, ref itype, original.Width, original.Height, 0);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Thumbnail creation failed: " + ex.Message);
            }

            return bytes;
        }

        private static void createMthumb(string imagepath, SQLiteConnection connection)
        {
            int width = 240;
            int height = 320;
            int t = 5;
            string ldir = Path.GetDirectoryName(imagepath);
            string lpath = ldir.Replace(dir, "").Trim();

            // Corrected function call
            var videoData = ThumbnailExtractor.GetVideoData(imagepath, width, height, t);
            int argy = y;
            int argitype = itype;
            SaveToDatabase(connection, imagepath, lpath, videoData.ThumbnailBytes, ref argy, ref argitype, videoData.Width, videoData.Height, (int)Math.Round(videoData.Duration));

        }
        private static bool FileExistsInDatabase(SQLiteConnection connection, string filename)
        {
            using (var command = new SQLiteCommand(SQL_CHECK_EXISTS, connection))
            {
                command.Parameters.AddWithValue("@FileName", filename);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }
>>>>>>> 7cba59675801a0b8e862e7f0276de92de193daa6
}