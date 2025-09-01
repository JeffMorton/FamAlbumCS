Imports System.IO

Module CheckFiletType
    Public Function GetMimeTypeUsingFFprobe(filePath As String) As String
        Try
            Dim psi As New ProcessStartInfo("ffprobe", $"-v error -select_streams v:0 -show_entries stream=codec_name -of default=nw=1 ""{filePath}""") With {
            .CreateNoWindow = True,
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True
        }

            Using proc As Process = Process.Start(psi)
                Dim output As String = proc.StandardOutput.ReadToEnd().Trim().ToLowerInvariant()
                proc.WaitForExit()

                If output.Contains("mjpeg") Then Return "image/jpeg"
                If output.Contains("png") Then Return "image/png"
            End Using
        Catch
            ' Silent fail
        End Try
        Return "unknown"
    End Function
    Public Sub FixHiddenPngfile(filepath As String)
        Dim mimeType = GetMimeTypeUsingFFprobe(filepath)
        Dim ext = Path.GetExtension(filepath).ToLowerInvariant()

        If mimeType = "image/png" AndAlso ext = ".jpg" Then
            ' Possibly mislabeled PNG—optional: log or skip silently
            Return ' Skip silently
        ElseIf mimeType = "image/png" AndAlso ext = ".png" Then
            filepath = ConvertPngToJpeg(filepath)
            mimeType = "image/jpeg" ' Reset MIME type after conversion
        End If


    End Sub

End Module
