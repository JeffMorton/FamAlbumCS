
Imports System.Collections.ObjectModel
Imports Microsoft.Data.SqlClient

Imports System.Drawing.Text
Imports System.IO
Imports System.Web
Imports System.Windows.Forms
Imports System.Xml
Imports Microsoft.Win32

Public Class Thum

    Dim flowPanel As New FlowLayoutPanel()
    Public Property NamesSelected() As String()
    Dim connectionString As String = "Server=.\SQLEXPRESS;Database=FamilyAlbum;Trusted_Connection=yes;TrustServerCertificate=True;"

    Dim piclist(2000) As String
    Private menuStrip As New MenuStrip()

    Dim Cnt As Integer = 0

    Private Sub Thum_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.WindowState = FormWindowState.Maximized

        'SetupComponent()
        InitializeFlowLayoutPanel()
        Dim Connection As New SQLconnection(connectionString)

        Dim menuItemExit As New ToolStripMenuItem("Exit")
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_Click

        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        Me.Controls.Add(menuStrip)
        If NamesSelected(0) = "Event" Then
            Connection.Open()
            Dim x As Integer = 0
            Dim DDir As String = GetDefaultDir()

            Dim qryGetFiles As String = "select NLFileName from Namelist where NLName = @Name"
            Dim commanda As New SQLcommand(qryGetFiles, Connection)
            commanda.Parameters.AddWithValue("@Name", CStr(NamesSelected(1))) ' Use parameterized query
            Dim reader As SqlDataReader = commanda.ExecuteReader()
            While reader.Read()
                x += 1
                piclist(x) = reader("NLFilename")
            End While
            reader.Close()
            DisplayThumbnails(piclist, x, Connection)

            Connection.Close()

        Else

            Dim qryfixname As String = "select lcID,lcDescription from Listclass where LcDescription = @Name"
            Dim x1 As Integer = 0
            Connection.Open()
            For I = 1 To 5
                Dim command1 As New SqlCommand(qryfixname, Connection)
                command1.Parameters.AddWithValue("@Name", NamesSelected(I)) ' Use parameterized query

                Dim reader As SqlDataReader = command1.ExecuteReader()
                While reader.Read()
                    x1 += 1
                    NamesSelected(x1) = reader("lcID").ToString
                End While
                reader.Close()
            Next

            Connection.Close()
            Dim combobox1 As New ComboBox
            'Controls.Add(combobox1)
            Cnt = x1
            Controls.Add(flowPanel)





            Dim qryPic As String = "Select  Namelist.NLFileName, Pictures.PMonth, Pyear,Pictures.Pnamecount From Namelist INNER Join Pictures On Namelist.NLFileName = Pictures.PFileName WHERE Namelist.NLName IN (@NLName1,@NLName2,@NLName3,@NLName4,@NLName5)  GROUP BY Namelist.NLFileName, Pictures.PMonth, Pictures.PYear,Pictures.PNameCount ORDER BY Pictures.PYear, Pictures.PMonth;"

            Using Connection
                Dim command As New SqlCommand(qryPic, Connection)
                command.Parameters.AddWithValue("@NLName1", NamesSelected(1)) ' Use parameterized query
                command.Parameters.AddWithValue("@NLName2", NamesSelected(2)) ' Use parameterized query
                command.Parameters.AddWithValue("@NLName3", NamesSelected(3)) ' Use parameterized query
                command.Parameters.AddWithValue("@NLName4", NamesSelected(4)) ' Use parameterized query
                command.Parameters.AddWithValue("@NLName5", NamesSelected(5)) ' Use parameterized query

                Try
                    Connection.Open()
                    Dim reader As SqlDataReader = command.ExecuteReader()
                    Dim x = 0
                    While reader.Read()
                        If NamesSelected(0) = "Exclusive" Then
                            If Cnt = reader("PNameCount") Then
                                x += 1
                                If x > 1999 Then Exit While
                                piclist(x) = reader("NLFilename").ToString()
                            End If
                        Else
                            x += 1
                            If x > 1999 Then Exit While
                            piclist(x) = reader("NLFilename").ToString()
                        End If


                    End While
                    reader.Close()
                Catch ex As SQLException
                    MessageBox.Show("Database error: " & ex.Message)
                Catch ex As Exception
                    MessageBox.Show("An error occurred: " & ex.Message)
                End Try
                DisplayThumbnails(piclist, Cnt, Connection)
            End Using
        End If
    End Sub
    Private Sub SetupComponent()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(12, 12)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(200, 200)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        Me.Controls.Add(Me.PictureBox1)
        '
        'Thum
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 261)
        Me.Name = "Thum"
        Me.Text = "Thum"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox

    Private Sub DisplayThumbnails(namelist As String(), cnt As Integer, Connection As SqlConnection)
        flowPanel.Controls.Clear()

        Using Connection
            Try
                If Connection.State <> ConnectionState.Open Then
                    Connection.Open()
                End If
                Dim ImageData As Byte()
                Dim Peoplelist As String

                For Each name As String In namelist
                    If name IsNot Nothing Then
                        Dim command As New SqlCommand("SELECT Pthumbnail as imagedata,Ppeoplelist FROM Pictures WHERE Pfilename = @name", Connection)
                        command.Parameters.AddWithValue("@name", name)
                        Using reader As SqlDataReader = command.ExecuteReader()
                            While reader.Read()
                                ' Assuming PThumbnail is binary and Ppeoplelist is a string
                                ImageData = DirectCast(reader("imageData"), Byte())
                                Peoplelist = reader("Ppeoplelist").ToString()
                                If ImageData IsNot Nothing And ImageData.Length > 0 And ((CheckPicture(Peoplelist, cnt) = True) Or (NamesSelected(0) <> "Event")) Then
                                    Using ms As New MemoryStream(ImageData)
                                        Dim picBox As New PictureBox
                                        picBox.Image = Image.FromStream(ms)
                                        picBox.SizeMode = PictureBoxSizeMode.Zoom
                                        picBox.Width = 200
                                        picBox.Height = 200
                                        picBox.Tag = name ' Store the file name in the Tag property
                                        AddHandler picBox.MouseUp, AddressOf PictureBox_MouseUp ' Add MouseUp event handler
                                        flowPanel.Controls.Add(picBox)
                                    End Using
                                Else
                                    ' Handle case when no image is found, if necessary
                                End If

                            End While
                        End Using




                    End If
                Next

            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message)
            End Try
        End Using
        Connection.Close()
    End Sub


    Private Sub PictureBox_MouseUp(sender As Object, e As MouseEventArgs)
        Dim picBox As PictureBox = CType(sender, PictureBox)
        Dim SfileName As String = CType(picBox.Tag, String)

        If e.Button = MouseButtons.Left Then
            Dim DisplayPicForm As New DisplayPic()
            DisplayPicForm.StartPosition = FormStartPosition.Manual
            Me.StartPosition = FormStartPosition.Manual

            ' Calculate the X position to center the form horizontally
            Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
            Dim formWidth As Integer = Me.Width
            Dim xPosition As Integer = (screenWidth - formWidth) \ 2

            ' Set the Y position to 0 (top of the screen)
            Dim yPosition As Integer = 0

            ' Set the form's Location
            DisplayPicForm.Location = New Point(xPosition, yPosition)

            DisplayPicForm.SFileName = SfileName
            DisplayPicForm.Show()
            ' Add more logic here for left-click actions
        ElseIf e.Button = MouseButtons.Right Then
            ' Handle right-click event
            Dim DisplayInfom As New DisplayPic()
            DisplayInfo.StartPosition = FormStartPosition.Manual
            Me.StartPosition = FormStartPosition.Manual

            ' Calculate the X position to center the form horizontally
            Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
            Dim formWidth As Integer = Me.Width
            Dim xPosition As Integer = (screenWidth - formWidth) \ 2

            ' Set the Y position to 0 (top of the screen)
            Dim yPosition As Integer = 0

            ' Set the form's Location
            DisplayInfo.Location = New Point(xPosition, yPosition)

            DisplayInfo.SFileName = SfileName
            DisplayInfo.Show()
            ' Add more logic here for right-click actions
        End If
    End Sub
    Private Sub InitializeFlowLayoutPanel()

        flowPanel.Name = "flowLayoutPanel1"
        flowPanel.Size = New Size(1000, 1000)
        flowPanel.Dock = DockStyle.Fill ' Adjust as needed
        flowPanel.AutoScroll = True
        ' Add the FlowLayoutPanel to the form
        Me.Controls.Add(flowPanel)
    End Sub
    Private Function CheckPicture(Names As String, cnt As Integer) As Boolean
        Dim j As Integer
        If NamesSelected(0) = "Event" Then
            CheckPicture = True
            Exit Function
        End If
        For i = 1 To 5
            If InStr(Names, NamesSelected(i)) > 0 Then j += 1
        Next i
        If j = cnt Then
            CheckPicture = True
        Else
            CheckPicture = False

        End If

    End Function

    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub
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

End Class
