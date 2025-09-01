Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports AxWMPLib
Imports FamAlbum.VideoThumbnailExtractor
Imports Microsoft.Data.SqlClient
Public Class ImagesNotinDatabase
        Dim Manager As New ConnectionManager(GetConnectionString())
    Dim connection As New SqlConnection
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
        "INSERT INTO UnindexedFiles (uiFileName, uiDirectory, uiThumb,uiType,uiWidth,uiHeight,uiVtime) 
         VALUES (@filename, @path, @thumb,@type,@width,@height,@time)"
    Dim lpath As String
    Dim y As Integer = 0
    Dim filename As String
    Dim fname As String
    Private Sub ImagesNotinDatabase_Load(sender As Object, e As EventArgs) Handles MyBase.Load


        Dim splash As New Working()
        splash.Location = New Point(500, 300) ' X=500, Y=300 — adjust as needed
        splash.Show()

        Task.Run(Sub()
                     Dim x As Integer = 0
                     Dim dir As String = GetDefaultDir()
                     connection = Manager.GetConnection
                     ConfigureFFMpeg()
                     Using connection
                         For Each imageFile In Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                             Dim ext As String = Path.GetExtension(imageFile).ToLowerInvariant()

                             ' Intercept only .jpg/.jpeg
                             If ext = ".jpg" OrElse ext = ".jpeg" Then
                                 If IsPngDisguisedAsJpg(imageFile) Then
                                     Dim tempJpeg As String = ConvertPngToJpegSilent(imageFile)
                                     If Not String.IsNullOrEmpty(tempJpeg) Then
                                         File.Delete(imageFile)
                                         File.Move(tempJpeg, imageFile)
                                         convertedFiles.Add(imageFile) ' Log it
                                     End If
                                 End If

                             End If

                             ' Proceed as usual with cleaned/verified file
                             filename = Path.GetFileName(imageFile)
                             Dim ldir = Path.GetDirectoryName(imageFile)
                             lpath = ldir.Replace(dir, "").Trim()
                             fname = imageFile.Replace(dir, "").Trim()
                             ProcessImageFile(fname, imageFile, lpath, itype, x, y)
                         Next
                     End Using
                     If convertedFiles.Count > 0 Then
                         Dim msg = $"Converted {convertedFiles.Count} PNG disguised as JPG:" & Environment.NewLine &
              String.Join(Environment.NewLine, convertedFiles.Take(10)) ' Show up to 10 as preview
                         MessageBox.Show(msg, "Format Fix Report")
                     End If

                     Me.Close()
                 End Sub)
        splash.Close()
        MessageBox.Show("Search complete  " & y & " images found")

    End Sub
    Public Function ProcessImageFile(filename As String,
                                   imageFile As String,
                                   lPath As String,
                                   ByRef itype As Integer,
                                   ByRef x As Integer,
                                   ByRef y As Integer) As Boolean
        If ShouldSkipFile(filename, itype) Then
            Return True
        End If
        connection = Manager.GetConnection

        Using connection

            If FileExistsInDatabase(connection, filename) Then
                x += 1
                Return True
            End If

            y += 1
            CreateThumbnail(imageFile, itype)
            Return True
        End Using

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

    Private Function FileExistsInDatabase(connection As SqlConnection,
                                        filename As String) As Boolean
        Using command As New SqlCommand(SQL_CHECK_EXISTS, connection)
            command.Parameters.AddWithValue("@FileName", filename)
            Return Convert.ToInt32(command.ExecuteScalar()) > 0
        End Using
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


    Private Function SaveToDatabase(connection As SqlConnection, filename As String, lPath As String, thumbnailBytes As Byte(), ByRef y As Integer, ByRef itype As Integer, width As Integer, height As Integer, tm As Integer) As Boolean
        Try
            ' Optional: guard against overly long strings
            If filename.Length > 200 OrElse lPath.Length > 200 Then
                MessageBox.Show("Filename or path exceeds 200 characters.")
                Return False
            End If

            Using command As New SqlCommand(SQL_SAVE_UNINDEXED, connection)
                command.Parameters.Add("@filename", SqlDbType.NVarChar, 200).Value = filename
                command.Parameters.Add("@path", SqlDbType.NVarChar, 200).Value = lPath
                command.Parameters.Add("@type", SqlDbType.Int).Value = itype
                command.Parameters.Add("@width", SqlDbType.Int).Value = width
                command.Parameters.Add("@height", SqlDbType.Int).Value = height
                command.Parameters.Add("@time", SqlDbType.Int).Value = tm

                ' Safely handle the thumbnail (may be null or empty)
                If thumbnailBytes Is Nothing OrElse thumbnailBytes.Length = 0 Then
                    command.Parameters.Add("@thumb", SqlDbType.VarBinary).Value = DBNull.Value
                Else
                    command.Parameters.Add("@thumb", SqlDbType.VarBinary, thumbnailBytes.Length).Value = thumbnailBytes
                End If

                command.ExecuteNonQuery()
                Return True
            End Using

        Catch ex As SqlException
            If ex.Number = 2627 Then ' Duplicate key
                y -= 1
                Return True
            End If
            MessageBox.Show($"SQL error: {ex.Message} (#{ex.Number})")
            Return False

        Catch ex As Exception
            MessageBox.Show($"General error saving '{filename}': {ex.Message}")
            Return False
        End Try
    End Function



    Private Sub CreateThumb(filename As String)
        ' Debug hook
        If filename.Contains("thumbnail") Or filename.Contains("Events\MattProposal\IMG_0231.mov") Then
            Dim j = 1 ' breakpoint here
        End If
        Dim imgWidth As Integer = 0
        Dim imgHeight As Integer = 0
        Dim originalImage As Image = Nothing
        Dim thumbnail As Image = Nothing
        Dim bytes As Byte() = Nothing

        FixImageOrientation(filename)

        Try
            originalImage = Image.FromFile(filename)
            ' Calculate dimensions
            imgWidth = originalImage.Width
            imgHeight = originalImage.Height
            Dim ratio As Double = originalImage.Width / CDbl(originalImage.Height)
            Dim W As Integer
            Dim H As Integer
            If ratio > 1.0 Then
                W = 200
                H = CInt(W / ratio)
            Else
                H = 200
                W = CInt(H * ratio)
            End If

            ' Generate the thumbnail
            thumbnail = originalImage.GetThumbnailImage(W, H, Nothing, IntPtr.Zero)
            If thumbnail Is Nothing Then
                MessageBox.Show($"Failed to create thumbnail for: {filename}")
                Exit Sub
            End If

            ' Convert to byte array
            bytes = ImageToByteArray(thumbnail, Imaging.ImageFormat.Jpeg)
            If bytes Is Nothing OrElse bytes.Length = 0 Then
                MessageBox.Show($"Thumbnail byte array is empty for: {filename}")
                Exit Sub
            End If

        Catch ex As Exception
            MessageBox.Show($"Error processing {filename}:{Environment.NewLine}{ex.Message}")
            Exit Sub
        Finally
            originalImage?.Dispose()
            thumbnail?.Dispose()
        End Try

        ' Save to DB only if everything checks out
        If bytes IsNot Nothing AndAlso bytes.Length > 0 Then
            connection = Manager.GetConnection()
            If Not SaveToDatabase(connection, fname, lpath, bytes, y, itype, CInt(imgWidth), CInt(imgHeight), 0) Then
                MessageBox.Show("Failed to save thumbnail to database.")
            End If
        End If
    End Sub




    Private Sub createMthumb(imagepath As String)
        Dim width As Integer = 240
        Dim height As Integer = 320
        Dim t As Integer = 5

        ' Corrected function call
        Dim videoData As VideoData = GetVideoData(imagepath, width, height, t)

        Console.WriteLine("Width: " & videoData.Width)
        Console.WriteLine("Height: " & videoData.Height)
        Console.WriteLine("Duration: " & videoData.Duration)

        SaveToDatabase(connection, fname, lpath, videoData.ThumbnailBytes, y, itype, videoData.Width, videoData.Height, videoData.Duration)

    End Sub



End Class