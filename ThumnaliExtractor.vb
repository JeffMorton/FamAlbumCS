Imports System.IO
Imports System.Text
Public Class VideoData
    Public Property ThumbnailBytes As Byte()
    Public Property Width As Integer
    Public Property Height As Integer
    Public Property Duration As Double
End Class

Public Class ThumbnailExtractor


    Public Shared Function GetVideoData(ByVal videoFilePath As String, ByVal outputWidth As Integer, ByVal outputHeight As Integer, ByVal timestampSeconds As Integer) As VideoData
        Dim videoData As New VideoData()

        Try
            ' Define paths
            Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
            Dim ffmpegPath As String = Path.Combine(baseDir, "ffmpeg.exe")
            Dim ffprobePath As String = Path.Combine(baseDir, "ffprobe.exe")
            Dim tempThumbnailPath As String = Path.Combine(baseDir, "thumbnail.jpg")

            ' Generate Thumbnail
            Dim thumbnailArgs As String = String.Format(
    "-y -nostdin -ss {0} -i ""{1}"" -vf ""scale=w={2}:h=-1"" -vframes 1 ""{3}""",
    timestampSeconds, videoFilePath, outputWidth, tempThumbnailPath)

            Dim psi As New ProcessStartInfo() With {
                .FileName = ffmpegPath,
                .Arguments = thumbnailArgs,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Dim outputBuilder As New StringBuilder()
            Dim errorBuilder As New StringBuilder()

            ' Start FFmpeg process
            Using proc As Process = Process.Start(psi)
                AddHandler proc.OutputDataReceived, Sub(sender, e)
                                                        If Not String.IsNullOrEmpty(e.Data) Then outputBuilder.AppendLine(e.Data)
                                                    End Sub
                AddHandler proc.ErrorDataReceived, Sub(sender, e)
                                                       If Not String.IsNullOrEmpty(e.Data) Then errorBuilder.AppendLine(e.Data)
                                                   End Sub
                proc.BeginOutputReadLine()
                proc.BeginErrorReadLine()

                ' Apply timeout to prevent hanging
                Dim exited As Boolean = proc.WaitForExit(30000)
                If Not exited Then
                    proc.Kill()
                    Throw New Exception("FFmpeg did not exit in time and was forcefully killed.")
                End If
            End Using

            ' Check if thumbnail was created
            If Not File.Exists(tempThumbnailPath) Then
                Throw New Exception("Thumbnail generation failed.")
            End If

            ' Read thumbnail bytes
            videoData.ThumbnailBytes = File.ReadAllBytes(tempThumbnailPath)
            'File.Delete(tempThumbnailPath)

            ' Extract Metadata (Width, Height, Duration)
            Dim metadataArgs As String = "-v error -select_streams v:0 -show_entries stream=width,height,duration -of csv=p=0 " & Chr(34) & videoFilePath & Chr(34)
            Dim metadataPsi As New ProcessStartInfo() With {
                .FileName = ffprobePath,
                .Arguments = metadataArgs,
                .RedirectStandardOutput = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Using proc As Process = Process.Start(metadataPsi)
                Using reader As StreamReader = proc.StandardOutput
                    Dim output As String = reader.ReadToEnd()
                    Dim values As String() = output.Split(","c)

                    If values.Length = 4 Then
                        videoData.Width = Convert.ToInt32(values(0))
                        videoData.Height = Convert.ToInt32(values(1))
                        videoData.Duration = Convert.ToDouble(values(2))
                    End If
                End Using
                proc.WaitForExit()
            End Using

        Catch ex As Exception
            Throw New Exception("Error processing video data: " & ex.Message, ex)
        End Try

        Return videoData
    End Function
End Class


