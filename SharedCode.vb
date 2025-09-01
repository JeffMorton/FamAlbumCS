Imports System.Data.SqlClient
Imports System.Data.SQLite
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports FFMpegCore
Imports Microsoft.Win32
Module SharedCode
    Dim Manager As New ConnectionManager("Data Source=C:\Family Album\FamilyAlbum.db;Version=3;")
    Dim connection As New SQLiteConnection

    Public Function GetDefaultDir() As String
        ' Open the registry key
        Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\FamilyAlbum")
        If key IsNot Nothing Then
            Dim value As String = key.GetValue("DefaultDir", "Default Value").ToString()
            key.Close()
            Return value

            MessageBox.Show("Registry Value: " & value)
        Else
            Dim GetDir As New GetDefaultFile()
            GetDir.Show()
            Return Nothing
        End If
    End Function
    Public Function ModifyPeopleList(oldList As String, Pos As Integer, D As Integer, name As String) As String
        ' Normalize the input list
        If oldList = "1" Then oldList = ""

        ' Split and sanitize the list
        Dim items As List(Of String) = oldList.
            Split(","c).
            Select(Function(s) s.Trim()).
            Where(Function(s) Not String.IsNullOrWhiteSpace(s)).
            ToList()

        Dim cleanName As String = name.Trim()

        If D > 0 Then
            ' Add name at specified position
            If Not String.IsNullOrWhiteSpace(cleanName) Then
                If Pos >= 0 AndAlso Pos <= items.Count Then
                    items.Insert(Pos, cleanName)
                Else
                    items.Add(cleanName)
                End If
            End If
        Else
            ' Remove matching name
            items = items.
                Where(Function(s) Not s.Equals(cleanName, StringComparison.OrdinalIgnoreCase)).
                ToList()
        End If

        ' Return the cleaned, comma-delimited string
        Return String.Join(",", items)
    End Function



    Public Function FillNames() As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("Names", GetType(String))
        dt.Columns.Add("ID", GetType(String))
        dt.Clear()
        Dim qryNameCnt As String = "Select ID,neName from NameEvent where neType ='N' order by neName"
        Try
            connection = Manager.GetConnection()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        Dim command As New SQLiteCommand(qryNameCnt, connection)
        Try
            Dim reader As SQLiteDataReader = command.ExecuteReader()
            While reader.Read()
                dt.Rows.Add(reader("neName"), reader("ID").ToString)
            End While
            reader.Close()
        Catch ex As SQLiteException
            MessageBox.Show("Database error: " & ex.Message)
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
        Return dt
    End Function
    Public Function FillEvents() As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("Event", GetType(String))
        dt.Columns.Add("ID", GetType(String))
        dt.Clear()
        Dim qryNameCnt As String = "Select ID,neName from NameEvent where neType ='E' order by neName"

        connection = Manager.GetConnection()

        Dim command As New SQLiteCommand(qryNameCnt, connection)
        Try
            Dim reader As SQLiteDataReader = command.ExecuteReader()
            While reader.Read()
                dt.Rows.Add(reader("neName"), reader("ID").ToString)
            End While
            reader.Close()
        Catch ex As SQLiteException
            MessageBox.Show("Database error: " & ex.Message)
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
        Return dt
    End Function
    Public Function GetPPeopleList(FileName As String, ByRef Namecount As Integer) As String
        connection = Manager.GetConnection
        Dim NL As String = ""
        Dim command As New SQLiteCommand("SELECT  PPeoplelist,PNameCount FROM Pictures WHERE Pfilename = @name", connection)
        command.Parameters.AddWithValue("@name", FileName)
        Using reader As SQLiteDataReader = command.ExecuteReader()
            While reader.Read()
                NL = CStr(reader("PPeopleList"))
                Namecount += CInt(reader("PNameCount"))
            End While
        End Using
        connection.Close()
        Return NL
    End Function
    Public Sub SavePPeopleList(namelt As String, selectedPerson As String, filename As String, Namecount As Integer)
        connection = Manager.GetConnection
        Using connection

            ' Begin a transaction
            Dim transaction As SQLiteTransaction = connection.BeginTransaction()

            Try
                ' Create a command and associate it with the transaction
                Dim command As New SQLiteCommand()
                command.Connection = connection
                command.Transaction = transaction

                ' Update Statement 1

                Try
                    Dim currentDate As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    command.CommandText = "Update Pictures set PPeoplelist = @namelist, PlastModifiedDate = @currentdate,PNameCount = @namecount WHERE PfileName = @filename"
                    command.Parameters.AddWithValue("@namelist", namelt)
                    command.Parameters.AddWithValue("@filename", filename)
                    command.Parameters.AddWithValue("@namecount", Namecount)
                    command.Parameters.AddWithValue("@currentdate", currentDate)
                    Dim rowsAffected As Integer = command.ExecuteNonQuery()

                    If Not Reexists(selectedPerson, filename) Then
                        ' Update Statement 2
                        Dim command2 As New SQLiteCommand("Insert into NamePhoto (npID,npFilename) values (@selectedPerson,@filename1)", connection)
                        command2.Connection = connection
                        command2.Transaction = transaction
                        command2.Parameters.AddWithValue("@filename1", filename)
                        command2.Parameters.AddWithValue("@selectedPerson", selectedPerson)
                        command2.ExecuteNonQuery()

                    End If

                    ' Commit the transaction if all updates succeed
                    transaction.Commit()
                Catch ex As Exception
                    ' Rollback the transaction if an error occurs
                    transaction.Rollback()
                    MessageBox.Show("Transaction rolled back. Error: " & ex.Message)
                End Try
            Catch EX As Exception
                MessageBox.Show(EX.Message)
            End Try
        End Using
    End Sub
    Public Function Reexists(ID As String, filename As String) As Boolean
        Dim query As String = "SELECT EXISTS(SELECT 1 FROM NamePhoto WHERE npID=@ID AND npFileName=@filename)"

        Using connection As New SQLiteConnection(GetConnectionString())
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@ID", ID)
                command.Parameters.AddWithValue("@filename", filename)
                connection.Open()
                Dim result As Integer = Convert.ToInt32(command.ExecuteScalar())
                Return result = 1
            End Using
        End Using
    End Function
    Public Function DeleteAPerson(name As String, filename As String, Namecount As Integer) As String

        connection = Manager.GetConnection()
        Dim plist As String = GetPPeopleList(filename, Namecount)
        Dim newplist As String = ModifyPeopleList(plist, 0, -1, name)

        Try
            connection = Manager.GetConnection()
            Using connection

                ' Begin a transaction
                Dim transaction As SQLiteTransaction = connection.BeginTransaction()

                Try
                    ' Create a command and associate it with the transaction
                    Dim command As New SQLiteCommand()
                    command.Connection = connection
                    command.Transaction = transaction

                    ' Update Statement 1
                    command.CommandText = "UPDATE Pictures SET PPeopleList = @plst WHERE PfileName = @filename"
                    command.Parameters.AddWithValue("@filename", filename)
                    command.Parameters.AddWithValue("@plst", newplist)

                    command.ExecuteNonQuery()

                    ' Update Statement 2
                    command.CommandText = "Delete from NamePhoto where npID = @name and npfilename=@filename"
                    command.Parameters.AddWithValue("@name", name)
                    command.ExecuteNonQuery()

                    ' Commit the transaction if all updates succeed
                    transaction.Commit()
                    Console.WriteLine("Transaction committed successfully.")
                Catch ex As Exception
                    ' Rollback the transaction if an error occurs
                    transaction.Rollback()
                    Console.WriteLine("Delete Name Transaction rolled back. Error: " & ex.Message)
                End Try
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        Return newplist
    End Function
    Public Function AddNametoPeopleList(Name As String, pos As Integer, filename As String) As List(Of String)
        Dim updatednamelist As String
        Dim Namecount As Integer
        ' Get the current list of names
        Dim namelist As String = GetPPeopleList(filename, Namecount)
        If namelist = "1" Then namelist = ""
        Dim items As List(Of String) = namelist.Split(","c).ToList()

        ' Insert the new name at the specified position
        If pos >= 0 And pos <= items.Count Then
            items.Insert(pos, Name.ToString())
        Else
            namelist = namelist & "," & Name
        End If
        Namecount = items.Count
        ' Convert the list back to an integer array

        ' Update the name list
        Try
            updatednamelist = String.Join(",", items)
        Catch ex As Exception
            ' Handle any exception and log it if necessary
            updatednamelist = String.Join(",", items) & "," & Name
        End Try

        SavePPeopleList(updatednamelist, Name, filename, Namecount)
        Return items
    End Function
    Public Function AddNametoNamelist(Name As String, pos As Integer, items As List(Of String)) As List(Of String)
        ' Sanitize the incoming name
        Dim cleanName As String = Name.Trim()

        ' Only add if it's not blank
        If Not String.IsNullOrWhiteSpace(cleanName) Then
            If items.Count = 0 Then
                items.Add(cleanName)
            ElseIf pos >= 0 AndAlso pos <= items.Count Then
                items.Insert(pos, cleanName)
            Else
                items.Add(cleanName)
            End If
        End If

        ' Clean the entire list: trim and remove blanks
        Dim cleanedList As List(Of String) = items.
        Select(Function(s) s.Trim()).
        Where(Function(s) Not String.IsNullOrWhiteSpace(s)).
        ToList()

        Return cleanedList
    End Function

    Public Function GetConnectionString() As String
        Dim DDir As String = GetDefaultDir()
        Return "Data Source=" & DDir & "FamilyAlbum.db;Version=3;"
    End Function
    Public Function CMovie(Name As String) As Boolean
        Dim movieExtensions As String() = {".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv"}

        ' Get the file extension
        Dim fileExtension As String = Path.GetExtension(Name).ToLower()

        If movieExtensions.Contains(fileExtension) Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function GetMemberID(FullName As String, Relation As String) As String
        Dim query As String = "select ID from NameEvent where neName = @name"
        Using connection As New SQLiteConnection(GetConnectionString)
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@name", FullName)
                connection.Open()
                Dim result As Object = command.ExecuteScalar()

                If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
                    Return CStr(result)
                Else
                    Return AddNewName(FullName, Relation)
                End If
            End Using
        End Using
    End Function
    Public Function AddNewName(FullName As String, Relation As String) As String
        Dim qryInsertName As String = "INSERT INTO NameEvent(neName, neRelation, neType, neDatelastModified) " &
                              "VALUES (@name, @relation, 'N', @currentdate)  returning ID;"
        Dim newKey As Integer
        connection = Manager.GetConnection()
        Using connection
            Dim currentDate As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            Try
                ' First command: insert into NameEvent
                Dim command As New SQLiteCommand(qryInsertName, connection)
                command.Parameters.AddWithValue("@name", FullName)
                command.Parameters.AddWithValue("@relation", Relation)
                command.Parameters.AddWithValue("@currentdate", currentDate)
                newKey = CInt(command.ExecuteScalar())
                Return CStr(newKey)
            Catch EX As SQLiteException
                MessageBox.Show("Add Name Failed " & EX.Message)
                Return ""
            End Try
        End Using
    End Function
    Public Function GetPhotoMonthAndYear(ByVal imagePath As String) As Tuple(Of Integer, Integer)
        Try
            ' Step 1: Try EXIF DateTaken
            Using image As Image = Image.FromFile(imagePath)
                If image.PropertyIdList.Contains(&H9003) Then
                    Dim prop As PropertyItem = image.GetPropertyItem(&H9003)
                    Dim dateTaken As String = System.Text.Encoding.ASCII.GetString(prop.Value).Trim(Chr(0))
                    Dim dateTime As DateTime = DateTime.ParseExact(dateTaken.Substring(0, 19), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture)
                    Return New Tuple(Of Integer, Integer)(dateTime.Month, dateTime.Year)
                End If
            End Using

            ' Step 2: Fall back to Last Modified Date
            If File.Exists(imagePath) Then
                Dim fileInfo As New FileInfo(imagePath)
                Dim lastModified As DateTime = fileInfo.LastWriteTime
                Return New Tuple(Of Integer, Integer)(lastModified.Month, lastModified.Year)
            End If

            ' Step 3: Fall back to File Creation Date
            If File.Exists(imagePath) Then
                Dim fileInfo As New FileInfo(imagePath)
                Dim fileDate As DateTime = fileInfo.CreationTime
                Return New Tuple(Of Integer, Integer)(fileDate.Month, fileDate.Year)
            End If
        Catch ex As Exception
            Debug.WriteLine("Metadata fallback failed: " & ex.Message)
        End Try

        ' Step 4: Graceful fallback
        Return New Tuple(Of Integer, Integer)(0, 0)
    End Function
    Public Function GeteventID(FullName As String, relation As String) As String
        Dim query As String = "SELECT ID FROM NameEvent WHERE neName = @name"

        Using connection As New SQLiteConnection(GetConnectionString())
            Using command As New SQLiteCommand(query, connection)
                command.Parameters.AddWithValue("@name", FullName)
                connection.Open()
                Dim result As Object = command.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    Return result.ToString()
                Else
                    Return SaveNewEvent(FullName, relation)
                End If
            End Using
        End Using
    End Function
    Public Function GetEvent(EventId As String) As (EventName As String, Eventdetail As String)
        Dim query As String = "select neName,neRelation from NameEvent where ID=@ID"
        connection = Manager.GetConnection()
        Dim EVName As String = ""
        Dim EVDetail As String = ""
        Using connection
            Dim Command As New SQLiteCommand(query, connection)
            Command.Parameters.AddWithValue("@ID", EventId)
            Dim reader As SQLiteDataReader = Command.ExecuteReader()

            While reader.Read()
                EVName = reader("neName").ToString
                If Trim(reader("neRelation").ToString) = "NULL" Then
                    EVDetail = ""
                Else
                    EVDetail = reader("neRelation").ToString
                End If
            End While
            Return (EVName, EVDetail)
        End Using
    End Function
    Public Function SaveNewEvent(Evnt As String, EventDetails As String) As String
        Dim EventID As String
        connection = Manager.GetConnection()
        Using connection
            Dim d As Date = Today
            Try
                Dim command As New SQLiteCommand()
                command.Connection = connection

                command.CommandText = "Insert into NameEvent (neName, neRelation, neDateLastModified, neCount, neType) 
                        values (@Event, @EventDetails, @date, 0, 'E') RETURNING ID"
                command.Parameters.AddWithValue("@Event", Evnt)
                command.Parameters.AddWithValue("@EventDetails", EventDetails)
                command.Parameters.AddWithValue("@date", d)
                EventID = CStr(command.ExecuteScalar())
                Return CStr(EventID)

            Catch ex As Exception

                MessageBox.Show("Event not saved.  Err: " & ex.Message)
                Return ""
            End Try
        End Using

    End Function
    Public Function ImageToByteArray(img As Image, format As Imaging.ImageFormat) As Byte()
        Using ms As New MemoryStream()
            img.Save(ms, format)
            Return ms.ToArray()
        End Using
    End Function
    Public Sub FixImageOrientation(ByVal imagePath As String)
        Dim imgData As Byte() = File.ReadAllBytes(imagePath)
        Using ms As New MemoryStream(imgData)
            Using img As Bitmap = CType(Image.FromStream(ms), Bitmap)
                Dim changed As Boolean = False

                For Each p As PropertyItem In img.PropertyItems
                    If p.Id = 274 Then ' EXIF Orientation Tag
                        Dim orientation As Short = BitConverter.ToInt16(p.Value, 0)
                        Select Case orientation
                            Case 3
                                img.RotateFlip(RotateFlipType.Rotate180FlipNone)
                                changed = True
                            Case 6
                                img.RotateFlip(RotateFlipType.Rotate90FlipNone)
                                changed = True
                            Case 8
                                img.RotateFlip(RotateFlipType.Rotate270FlipNone)
                                changed = True
                        End Select
                        Exit For
                    End If
                Next

                If changed Then
                    Dim tempPath As String = Path.GetTempFileName()
                    img.Save(tempPath)
                    File.Copy(tempPath, imagePath, True)
                    File.Delete(tempPath)
                End If
            End Using
        End Using
    End Sub

    Public Sub ConfigureFFMpeg()
        GlobalFFOptions.Configure(New FFOptions() With {
        .BinaryFolder = AppDomain.CurrentDomain.BaseDirectory,   ' This should be your output folder.
        .TemporaryFilesFolder = Path.GetTempPath()
    })
    End Sub
    Public Function GetPendingImageCount() As Integer
        Dim count As Integer = 0
        Dim sql As String = "SELECT COUNT(*) FROM UnindexedFiles WHERE uiStatus = 'N'" ' Example condition
        connection = Manager.GetConnection()
        Using cmd As New SQLiteCommand(sql, connection)
            Try
                Dim result = cmd.ExecuteScalar()
                If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                    count = Convert.ToInt32(result)
                Else
                    count = 0
                End If
            Catch ex As Exception
                MessageBox.Show("Error counting pending images: " & ex.Message)
            End Try
        End Using

        Return count
    End Function
    Public Sub VerifyPictureFilesExist(rootFolder As String)
        Dim wk As New working
        wk.Show()
        Application.DoEvents()
        connection = Manager.GetConnection()
        Dim missingFiles As New List(Of String)
        Dim query As String = "SELECT pfilename FROM pictures"

        Using command As New SQLiteCommand(query, connection)
            Using reader As SQLiteDataReader = command.ExecuteReader()
                While reader.Read()
                    Dim relativePath As String = reader("pfilename").ToString()
                    Dim fullPath As String = rootFolder & relativePath

                    If Not File.Exists(fullPath) Then
                        missingFiles.Add(fullPath)
                    End If
                End While
            End Using
            UpdateNameCountsManually(connection)
        End Using
        working.Close()
        If missingFiles.Count > 0 Then
            MessageBox.Show($"Missing {missingFiles.Count} files. Example:{Environment.NewLine}{String.Join(Environment.NewLine, missingFiles.Take(10))}", "Missing Files")
            ' Optionally log to file:
            File.WriteAllLines(rootFolder & "missing_files.log", missingFiles)
        Else
            MessageBox.Show("All files are present!", "Verification Complete")
        End If
    End Sub
    Sub UpdateNameCountsManually(connection As SQLiteConnection)
        Dim selectQuery As String = "SELECT Pfilename, PPeopleList FROM pictures"
        Dim updateQuery As String = "UPDATE pictures SET PNameCount = @count WHERE Pfilename = @id"

        Using cmdSelect As New SQLiteCommand(selectQuery, connection),
          reader As SQLiteDataReader = cmdSelect.ExecuteReader()

            While reader.Read()
                Dim id As String = (reader("Pfilename").ToString)
                Dim rawList As String = Convert.ToString(reader("PPeopleList"))
                Dim names = rawList.Split(","c).
                Select(Function(n) n.Trim()).
                Where(Function(n) Not String.IsNullOrEmpty(n)).
                Distinct().
                ToList()

                Using cmdUpdate As New SQLiteCommand(updateQuery, connection)
                    cmdUpdate.Parameters.AddWithValue("@count", names.Count)
                    cmdUpdate.Parameters.AddWithValue("@id", id)
                    cmdUpdate.ExecuteNonQuery()
                End Using
            End While
        End Using
    End Sub
    Public Sub Updatethumb(filename As String)
        Dim bytes As Byte() = Nothing
        Dim original As Image = Image.FromFile(filename)

        Dim orientationId As Integer = &H112
        If original.PropertyIdList.Contains(orientationId) Then
            Dim prop = original.GetPropertyItem(orientationId)
            Dim orientation = BitConverter.ToUInt16(prop.Value, 0)
            Dim rotateFlipType As RotateFlipType = RotateFlipType.RotateNoneFlipNone

            Select Case orientation
                Case 3 : rotateFlipType = RotateFlipType.Rotate180FlipNone
                Case 6 : rotateFlipType = RotateFlipType.Rotate90FlipNone
                Case 8 : rotateFlipType = RotateFlipType.Rotate270FlipNone
            End Select

            original.RotateFlip(rotateFlipType)
            original.RemovePropertyItem(orientationId)
        End If

        Dim thumbSize As Integer = 150
        Dim DDir As String = GetDefaultDir()
        ' Calculate scale while preserving aspect ratio
        Dim ratio As Double = Math.Min(thumbSize / original.Width, thumbSize / original.Height)
        Dim newWidth As Integer = CInt(original.Width * ratio)
        Dim newHeight As Integer = CInt(original.Height * ratio)
        Dim sfilename = filename.Replace(DDir, "")
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
        Using connection As New SQLiteConnection()
            Try
                Dim command As New SQLiteCommand("Update pictures set Pthumb=@thumb where Pfilename =@filename", connection)
                command.Parameters.AddWithValue("@thumb", bytes)
                command.Parameters.AddWithValue("@filename", sfilename)
            Catch ex As Exception
                MessageBox.Show("Error updating thumb -" & ex.Message)
            End Try
        End Using
        original?.Dispose()
        thumb?.Dispose()


    End Sub
    Public Sub ShowTextInPictureBox(picBox As PictureBox, message As String)
        Dim width As Integer = picBox.Width
        Dim height As Integer = picBox.Height

        ' Create a blank bitmap
        Dim bmp As New Bitmap(width, height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.White) ' Background color

            ' Set up font and brush
            Dim font As New Font("Arial", 14, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.Black)

            ' Center the text
            Dim textSize As SizeF = g.MeasureString(message, font)
            Dim x As Single = (width - textSize.Width) / 2
            Dim y As Single = (height - textSize.Height) / 2

            ' Draw the text
            g.DrawString(message, font, brush, x, y)
        End Using

        ' Assign the image to the PictureBox
        picBox.Image = bmp
    End Sub
    Public Sub CleanPpeoplelistAndUpdateCount(connection As SQLiteConnection)
        Try
            Dim cmdSelect As New SQLiteCommand("SELECT rowid, Ppeoplelist FROM pictures", connection)
            Using reader As SQLiteDataReader = cmdSelect.ExecuteReader()
                While reader.Read()
                    Dim rowId As Integer = reader.GetInt32(0)
                    Dim rawList As String = reader.GetString(1)

                    ' Clean and split the list
                    Dim cleanedItems As List(Of String) = rawList.Split(","c).
                    Select(Function(s) s.Trim()).
                    Where(Function(s) Not String.IsNullOrWhiteSpace(s)).
                    Distinct().
                    ToList()

                    ' Rejoin into cleaned string
                    Dim cleanedList As String = String.Join(",", cleanedItems)
                    Dim nameCount As Integer = cleanedItems.Count

                    ' Update both fields
                    Dim cmdUpdate As New SQLiteCommand("UPDATE pictures SET Ppeoplelist = @cleaned, PNamecount = @count WHERE rowid = @id", connection)
                    cmdUpdate.Parameters.AddWithValue("@cleaned", cleanedList)
                    cmdUpdate.Parameters.AddWithValue("@count", nameCount)
                    cmdUpdate.Parameters.AddWithValue("@id", rowId)
                    cmdUpdate.ExecuteNonQuery()
                End While
            End Using
        Catch ex As Exception
            Console.WriteLine("Error cleaning Ppeoplelist and updating PNamecount: " & ex.Message)
        End Try
    End Sub

End Module
