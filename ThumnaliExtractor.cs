using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FamAlbum
{
    public class VideoData
    {
        public byte[] ThumbnailBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Duration { get; set; }
    }

    public class ThumbnailExtractor
    {


        public static VideoData GetVideoData(string videoFilePath, int outputWidth, int outputHeight, int timestampSeconds)
        {
            var videoData = new VideoData();

            try
            {
                // Define paths
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string ffmpegPath = Path.Combine(baseDir, "ffmpeg.exe");
                string ffprobePath = Path.Combine(baseDir, "ffprobe.exe");
                string tempThumbnailPath = Path.Combine(baseDir, "thumbnail.jpg");

                // Generate Thumbnail
                string thumbnailArgs = string.Format("-y -nostdin -ss {0} -i \"{1}\" -vf \"scale=w={2}:h=-1\" -vframes 1 \"{3}\"", timestampSeconds, videoFilePath, outputWidth, tempThumbnailPath);

                var psi = new ProcessStartInfo()
                {
                    FileName = ffmpegPath,
                    Arguments = thumbnailArgs,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                // Start FFmpeg process
                using (var proc = Process.Start(psi))
                {
                    proc.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) outputBuilder.AppendLine(e.Data); };
                    proc.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) errorBuilder.AppendLine(e.Data); };
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    // Apply timeout to prevent hanging
                    bool exited = proc.WaitForExit(30000);
                    if (!exited)
                    {
                        proc.Kill();
                        throw new Exception("FFmpeg did not exit in time and was forcefully killed.");
                    }
                }

                // Check if thumbnail was created
                if (!File.Exists(tempThumbnailPath))
                {
                    throw new Exception("Thumbnail generation failed.");
                }

                // Read thumbnail bytes
                videoData.ThumbnailBytes = File.ReadAllBytes(tempThumbnailPath);
                // File.Delete(tempThumbnailPath)

                // Extract Metadata (Width, Height, Duration)
                string metadataArgs = "-v error -select_streams v:0 -show_entries stream=width,height,duration -of csv=p=0 " + '"' + videoFilePath + '"';
                var metadataPsi = new ProcessStartInfo()
                {
                    FileName = ffprobePath,
                    Arguments = metadataArgs,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(metadataPsi))
                {
                    using (var reader = proc.StandardOutput)
                    {
                        string output = reader.ReadToEnd();
                        string[] values = output.Split(',');

                        if (values.Length == 4)
                        {
                            videoData.Width = Convert.ToInt32(values[0]);
                            videoData.Height = Convert.ToInt32(values[1]);
                            videoData.Duration = Convert.ToDouble(values[2]);
                        }
                    }
                    proc.WaitForExit();
                }
            }

            catch (Exception ex)
            {
                throw new Exception("Error processing video data: " + ex.Message, ex);
            }

            return videoData;
        }
    }
}