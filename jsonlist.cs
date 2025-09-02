<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{

    static class jsonlist
    {
        public class Person
        {
            public string Name { get; set; }
            public string Relationship { get; set; }
        }
        public class ParsedMetadata
        {
            public string EventName { get; set; }
            public string EventDetails { get; set; }
            public int imMonth { get; set; }
            public int imYear { get; set; }
            public string Description { get; set; }
            public List<Person> People { get; set; } = new List<Person>();
        }
        public static string BuildJsonFromControls(TextBox txtEvent, TextBox txtEventDetails, TextBox txtMonth, TextBox txtYear, TextBox txtDescription, ListView lvPeople)
        {
            var metadata = new ParsedMetadata()
            {
                EventName = txtEvent.Text,
                EventDetails = txtEventDetails.Text,
                imMonth = Information.IsNumeric(txtMonth.Text) && !string.IsNullOrEmpty(txtMonth.Text) ? Conversions.ToInteger(txtMonth.Text) : 0,
                Description = txtDescription.Text,
                imYear = Information.IsNumeric(txtYear.Text) && !string.IsNullOrEmpty(txtYear.Text) ? Conversions.ToInteger(txtYear.Text) : 0,
                People = new List<Person>()
            };

            foreach (ListViewItem item in lvPeople.Items)
            {
                if (item.SubItems.Count >= 2)
                {
                    metadata.People.Add(new Person()
                    {
                        Name = item.SubItems[0].Text,
                        Relationship = item.SubItems[1].Text
                    });
                }
            }

            return JsonSerializer.Serialize(metadata, new JsonSerializerOptions() { WriteIndented = true });
        }
        public static void WriteJsonMetadataToMediaFile(string filePath, string jsonMetadata)
        {
            // Step 1: Write JSON to a temporary file
            string tempJsonFile = Path.GetTempFileName();
            File.WriteAllText(tempJsonFile, jsonMetadata);

            // Step 2: Use XMP-dc:Description (works with both images & videos)
            var arguments = new List<string>() { $"-XMP-dc:Description<={tempJsonFile}", "-overwrite_original", "--", $"\"{filePath}\"" };

            // Step 3: Set up ExifTool process
            var psi = new ProcessStartInfo("exiftool.exe", string.Join(" ", arguments))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Step 4: Run the process
            using (var proc = Process.Start(psi))
            {
                string result = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                Console.WriteLine("ExifTool Output: " + result);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Console.WriteLine("ExifTool Error: " + err);
                }
            }

            // Step 5: Clean up
            try
            {
                if (File.Exists(tempJsonFile))
                {
                    File.Delete(tempJsonFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Temp file cleanup failed: " + ex.Message);
            }
        }
        public static ParsedMetadata ReadJsonFromMediaFile(string filePath)
        {
            // Use the XMP-dc:Description tag which works for images AND videos
            string arguments = $"-XMP-dc:Description \"{filePath}\"";
            var psi = new ProcessStartInfo("exiftool.exe", arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    // Look for: "Description : {JSON string here}"
                    string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith("Description"))
                        {
                            int colonIndex = line.IndexOf(":");
                            if (colonIndex > -1 && colonIndex + 1 < line.Length)
                            {
                                string rawJson = line.Substring(colonIndex + 1).Trim();

                                // Basic cleanup
                                string cleanJson = rawJson.Replace(@"\u0027", "'").Replace(@"\u003c", "<").Replace(@"\u003e", ">").Replace("..", "");

                                if (cleanJson.StartsWith("{"))
                                {
                                    // Try to deserialize into ParsedMetadata
                                    return JsonSerializer.Deserialize<ParsedMetadata>(cleanJson);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail — don't interrupt workflow
                Console.WriteLine("Metadata read skipped: " + ex.Message);
            }

            // No metadata found or couldn't parse
            return null;
        }
        public static bool IsPngDisguisedAsJpg(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var sig = new byte[8];
                    fs.Read(sig, 0, sig.Length);
                    return sig[0] == 0x89 && sig[1] == 0x50 && sig[2] == 0x4E && sig[3] == 0x47 && sig[4] == 0xD && sig[5] == 0xA && sig[6] == 0x1A && sig[7] == 0xA;
                }
            }
            catch
            {
                return false;
            }
        }
        public static string ConvertPngToJpegSilent(string pngFile)
        {
            string tempJpegFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            var psi = new ProcessStartInfo("ffmpeg", $"-y -loglevel quiet -i \"{pngFile}\" -f image2 -vcodec mjpeg \"{tempJpegFile}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            try
            {
                using (var proc = new Process())
                {
                    proc.StartInfo = psi;
                    proc.Start();
                    proc.WaitForExit();
                }

                if (File.Exists(tempJpegFile))
                {
                    return tempJpegFile;
                }
            }
            catch
            {
                // Silent failure – no action
            }

            return null;
        }
    }
=======
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace FamAlbum
{

    static class jsonlist
    {
        public class Person
        {
            public string Name { get; set; }
            public string Relationship { get; set; }
        }
        public class ParsedMetadata
        {
            public string EventName { get; set; }
            public string EventDetails { get; set; }
            public int imMonth { get; set; }
            public int imYear { get; set; }
            public string Description { get; set; }
            public List<Person> People { get; set; } = new List<Person>();
        }
        public static string BuildJsonFromControls(TextBox txtEvent, TextBox txtEventDetails, TextBox txtMonth, TextBox txtYear, TextBox txtDescription, ListView lvPeople)
        {
            var metadata = new ParsedMetadata()
            {
                EventName = txtEvent.Text,
                EventDetails = txtEventDetails.Text,
                imMonth = Information.IsNumeric(txtMonth.Text) && !string.IsNullOrEmpty(txtMonth.Text) ? Conversions.ToInteger(txtMonth.Text) : 0,
                Description = txtDescription.Text,
                imYear = Information.IsNumeric(txtYear.Text) && !string.IsNullOrEmpty(txtYear.Text) ? Conversions.ToInteger(txtYear.Text) : 0,
                People = new List<Person>()
            };

            foreach (ListViewItem item in lvPeople.Items)
            {
                if (item.SubItems.Count >= 2)
                {
                    metadata.People.Add(new Person()
                    {
                        Name = item.SubItems[0].Text,
                        Relationship = item.SubItems[1].Text
                    });
                }
            }

            return JsonSerializer.Serialize(metadata, new JsonSerializerOptions() { WriteIndented = true });
        }
        public static void WriteJsonMetadataToMediaFile(string filePath, string jsonMetadata)
        {
            // Step 1: Write JSON to a temporary file
            string tempJsonFile = Path.GetTempFileName();
            File.WriteAllText(tempJsonFile, jsonMetadata);

            // Step 2: Use XMP-dc:Description (works with both images & videos)
            var arguments = new List<string>() { $"-XMP-dc:Description<={tempJsonFile}", "-overwrite_original", "--", $"\"{filePath}\"" };

            // Step 3: Set up ExifTool process
            var psi = new ProcessStartInfo("exiftool.exe", string.Join(" ", arguments))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Step 4: Run the process
            using (var proc = Process.Start(psi))
            {
                string result = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                Console.WriteLine("ExifTool Output: " + result);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Console.WriteLine("ExifTool Error: " + err);
                }
            }

            // Step 5: Clean up
            try
            {
                if (File.Exists(tempJsonFile))
                {
                    File.Delete(tempJsonFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Temp file cleanup failed: " + ex.Message);
            }
        }
        public static ParsedMetadata ReadJsonFromMediaFile(string filePath)
        {
            // Use the XMP-dc:Description tag which works for images AND videos
            string arguments = $"-XMP-dc:Description \"{filePath}\"";
            var psi = new ProcessStartInfo("exiftool.exe", arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    // Look for: "Description : {JSON string here}"
                    string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith("Description"))
                        {
                            int colonIndex = line.IndexOf(":");
                            if (colonIndex > -1 && colonIndex + 1 < line.Length)
                            {
                                string rawJson = line.Substring(colonIndex + 1).Trim();

                                // Basic cleanup
                                string cleanJson = rawJson.Replace(@"\u0027", "'").Replace(@"\u003c", "<").Replace(@"\u003e", ">").Replace("..", "");

                                if (cleanJson.StartsWith("{"))
                                {
                                    // Try to deserialize into ParsedMetadata
                                    return JsonSerializer.Deserialize<ParsedMetadata>(cleanJson);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail — don't interrupt workflow
                Console.WriteLine("Metadata read skipped: " + ex.Message);
            }

            // No metadata found or couldn't parse
            return null;
        }
        public static bool IsPngDisguisedAsJpg(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var sig = new byte[8];
                    fs.Read(sig, 0, sig.Length);
                    return sig[0] == 0x89 && sig[1] == 0x50 && sig[2] == 0x4E && sig[3] == 0x47 && sig[4] == 0xD && sig[5] == 0xA && sig[6] == 0x1A && sig[7] == 0xA;
                }
            }
            catch
            {
                return false;
            }
        }
        public static string ConvertPngToJpegSilent(string pngFile)
        {
            string tempJpegFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            var psi = new ProcessStartInfo("ffmpeg", $"-y -loglevel quiet -i \"{pngFile}\" -f image2 -vcodec mjpeg \"{tempJpegFile}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            try
            {
                using (var proc = new Process())
                {
                    proc.StartInfo = psi;
                    proc.Start();
                    proc.WaitForExit();
                }

                if (File.Exists(tempJpegFile))
                {
                    return tempJpegFile;
                }
            }
            catch
            {
                // Silent failure – no action
            }

            return null;
        }
    }
>>>>>>> 7cba59675801a0b8e862e7f0276de92de193daa6
}