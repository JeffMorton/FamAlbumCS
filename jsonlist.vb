Imports System.IO
Imports System.Text.Json

Module jsonlist
    Public Class Person
        Public Property Name As String
        Public Property Relationship As String
    End Class
    Public Class ParsedMetadata
        Public Property EventName As String
        Public Property EventDetails As String
        Public Property imMonth As Integer
        Public Property imYear As Integer
        Public Property Description As String
        Public Property People As New List(Of Person)
    End Class
    Public Function BuildJsonFromControls(txtEvent As TextBox, txtEventDetails As TextBox, txtMonth As TextBox, txtYear As TextBox, txtDescription As TextBox, lvPeople As ListView) As String
        Dim metadata As New ParsedMetadata With {
        .EventName = txtEvent.Text,
        .EventDetails = txtEventDetails.Text,
        .imMonth = If(IsNumeric(txtMonth.Text) AndAlso txtMonth.Text <> "", CInt(txtMonth.Text), 0),
        .Description = txtDescription.Text,
        .imYear = If(IsNumeric(txtYear.Text) AndAlso txtYear.Text <> "", CInt(txtYear.Text), 0),
        .People = New List(Of Person)
    }

        For Each item As ListViewItem In lvPeople.Items
            If item.SubItems.Count >= 2 Then
                metadata.People.Add(New Person With {
                .Name = item.SubItems(0).Text,
                .Relationship = item.SubItems(1).Text
            })
            End If
        Next

        Return JsonSerializer.Serialize(metadata, New JsonSerializerOptions With {.WriteIndented = True})
    End Function
    Public Sub WriteJsonMetadataToMediaFile(filePath As String, jsonMetadata As String)
        ' Step 1: Write JSON to a temporary file
        Dim tempJsonFile As String = Path.GetTempFileName()
        File.WriteAllText(tempJsonFile, jsonMetadata)

        ' Step 2: Use XMP-dc:Description (works with both images & videos)
        Dim arguments As New List(Of String) From {
        $"-XMP-dc:Description<={tempJsonFile}",
        "-overwrite_original",
        "--",
        $"""{filePath}"""
    }

        ' Step 3: Set up ExifTool process
        Dim psi As New ProcessStartInfo("exiftool.exe", String.Join(" ", arguments)) With {
        .UseShellExecute = False,
        .CreateNoWindow = True,
        .RedirectStandardOutput = True,
        .RedirectStandardError = True
    }

        ' Step 4: Run the process
        Using proc As Process = Process.Start(psi)
            Dim result As String = proc.StandardOutput.ReadToEnd()
            Dim err As String = proc.StandardError.ReadToEnd()
            proc.WaitForExit()

            Console.WriteLine("ExifTool Output: " & result)
            If Not String.IsNullOrWhiteSpace(err) Then
                Console.WriteLine("ExifTool Error: " & err)
            End If
        End Using

        ' Step 5: Clean up
        Try
            If File.Exists(tempJsonFile) Then
                File.Delete(tempJsonFile)
            End If
        Catch ex As Exception
            Console.WriteLine("Temp file cleanup failed: " & ex.Message)
        End Try
    End Sub
    Public Function ReadJsonFromMediaFile(filePath As String) As ParsedMetadata
        ' Use the XMP-dc:Description tag which works for images AND videos
        Dim arguments As String = $"-XMP-dc:Description ""{filePath}"""
        Dim psi As New ProcessStartInfo("exiftool.exe", arguments) With {
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True
        }

        Try
            Using proc As Process = Process.Start(psi)
                Dim output As String = proc.StandardOutput.ReadToEnd()
                proc.WaitForExit()

                ' Look for: "Description : {JSON string here}"
                Dim lines = output.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                For Each line In lines
                    If line.Trim().StartsWith("Description") Then
                        Dim colonIndex = line.IndexOf(":")
                        If colonIndex > -1 AndAlso colonIndex + 1 < line.Length Then
                            Dim rawJson As String = line.Substring(colonIndex + 1).Trim()

                            ' Basic cleanup
                            Dim cleanJson As String = rawJson.
                                Replace("\u0027", "'").
                                Replace("\u003c", "<").
                                Replace("\u003e", ">").
                                Replace("..", "")

                            If cleanJson.StartsWith("{") Then
                                ' Try to deserialize into ParsedMetadata
                                Return JsonSerializer.Deserialize(Of ParsedMetadata)(cleanJson)
                            End If
                        End If
                    End If
                Next
            End Using
        Catch ex As Exception
            ' Silent fail — don't interrupt workflow
            Console.WriteLine("Metadata read skipped: " & ex.Message)
        End Try

        ' No metadata found or couldn't parse
        Return Nothing
    End Function
    Public Function IsPngDisguisedAsJpg(filePath As String) As Boolean
        Try
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Dim sig(7) As Byte
                fs.Read(sig, 0, sig.Length)
                Return sig(0) = &H89 AndAlso sig(1) = &H50 AndAlso sig(2) = &H4E AndAlso
                   sig(3) = &H47 AndAlso sig(4) = &HD AndAlso sig(5) = &HA AndAlso
                   sig(6) = &H1A AndAlso sig(7) = &HA
            End Using
        Catch
            Return False
        End Try
    End Function
    Public Function ConvertPngToJpegSilent(pngFile As String) As String
        Dim tempJpegFile As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() & ".jpg")
        Dim psi As New ProcessStartInfo("ffmpeg", $"-y -loglevel quiet -i ""{pngFile}"" -f image2 -vcodec mjpeg ""{tempJpegFile}""") With {
        .CreateNoWindow = True,
        .UseShellExecute = False,
        .RedirectStandardOutput = False,
        .RedirectStandardError = False
    }

        Try
            Using proc As New Process()
                proc.StartInfo = psi
                proc.Start()
                proc.WaitForExit()
            End Using

            If File.Exists(tempJpegFile) Then
                Return tempJpegFile
            End If
        Catch
            ' Silent failure – no action
        End Try

        Return Nothing
    End Function
End Module
