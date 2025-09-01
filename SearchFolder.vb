Imports System.Data.SQLite
Imports System.IO


Module SearchFolder
    Dim Manager As New ConnectionManager(GetConnectionString())
    Dim connection As New SQLiteConnection
    Dim thumbnailBytes As Byte()
    Dim itype As Integer
    Private ReadOnly _connectionString As String
    Dim convertedFiles As New List(Of String)

    ' Constants
    Private Const INDEX_MARKER As String = "_index"

    ' SQL Queries
    Private Const SQL_CHECK_EXISTS As String =
        "SELECT COUNT(*) FROM Pictures WHERE PFileName = @FileName"
    Private Const SQL_SAVE_UNINDEXED As String =
        "INSERT INTO UnindexedFiles (uiFileName, uiDirectory, uiThumb,uiType,uiWidth,uiHeight,uiVtime,uiStatus) 
         VALUES (@filename, @path, @thumb,@type,@width,@height,@time,@Status)"
    Dim lpath As String
    Dim y As Integer = 0
    Dim filename As String
    Dim fname As String
    Dim Ddir As String = GetDefaultDir()



    Public Sub RunUnindexedFolderSearchWithSplash()
        Dim splash As New working()
        splash.Location = New Point(500, 800)
        splash.Show()
        Application.DoEvents()

        ' 👉 Move folder selection before launching background task
        Dim selectedFolder As String = ""
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Select a folder within the Family Album"
            fbd.SelectedPath = Ddir
            fbd.RootFolder = Environment.SpecialFolder.MyComputer

            If fbd.ShowDialog() = DialogResult.OK Then
                selectedFolder = fbd.SelectedPath
                If Not selectedFolder.StartsWith(Ddir) Then
                    MessageBox.Show("Please select a folder within the main directory!", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    splash.Close()
                    Return
                End If
            Else
                splash.Close()
                Return
            End If
        End Using

        ' ✅ Pass selected folder into background task
        Task.Run(Sub()
                     Dim result As Integer = DoHeavyWork(selectedFolder)

                     splash.Invoke(Sub()
                                       splash.Close()
                                       MessageBox.Show($"Search complete. {result} images found.")
                                   End Sub)
                 End Sub)
    End Sub
    Function DoHeavyWork(selectedFolder As String) As Integer
        Dim x As Integer = 0
        connection = Manager.GetConnection
        ConfigureFFMpeg()

        Using connection
            For Each imageFile In Directory.EnumerateFiles(selectedFolder, "*.*", SearchOption.AllDirectories)
                Dim ext As String = Path.GetExtension(imageFile).ToLowerInvariant()

                ' Intercept only .jpg/.jpeg
                If ext = ".jpg" OrElse ext = ".jpeg" Then
                    ' 🧪 Handle disguised PNGs first
                    If IsPngDisguisedAsJpg(imageFile) Then
                        Dim tempJpeg As String = ConvertPngToJpegSilent(imageFile)
                        If Not String.IsNullOrEmpty(tempJpeg) Then
                            File.Delete(imageFile)
                            File.Move(tempJpeg, imageFile)
                            convertedFiles.Add(imageFile)
                        End If
                    End If

                    ' 🔄 Fix orientation before thumbnail generation
                    Try
                        FixImageOrientation(imageFile)
                    Catch ex As Exception
                        Debug.WriteLine("Orientation fix failed for " & imageFile & ": " & ex.Message)
                    End Try
                End If

                ' ✅ Only process if not already in the database
                fname = imageFile.Replace(Ddir, "").Trim()
                If Not FileExistsInDatabase(connection, fname) Then
                    filename = Path.GetFileName(imageFile)
                    Dim ldir = Path.GetDirectoryName(imageFile)
                    lpath = ldir.Replace(Ddir, "").Trim()
                    ProcessImageFile(fname, imageFile, lpath, itype, x, y, connection)
                End If
            Next
        End Using

        If convertedFiles.Count > 0 Then
            Dim msg = $"Converted {convertedFiles.Count} PNG disguised as JPG:" & Environment.NewLine &
                  String.Join(Environment.NewLine, convertedFiles.Take(10))
            MessageBox.Show(msg, "Format Fix Report")
        End If

        Return y
    End Function

    Public Function ProcessImageFile(filename As String,
                                 imageFile As String,
                                 lPath As String,
                                 ByRef itype As Integer,
                                 ByRef x As Integer,
                                 ByRef y As Integer,
                                 connection As SQLiteConnection) As Boolean


        If ShouldSkipFile(filename, itype) Then
            Return True
        End If


        If FileExistsInDatabase(connection, filename) Then
                x += 1
                Return True
            End If

            y += 1
            CreateThumbnail(imageFile, itype)
            Return True

    End Function

    Private Function ShouldSkipFile(filename As String, ByRef itype As Integer) As Boolean
        If filename.ToLower().Contains(INDEX_MARKER) Then
            Return True
        End If
        Dim extension As String = Path.GetExtension(filename).ToLower()
        ' Remove the dot if you want

        extension = extension.TrimStart("."c)
        Dim picArray() As String = {"jpg", "png"}
        Dim mArray() As String = {"mov", "mp4", "avi"}
        If picArray.Contains(extension) Then
            itype = 1
            Return False
        ElseIf mArray.Contains(extension) Then
            itype = 2
            Return False
        Else
            Return True
        End If

    End Function
    Private Sub CreateThumbnail(imageFile As String, itype As Integer)
        Try
            If itype = 1 Then
                'handles photos
                CreateThumb(imageFile)
            Else
                ' Handle video type 
                createMthumb(imageFile)
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error creating thumbnail: {ex.Message}")

        End Try
    End Sub
    Private Function SaveToDatabase(connection As SQLiteConnection,
                                filename As String,
                                lPath As String,
                                thumbnailBytes As Byte(),
                                ByRef y As Integer,
                                ByRef itype As Integer,
                                width As Integer,
                                height As Integer,
                                tm As Integer) As Boolean
        Try
            ' Guard against overly long strings
            If filename.Length > 200 OrElse lPath.Length > 200 Then
                MessageBox.Show("Filename or path exceeds 200 characters.")
                Return False
            End If

            Using command As New SQLiteCommand(SQL_SAVE_UNINDEXED, connection)
                command.Parameters.AddWithValue("@filename", filename)
                command.Parameters.AddWithValue("@path", lPath)
                command.Parameters.AddWithValue("@type", itype)
                command.Parameters.AddWithValue("@Status", "N")
                command.Parameters.AddWithValue("@width", width)
                command.Parameters.AddWithValue("@height", height)
                command.Parameters.AddWithValue("@time", tm)

                ' Handle thumbnail safely
                If thumbnailBytes Is Nothing OrElse thumbnailBytes.Length = 0 Then
                    command.Parameters.AddWithValue("@thumb", DBNull.Value)
                Else
                    command.Parameters.AddWithValue("@thumb", thumbnailBytes)
                End If

                command.ExecuteNonQuery()
                Return True
            End Using

        Catch ex As SQLiteException
            If ex.ResultCode = SQLiteErrorCode.Constraint OrElse
           ex.ResultCode = SQLiteErrorCode.Constraint_PrimaryKey Then
                y -= 1
                Return True
            End If
            MessageBox.Show($"SQLite error: {ex.Message} (#{ex.ResultCode})")
            Return False

        Catch ex As Exception
            MessageBox.Show($"General error saving '{filename}': {ex.Message}")
            Return False
        End Try
    End Function
    Function CreateThumb(filename As String) As Byte()
        Dim bytes As Byte() = Nothing
        Dim original As Image = Image.FromFile(filename)
        Dim thumbSize As Integer = 150

        ' Calculate scale while preserving aspect ratio
        Dim ratio As Double = Math.Min(thumbSize / original.Width, thumbSize / original.Height)
        Dim newWidth As Integer = CInt(original.Width * ratio)
        Dim newHeight As Integer = CInt(original.Height * ratio)

        ' Center the image on a 200x200 canvas
        Dim thumb As New Bitmap(thumbSize, thumbSize)
        Using g As Graphics = Graphics.FromImage(thumb)
            g.Clear(Color.White) ' Change to Color.White or any background you want
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Dim x = (thumbSize - newWidth) \ 2
            Dim y = (thumbSize - newHeight) \ 2
            g.DrawImage(original, x, y, newWidth, newHeight)
        End Using
        bytes = ImageToByteArray(thumb, Imaging.ImageFormat.Jpeg)

        SaveToDatabase(connection, filename, lpath, bytes, y, itype, original.Width, original.Height, 0)

        original?.Dispose()
        thumb?.Dispose()

        Return bytes
    End Function
    Private Sub createMthumb(imagepath As String)
        Dim width As Integer = 240
        Dim height As Integer = 320
        Dim t As Integer = 5

        ' Corrected function call
        Dim videoData As VideoData = ThumbnailExtractor.GetVideoData(imagepath, width, height, t)

        Console.WriteLine("Width: " & videoData.Width)
        Console.WriteLine("Height: " & videoData.Height)
        Console.WriteLine("Duration: " & videoData.Duration)

        SaveToDatabase(connection, imagepath, lpath, videoData.ThumbnailBytes, y, itype, videoData.Width, videoData.Height, videoData.Duration)

    End Sub
    Private Function FileExistsInDatabase(connection As SQLiteConnection,
                                        filename As String) As Boolean
        Using command As New SQLiteCommand(SQL_CHECK_EXISTS, connection)
            command.Parameters.AddWithValue("@FileName", filename)
            Return Convert.ToInt32(command.ExecuteScalar()) > 0
        End Using
    End Function
End Module


