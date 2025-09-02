Imports System.Data.SQLite
Imports System.IO
Imports System.Net


Module unindexedfiles
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
    Dim dir As String = GetDefaultDir()

    Public Sub RunUnindexedFileSearchWithSplash()
        Dim splash As New working()
        splash.Location = New Point(500, 800)
        splash.Show()
        Application.DoEvents()

        Task.Run(Sub()
                     Dim result As Integer = DoHeavyWork()

                     splash.Invoke(Sub()
                                       splash.Close()
                                       MessageBox.Show($"Search complete  {result} images found")
                                   End Sub)
                 End Sub)
    End Sub

    Function DoHeavyWork() As Integer
        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim connection As SQLiteConnection = Manager.GetConnection
        If connection.State <> ConnectionState.Open Then
            connection.Open()
        End If

        ConfigureFFMpeg()

        ' 🔍 Preload indexed filenames
        Dim existingFiles As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Using preloadCmd As New SQLiteCommand("SELECT PFileName FROM Pictures", connection)
            Using reader As SQLiteDataReader = preloadCmd.ExecuteReader()
                While reader.Read()
                    existingFiles.Add(reader.GetString(0).Trim())
                End While
            End Using
        End Using

        ' 📂 Loop through files

        For Each imageFile In Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
            Dim ext As String = Path.GetExtension(imageFile).ToLowerInvariant()
            Dim fname As String = imageFile.Replace(dir, "").Trim()

            ' ⛔ Skip processing if already indexed
            If existingFiles.Contains(fname) Then
                    x += 1
                    Continue For
                End If
            If ext = ".jpg" OrElse ext = ".jpeg" Then

                ' ✅ Only run format and orientation fixes for new files
                If IsPngDisguisedAsJpg(imageFile) Then
                    Dim tempJpeg As String = ConvertPngToJpegSilent(imageFile)
                    If Not String.IsNullOrEmpty(tempJpeg) Then
                        File.Delete(imageFile)
                        File.Move(tempJpeg, imageFile)
                        convertedFiles.Add(imageFile)
                        imageFile = tempJpeg
                    End If
                End If

                Try
                    FixImageOrientation(imageFile)
                Catch ex As Exception
                    Debug.WriteLine($"Orientation fix failed for {imageFile}: {ex.Message}")
                End Try
            End If
            '  Process unindexed image
            Dim filename = Path.GetFileName(imageFile)
            Dim ldir = Path.GetDirectoryName(imageFile)
            Dim lpath = ldir.Replace(dir, "").Trim()

                If Not ShouldSkipFile(filename, itype) Then
                    'y += 1
                    ProcessImageFile(fname, imageFile, lpath, itype, x, y, connection)
                End If

        Next

        If convertedFiles.Count > 0 Then
            Dim msg = $"Converted {convertedFiles.Count} disguised PNGs:" & Environment.NewLine &
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
        'If ShouldSkipFile(filename, itype) Then
        '    Return True
        'End If


        'If FileExistsInDatabase(connection, filename) Then
        '    x += 1
        '    Return True
        'End If

        y += 1
        CreateThumbnail(imageFile, itype, connection)
        Return True

    End Function

    Private Function ShouldSkipFile(filename As String, ByRef itype As Integer) As Boolean
        If filename.ToLower().Contains(INDEX_MARKER) Then
            Return True
        End If
        Dim extension As String = Path.GetExtension(filename).ToLower()
        ' Remove the dot if you want

        extension = extension.TrimStart("."c)
        Dim picArray() As String = {"jpg", "png", "jpeg"}
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
    Private Sub CreateThumbnail(imageFile As String, itype As Integer, connection As SQLiteConnection)
        Try
            If itype = 1 Then
                'handles photos
                CreateThumb(imageFile, connection)
            Else
                ' Handle video type 
                createMthumb(imageFile, connection)
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

            filename = WebUtility.HtmlDecode(filename)
            Using command As New SQLiteCommand(SQL_SAVE_UNINDEXED, connection)
                command.Parameters.Add(New SQLiteParameter("@filename", DbType.String) With {.Value = filename})
                command.Parameters.Add(New SQLiteParameter("@path", DbType.String) With {.Value = lPath})
                command.Parameters.Add(New SQLiteParameter("@type", DbType.Int32) With {.Value = itype})
                command.Parameters.Add(New SQLiteParameter("@Status", DbType.String) With {.Value = "N"})
                command.Parameters.Add(New SQLiteParameter("@width", DbType.Int32) With {.Value = width})
                command.Parameters.Add(New SQLiteParameter("@height", DbType.Int32) With {.Value = height})
                command.Parameters.Add(New SQLiteParameter("@time", DbType.Int32) With {.Value = tm})
                Debug.WriteLine($"ThumbnailBytes: {If(thumbnailBytes Is Nothing, "null", thumbnailBytes.Length.ToString())}")

                If thumbnailBytes Is Nothing OrElse thumbnailBytes.Length = 0 Then
                    command.Parameters.Add(New SQLiteParameter("@thumb", DbType.Binary) With {.Value = DBNull.Value})
                Else
                    command.Parameters.Add(New SQLiteParameter("@thumb", DbType.Binary) With {.Value = thumbnailBytes})
                End If

                command.ExecuteNonQuery()
                'File.WriteAllBytes("c:\thumnails\test_image.jpg", thumbnailBytes)
                ' Process.Start("c:\thumnails\test_image.jpg")
                Return True

            End Using




        Catch ex As SQLiteException
            If ex.ResultCode = SQLiteErrorCode.Constraint OrElse
           ex.ResultCode = SQLiteErrorCode.Constraint_PrimaryKey Then
                y -= 1
                Return True
            End If
            MessageBox.Show($"SQLite error:   {ex.Message} (#{ex.ResultCode})")
            Return False

        Catch ex As Exception
            MessageBox.Show($"General error saving '{filename}': {ex.Message}")
            Return False
        End Try
    End Function
    Function CreateThumb(filename As String, connection As SQLiteConnection) As Byte()
        Dim thumbSize As Integer = 150
        Dim bytes As Byte() = Nothing

        Try
            Using original As Image = Image.FromFile(filename)
                ' Calculate scale ratio
                Dim ratio As Double = Math.Min(thumbSize / original.Width, thumbSize / original.Height)
                Dim newWidth As Integer = CInt(original.Width * ratio)
                Dim newHeight As Integer = CInt(original.Height * ratio)

                ' Create 150x150 canvas
                Using canvas As New Bitmap(thumbSize, thumbSize)
                    Using g As Graphics = Graphics.FromImage(canvas)
                        g.Clear(Color.White) ' Optional: background color
                        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

                        ' Center the scaled image
                        Dim xOffset As Integer = (thumbSize - newWidth) \ 2
                        Dim yOffset As Integer = (thumbSize - newHeight) \ 2

                        g.DrawImage(original, xOffset, yOffset, newWidth, newHeight)
                    End Using

                    bytes = ImageToByteArray(canvas, Imaging.ImageFormat.Jpeg)
                    Console.WriteLine("Thumbnail byte length: " & bytes.Length)
                    Dim ldir = Path.GetDirectoryName(filename)
                    Dim lpath = ldir.Replace(dir, "").Trim()

                    ' Save to DB (assuming y, itype are defined elsewhere)
                    SaveToDatabase(connection, filename, lpath, bytes, y, itype, original.Width, original.Height, 0)

                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine("Thumbnail creation failed: " & ex.Message)
        End Try

        Return bytes
    End Function

    Private Sub createMthumb(imagepath As String, connection As SQLiteConnection)
        Dim width As Integer = 240
        Dim height As Integer = 320
        Dim t As Integer = 5
        Dim ldir = Path.GetDirectoryName(imagepath)
        Dim lpath = ldir.Replace(dir, "").Trim()

        ' Corrected function call
        Dim videoData As VideoData = ThumbnailExtractor.GetVideoData(imagepath, width, height, t)
        SaveToDatabase(connection, imagepath, lpath, videoData.ThumbnailBytes, CInt(y), CInt(itype), CInt(videoData.Width), CInt(videoData.Height), CInt(videoData.Duration))

    End Sub
    Private Function FileExistsInDatabase(connection As SQLiteConnection,
                                        filename As String) As Boolean
        Using command As New SQLiteCommand(SQL_CHECK_EXISTS, connection)
            command.Parameters.AddWithValue("@FileName", filename)
            Return Convert.ToInt32(command.ExecuteScalar()) > 0
        End Using
    End Function
End Module

