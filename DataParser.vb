Imports System.IO
Imports System.Text
Imports FFMpegCore
Imports System.Diagnostics
Imports System.Text.Json
Imports System.Text.RegularExpressions


Module EncodeDataParser

    'Public Function ParseMetadataComment(filePath As String) As ParsedMetadata
    '    Dim rawComment As String = ReadMetadataComment(filePath)
    '    Dim parsed As New ParsedMetadata()

    '    If rawComment.StartsWith("No embedded comment") Then Return parsed

    '    Dim lines() As String = rawComment.Split({" | "}, StringSplitOptions.RemoveEmptyEntries)
    '    For Each line In lines
    '        If line.StartsWith("Event Name:") Then
    '            parsed.EventName = line.Substring("Event Name:".Length).Trim()
    '        ElseIf line.StartsWith("Event Details:") Then
    '            parsed.EventDetails = line.Substring("Event Details:".Length).Trim()
    '        ElseIf line.Trim().StartsWith("-") Then
    '            Dim parts = line.Trim().Substring(1).Trim()
    '            Dim openParen = parts.LastIndexOf("("c)
    '            Dim closeParen = parts.LastIndexOf(")"c)
    '            If openParen > 0 AndAlso closeParen > openParen Then
    '                Dim name = parts.Substring(0, openParen).Trim()
    '                Dim relationship = parts.Substring(openParen + 1, closeParen - openParen - 1).Trim()
    '                parsed.People.Add((name, relationship))
    '            End If
    '        End If
    '    Next

    '    Return parsed
    'End Function
    Private Function RunProcessAndCaptureOutput(exePath As String, arguments As String) As String
        Dim psi As New ProcessStartInfo(exePath, arguments) With {
        .RedirectStandardOutput = True,
        .UseShellExecute = False,
        .CreateNoWindow = True
    }
        Using proc As Process = Process.Start(psi)
            Return proc.StandardOutput.ReadToEnd()
        End Using
    End Function

    Private Function RunProcessAndCaptureError(exePath As String, arguments As String) As String
        Dim psi As New ProcessStartInfo(exePath, arguments) With {
        .RedirectStandardError = True,
        .UseShellExecute = False,
        .CreateNoWindow = True
    }
        Using proc As Process = Process.Start(psi)
            Return proc.StandardError.ReadToEnd()
        End Using
    End Function
    Public Function ReadMetadataComment(filePath As String) As String
        ' First attempt: use ffprobe to read metadata comment
        Dim probeArgs As String = $"-v quiet -show_entries format_tags=comment -of default=noprint_wrappers=1 ""{filePath}"""
        Dim ffprobeOutput As String = RunProcessAndCaptureOutput("ffprobe", probeArgs)

        If Not String.IsNullOrWhiteSpace(ffprobeOutput) Then
            For Each line In ffprobeOutput.Split(Environment.NewLine)
                If line.StartsWith("comment=") Then
                    Return line.Substring("comment=".Length).Trim()
                End If
            Next
        End If

        ' Fallback: use ffmpeg to read stderr and extract comment line
        Dim ffmpegArgs As String = $"-i ""{filePath}"""
        Dim ffmpegErrorOutput As String = RunProcessAndCaptureError("ffmpeg", ffmpegArgs)

        For Each line In ffmpegErrorOutput.Split(Environment.NewLine)
            If line.Trim().ToLower().StartsWith("comment") Then
                Dim parts = line.Split(":"c, 2)
                If parts.Length = 2 Then
                    Return parts(1).Trim()
                End If
            End If
        Next

        ' If nothing was found, return an empty string
        Return ""
    End Function



    'Private Function RunProcessOutput(exe As String, args As String) As String
    '    Dim psi As New ProcessStartInfo(exe, args) With {
    '    .UseShellExecute = False,
    '    .RedirectStandardOutput = True,
    '    .CreateNoWindow = True
    '}
    '    Using proc As Process = Process.Start(psi)
    '        Return proc.StandardOutput.ReadToEnd()
    '    End Using
    'End Function

    'Private Function RunProcessError(exe As String, args As String) As String
    '    Dim psi As New ProcessStartInfo(exe, args) With {
    '    .UseShellExecute = False,
    '    .RedirectStandardError = True,
    '    .CreateNoWindow = True
    '}
    '    Using proc As Process = Process.Start(psi)
    '        Return proc.StandardError.ReadToEnd()
    '    End Using
    'End Function

    Public Enum MediaType
        Photo
        Video
    End Enum
    Public Function DetectMediaType(filePath As String) As MediaType
        Dim ext As String = Path.GetExtension(filePath).ToLowerInvariant()

        Select Case ext
            Case ".jpg", ".jpeg", ".png", ".bmp", ".gif"
                Return MediaType.Photo

            Case ".mp4", ".mov", ".avi", ".mkv", ".webm"
                Return MediaType.Video

            Case Else
                Throw New NotSupportedException($"Unsupported file extension: {ext}")
        End Select
    End Function

    Public Function Quote(s As String) As String
        Return $"""{s}"""
    End Function

    '    Public Function BuildFfmpegArgs(
    '    mediaType As MediaType,
    '    inputPath As String,
    '    outputPath As String,
    '    metadataComment As String
    ') As String
    '        metadataComment = metadataComment.Replace("""", "'")
    '        inputPath = Quote(inputPath)
    '        outputPath = Quote(outputPath)

    '        If mediaType = MediaType.Photo Then
    '            ' Cleaner and compatible for static image metadata updates
    '            Return $"-i {inputPath} -metadata comment=""{metadataComment}"" -y {outputPath}"
    '        ElseIf mediaType = MediaType.Video Then
    '            Return $"-i {inputPath} -metadata comment=""{metadataComment}"" -c copy -map_metadata 0 {outputPath}"
    '        Else
    '            Throw New ArgumentException("Invalid media type.")
    '        End If
    '    End Function

    Public Function BuildJsonMetadata(
    txtEvent As TextBox,
    txtEventDetails As TextBox,
    lstPeople As ListView
) As String
        Dim metadata As New Dictionary(Of String, Object) From {
        {"Event", txtEvent.Text},
        {"Details", txtEventDetails.Text}
    }

        Dim people As New List(Of Dictionary(Of String, String))()

        For Each item As ListViewItem In lstPeople.Items
            If item.SubItems.Count >= 2 Then
                people.Add(New Dictionary(Of String, String) From {
                {"Name", item.SubItems(0).Text},
                {"Relationship", item.SubItems(1).Text}
            })
            End If
        Next

        metadata("People") = people

        ' Serialize to JSON using System.Text.Json
        Dim options As New JsonSerializerOptions With {.WriteIndented = False}
        Dim json = JsonSerializer.Serialize(metadata)
        Dim escapedJson = json.Replace("""", "\""")
        Return escapedJson

    End Function
    '    Public Function EmbedMetadata(
    '    mediaType As MediaType,
    '    inputPath As String,
    '    outputPath As String,
    '    metadataComment As String
    ') As Boolean
    '        Dim ffmpegPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe")
    '        If Not File.Exists(ffmpegPath) Then
    '            MessageBox.Show("FFmpeg executable not found in application directory.", "Missing FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '            Return False
    '        End If

    '        Dim args As String = BuildFfmpegArgs(mediaType, inputPath, outputPath, metadataComment)

    '        Dim psi As New ProcessStartInfo(ffmpegPath, args) With {
    '        .UseShellExecute = False,
    '        .RedirectStandardError = True,
    '        .RedirectStandardOutput = True,
    '        .CreateNoWindow = True
    '    }

    '        Using proc As Process = Process.Start(psi)
    '            Dim stderr As String = proc.StandardError.ReadToEnd()
    '            proc.WaitForExit()

    '            If proc.ExitCode = 0 Then
    '                Return True
    '            Else
    '                MessageBox.Show("FFmpeg error:" & vbCrLf & stderr, "Embed Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '                Return False
    '            End If
    '        End Using
    '    End Function
    Public Class PersonEntry
        Public Property Name As String
        Public Property Relationship As String
    End Class

    Public Class EventMetadata
        Public Property EventTitle As String
        Public Property Details As String
        Public Property People As List(Of PersonEntry)
    End Class


    Public Sub LoadMetadataComment(json As String, txtEvent As TextBox, txtEventDetails As TextBox, lstPeople As ListView)
        Try
            Dim metadata As EventMetadata = JsonSerializer.Deserialize(Of EventMetadata)(json)

            txtEvent.Text = metadata.EventTitle
            txtEventDetails.Text = metadata.Details

            lstPeople.Items.Clear()
            For Each person In metadata.People
                Dim item As New ListViewItem(person.Name)
                item.SubItems.Add(person.Relationship)
                lstPeople.Items.Add(item)
            Next
        Catch ex As Exception
            MessageBox.Show("Could not read metadata: " & ex.Message, "Metadata Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Public Sub WriteMetadata(filePath As String, metadata As Dictionary(Of String, String))
        Dim arguments As New List(Of String)

        For Each kvp In metadata
            arguments.Add($"-{kvp.Key}={kvp.Value}")
        Next

        arguments.Add($"-overwrite_original")
        arguments.Add($"--")
        arguments.Add($"'{filePath}'")

        Dim psi As New ProcessStartInfo("exiftool.exe", String.Join(" ", arguments))
        psi.UseShellExecute = False
        psi.CreateNoWindow = True
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True

        Using proc As Process = Process.Start(psi)
            Dim result As String = proc.StandardOutput.ReadToEnd()
            Dim err As String = proc.StandardError.ReadToEnd()
            proc.WaitForExit()
            Console.WriteLine("Metadata written: " & result)
            Console.WriteLine("ExifTool error: " & err)
        End Using

    End Sub
    '    Public Function FlattenMetadataForExif(
    '    txtEvent As TextBox,
    '    txtEventDetails As TextBox,
    '    lstPeople As ListView
    ') As Dictionary(Of String, String)

    '        Dim flattened As New Dictionary(Of String, String) From {
    '        {"Title", txtEvent.Text},
    '        {"Description", txtEventDetails.Text}
    '    }

    '        ' Combine people into a summary string
    '        Dim peopleTags As New List(Of String)()
    '        For Each item As ListViewItem In lstPeople.Items
    '            If item.SubItems.Count >= 2 Then
    '                peopleTags.Add($"{item.SubItems(0).Text} ({item.SubItems(1).Text})")
    '            End If
    '        Next

    '        If peopleTags.Count > 0 Then
    '            flattened("Keywords") = String.Join("; ", peopleTags)
    '        End If

    '        Return flattened
    '    End Function
    Public Function ConvertPngToJpeg(pngFile As String) As String
        Dim jpegFile As String = Path.ChangeExtension(pngFile, ".jpg")
        Dim psi As New ProcessStartInfo("ffmpeg", $"-y -i ""{pngFile}"" -f image2 -vcodec mjpeg ""{jpegFile}""") With {
    .CreateNoWindow = True,
    .UseShellExecute = False,
    .RedirectStandardOutput = True,
    .RedirectStandardError = True
}

        Using proc As New Process()
            proc.StartInfo = psi

            Dim stdOutBuilder As New System.Text.StringBuilder()
            Dim stdErrBuilder As New System.Text.StringBuilder()

            AddHandler proc.OutputDataReceived, Sub(sender, e)
                                                    If e.Data IsNot Nothing Then stdOutBuilder.AppendLine(e.Data)
                                                End Sub
            AddHandler proc.ErrorDataReceived, Sub(sender, e)
                                                   If e.Data IsNot Nothing Then stdErrBuilder.AppendLine(e.Data)
                                               End Sub

            proc.Start()
            proc.BeginOutputReadLine()
            proc.BeginErrorReadLine()
            proc.WaitForExit()
        End Using
        If Not File.Exists(jpegFile) Then
            Throw New IOException($"Failed to create JPEG: {jpegFile}")
        End If

        Return jpegFile
    End Function
    Public Function GetMimeTypeUsingFFprobe(filePath As String) As String
        Dim psi As New ProcessStartInfo("ffprobe", $"-v error -select_streams v:0 -show_entries stream=codec_name -of default=nw=1 ""{filePath}""") With {
        .CreateNoWindow = True,
        .UseShellExecute = False,
        .RedirectStandardOutput = True,
        .RedirectStandardError = True
    }

        Using proc As Process = Process.Start(psi)
            Dim output As String = proc.StandardOutput.ReadToEnd()
            proc.WaitForExit()
            If output.Contains("mjpeg") Then
                Return "image/jpeg"
            ElseIf output.Contains("png") Then
                Return "image/png"
            Else
                Return "unknown"
            End If
        End Using
    End Function
    Public Sub EmbedSmartMetadata(filePath As String, metadata As Dictionary(Of String, String))
        Dim mimeType = GetMimeTypeUsingFFprobe(filePath)
        ' If PNG masquerading as JPG, convert it
        If mimeType = "image/png" AndAlso Path.GetExtension(filePath).ToLowerInvariant() = ".png" Then
            filePath = ConvertPngToJpeg(filePath)
            mimeType = "image/jpeg" '
            ' Update after conversion
        End If
        ' Embed only for supported types
    End Sub
    Public Sub EmbedJsonMetadataWithExifTool(filePath As String, jsonMetadata As String)
        Dim escapedJson = jsonMetadata.Replace("""", "\""") ' escape quotes for CLI

        Dim args = $"-overwrite_original -Comment=""{escapedJson}"" ""{filePath}"""
        Dim psi As New ProcessStartInfo("exiftool", args) With {
        .CreateNoWindow = True,
        .UseShellExecute = False,
        .RedirectStandardOutput = True,
        .RedirectStandardError = True
    }

        Using proc As Process = Process.Start(psi)
            Dim stdOut = proc.StandardOutput.ReadToEnd()
            Dim stdErr = proc.StandardError.ReadToEnd()
            proc.WaitForExit()

            ' Optional: Logging
            Debug.WriteLine("ExifTool Output: " & stdOut)
            If Not String.IsNullOrWhiteSpace(stdErr) Then Debug.WriteLine("ExifTool Error: " & stdErr)
        End Using
    End Sub


    'Public Sub EmbedJsonComment(filePath As String, metadata As Dictionary(Of String, Object))
    '    'jbm
    '    ' Serialize the dictionary to JSON
    '    Dim json As String = JsonSerializer.Serialize(metadata)

    '    ' Escape double quotes for command-line safety
    '    Dim escapedJson As String = json.Replace("""", "\""")

    '    ' Build ExifTool arguments to embed the JSON in the Comment tag
    '    Dim arguments As String = $"-overwrite_original -Comment=""{escapedJson}"" ""{filePath}"""

    '    ' Set up process
    '    Dim psi As New ProcessStartInfo("exiftool", arguments) With {
    '    .CreateNoWindow = True,
    '    .UseShellExecute = False,
    '    .RedirectStandardOutput = True,
    '    .RedirectStandardError = True
    '}

    '    Using proc As Process = Process.Start(psi)
    '        Dim stdOut As String = proc.StandardOutput.ReadToEnd()
    '        Dim stdErr As String = proc.StandardError.ReadToEnd()
    '        proc.WaitForExit()

    '        ' Optional: log output
    '        If Not String.IsNullOrWhiteSpace(stdOut) Then Debug.WriteLine("ExifTool Output: " & stdOut)
    '        If Not String.IsNullOrWhiteSpace(stdErr) Then Debug.WriteLine("ExifTool Error: " & stdErr)
    '    End Using
    'End Sub



    Public Function GetActualMimeType(filePath As String) As String
        Dim psi As New ProcessStartInfo("exiftool.exe", $"-MIMEType -s3 ""{filePath}""")
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True
        psi.UseShellExecute = False
        psi.CreateNoWindow = True

        Try
            Using proc As Process = Process.Start(psi)
                Dim output As String = proc.StandardOutput.ReadToEnd().Trim()
                Dim errors As String = proc.StandardError.ReadToEnd().Trim()
                proc.WaitForExit()

                If String.IsNullOrWhiteSpace(output) Then
                    Debug.WriteLine("ExifTool output is empty.")
                    Debug.WriteLine("ExifTool errors: " & errors)
                End If

                Return output.ToLowerInvariant()
            End Using
        Catch ex As Exception
            Debug.WriteLine("ExifTool failed: " & ex.Message)
            Return ""
        End Try
    End Function
    'Public Sub EmbedJsonComment(filePath As String, metadata As Dictionary(Of String, Object))
    '    ' Serialize the dictionary to JSON
    '    Dim json As String = JsonSerializer.Serialize(metadata)

    '    ' Escape double quotes for command-line safety
    '    Dim escapedJson As String = json.Replace("""", "\""")

    '    ' Build ExifTool arguments to embed the JSON in the Comment tag
    '    Dim arguments As String = $"-overwrite_original -Comment=""{escapedJson}"" ""{filePath}"""

    '    ' Set up process
    '    Dim psi As New ProcessStartInfo("exiftool", arguments) With {
    '    .CreateNoWindow = True,
    '    .UseShellExecute = False,
    '    .RedirectStandardOutput = True,
    '    .RedirectStandardError = True
    '}

    '    Using proc As Process = Process.Start(psi)
    '        Dim stdOut As String = proc.StandardOutput.ReadToEnd()
    '        Dim stdErr As String = proc.StandardError.ReadToEnd()
    '        proc.WaitForExit()

    '        ' Optional: log output
    '        If Not String.IsNullOrWhiteSpace(stdOut) Then Debug.WriteLine("ExifTool Output: " & stdOut)
    '        If Not String.IsNullOrWhiteSpace(stdErr) Then Debug.WriteLine("ExifTool Error: " & stdErr)
    '    End Using
    'End Sub

    Public Function ReadJsonMetadataFromExif(filePath As String) As Dictionary(Of String, Object)
        Dim psi As New ProcessStartInfo("exiftool", $"-s -s -s -Comment ""{filePath}""") With {
            .CreateNoWindow = True,
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True
        }

        Using proc As Process = Process.Start(psi)
            Dim output As String = proc.StandardOutput.ReadToEnd().Trim()
            Dim errors As String = proc.StandardError.ReadToEnd()
            proc.WaitForExit()

            If Not String.IsNullOrWhiteSpace(errors) Then
                Throw New Exception("ExifTool error: " & errors)
            End If

            If Not String.IsNullOrWhiteSpace(output) Then
                Dim cleaned = FixLooseJson(output)
                Debug.WriteLine("Fixed JSON: " & cleaned)
                Return JsonSerializer.Deserialize(Of Dictionary(Of String, Object))(cleaned)
            End If
        End Using

        Return Nothing
    End Function



    Private Function FixLooseJson(input As String) As String
        ' Step 1: Add quotes around property names
        Dim quotedKeys = Regex.Replace(input, "([{,])\s*(\w+)\s*:", "$1""$2"":", RegexOptions.Compiled)

        ' Step 2: Quote string values that are unquoted and don't start with [ { or "
        Dim quotedValues = Regex.Replace(quotedKeys, ":\s*(?![""

\[{])([^,\]

}]+)", Function(m)
           Dim val = m.Groups(1).Value.Trim()
           ' Avoid quoting numbers, true, false, null
           If Regex.IsMatch(val, "^(true|false|null|\d+(\.\d+)?)$", RegexOptions.IgnoreCase) Then
               Return ":" & val
           End If
           Return ":" & """" & val.TrimEnd(","c) & """"
       End Function, RegexOptions.Compiled)

        ' Final polish
        quotedValues = quotedValues.Replace("\u0022", """").Replace("""""", """")
        Return quotedValues
    End Function

End Module