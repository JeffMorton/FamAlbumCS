Imports System.Data.SQLite
Imports System.IO
Imports System.Threading
Imports Microsoft.Win32
Imports Windows.Win32.UI
Public Class Sthumb

    Dim WithEvents flowPanel As New FlowLayoutPanel()
    Public Property NamesSelected As String()
    Dim connection As New SQLiteConnection
    Dim Manager As New ConnectionManager(GetConnectionString())

    Dim piclist(5000) As String
    Private menuStrip As New MenuStrip()

    Dim Cnt As Integer = 0
    Private loadingLabel As Label
    Dim x As Integer = 0

    Dim shownFirstThumbnail As Integer = 0 ' 0 = not shown, 1 = shown

    Private Sub SThumb_FormClosing(sender As Object, e As FormClosingEventArgs)
        flowPanel.SuspendLayout()
        flowPanel.Controls.Clear() ' Clear the panel
        flowPanel.ResumeLayout()
        GC.Collect()
    End Sub

    Private Sub FlowPanel_MouseWheel(sender As Object, e As MouseEventArgs)
        Dim scrollStep As Integer = 20 ' Fine-tuned for smoother feel
        Dim newValue As Integer = flowPanel.VerticalScroll.Value - Math.Sign(e.Delta) * scrollStep
        newValue = Math.Max(flowPanel.VerticalScroll.Minimum, Math.Min(flowPanel.VerticalScroll.Maximum, newValue))
        flowPanel.VerticalScroll.Value = newValue
    End Sub

    Private Sub SThumb_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
        'UpdateStyles()
        loadingLabel = New Label() With {
        .AutoSize = False,
        .Size = New Size(250, 30),
        .BackColor = Color.LightYellow,
        .ForeColor = Color.Black,
        .Font = New Font("Segoe UI", 12, FontStyle.Bold),
        .TextAlign = ContentAlignment.MiddleCenter,
        .Text = "Loading..."
    }
        Me.WindowState = FormWindowState.Maximized
        Me.AutoScroll = True
        'SetupComponent()
        InitializeFlowLayoutPanel()
        connection = Manager.GetConnection()
        Dim DDir As String = GetDefaultDir()

        Me.menuStrip = fmenus()
        Dim menuItemExit As New ToolStripMenuItem("Exit") With {
             .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_Click
        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        menuStrip.Items.RemoveAt(0)
        menuStrip.Items.Insert(0, menuItemExit)
        Me.Controls.Add(menuStrip)
        flowPanel.Controls.Clear()

        ' Style and size

        Me.Controls.Add(loadingLabel)
        AddHandler Me.FormClosing, AddressOf SThumb_FormClosing
        AddHandler flowPanel.MouseWheel, AddressOf FlowPanel_MouseWheel
        Dim centerX As Integer = (Me.ClientSize.Width - loadingLabel.Width) \ 2
        Dim centerY As Integer = (Me.ClientSize.Height - loadingLabel.Height) \ 2
        loadingLabel.Location = New Point(centerX, centerY)
        loadingLabel.BringToFront()
        loadingLabel.Visible = True
        Application.DoEvents() ' Force paint before loading thumbnails

        Dim SortOrd As String
        If Mid(NamesSelected(0), 3, 4) = ":Old" Then
            SortOrd = " Order by Pictures.PYear ,Pictures.Pmonth;"
        Else
            SortOrd = " Order by Pictures.PYear  desc,Pictures.Pmonth asc;"
        End If
        Dim SelectedPeople As Integer = 0
        For i = 1 To 5
            If NamesSelected(i) IsNot "99999" Then SelectedPeople += 1
        Next
        Dim command As New SQLiteCommand()

        If NamesSelected(0).StartsWith("NP") Then
            Dim qryPic As String = "SELECT Distinct Pfilename,npFileName, Pictures.PMonth, Pyear, Pictures.PPeoplelist, Pictures.Pthumbnail, Pnamecount
            FROM NamePhoto
            INNER JOIN Pictures ON NamePhoto.npFileName = Pictures.PFileName
            where PPeoplelist ='1' or Ppeoplelist is NULl or PPeoplelist=''
            " & SortOrd
            command.CommandText = qryPic
            command.Connection = connection
        Else
            Dim qryPic As String = "SELECT Distinct Pfilename,npFileName, Pictures.PMonth, Pyear, Pictures.PPeoplelist, Pictures.Pthumbnail, Pnamecount
            FROM NamePhoto
            INNER JOIN Pictures ON NamePhoto.npFileName = Pictures.PFileName
            WHERE npID IN (@NLName1,@NLName2,@NLName3,@NLName4,@NLName5)
            " & SortOrd
            command.CommandText = qryPic
            command.Connection = connection
            Command.Parameters.AddWithValue("@NLName1", NamesSelected(1))
            Command.Parameters.AddWithValue("@NLName2", NamesSelected(2))
            command.Parameters.AddWithValue("@NLName3", NamesSelected(3))
            command.Parameters.AddWithValue("@NLName4", NamesSelected(4))
            command.Parameters.AddWithValue("@NLName5", NamesSelected(5))
        End If


        Using connection


            Try
                Dim reader As SQLiteDataReader = command.ExecuteReader()
                While reader.Read()
                    Dim n As Integer = 0
                    Dim peopleList As String = CStr(reader("PPeoplelist"))
                    For j = 1 To 5
                        If peopleList.Contains(NamesSelected(j)) Then n += 1
                    Next
                    Dim allowAction As Boolean = False

                    If Not NamesSelected(0).StartsWith("Ex") Then
                        If SelectedPeople = n Or NamesSelected(0) = "-2" Then
                            allowAction = True
                        End If
                    Else
                        If SelectedPeople = CInt(reader("Pnamecount")) Then
                            Dim j As Integer = 0
                            For i = 1 To n
                                If peopleList.Split(","c).Contains(NamesSelected(i)) Then j += 1
                            Next
                            If j = SelectedPeople Then
                                allowAction = True
                            End If
                        End If
                    End If
                    If allowAction Then
                        If Not reader.IsDBNull(reader.GetOrdinal("Pthumbnail")) Then
                            Dim imgData As Byte() = DirectCast(reader("Pthumbnail"), Byte())
                            Dim ms As New MemoryStream(imgData)
                            Dim thumbImage As Image = Image.FromStream(ms)

                            Dim pb As New PictureBox
                            pb.Image = thumbImage
                            pb.SizeMode = PictureBoxSizeMode.Zoom
                            pb.Width = 150
                            pb.Height = 150
                            pb.Margin = New Padding(5)
                            pb.Tag = reader("npFileName").ToString
                            AddHandler pb.MouseUp, AddressOf PictureBox_MouseUp

                            flowPanel.Controls.Add(pb)
                            x += 1
                            If x > 4999 Then Exit While
                        End If
                    End If
                End While
                reader.Close()
            Catch ex As SQLiteException
                MessageBox.Show("Database error: " & ex.Message)
            Catch ex As Exception
                MessageBox.Show("An error occurred: " & ex.Message)
            End Try
        End Using
        Dim countMenuItem As New ToolStripMenuItem($"{x} images") With {
        .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
    }
        menuStrip.Items.RemoveAt(6)
        menuStrip.Items.Insert(6, countMenuItem)
        loadingLabel.Visible = False
        Me.Controls.Add(flowPanel)

        flowPanel.Enabled = True
        flowPanel.Refresh()
    End Sub
    Private Sub PictureBox_MouseUp(sender As Object, e As MouseEventArgs)
        Dim picBox As PictureBox = CType(sender, PictureBox)
        Dim SfileName As String = CType(picBox.Tag, String)
        If e.Button = MouseButtons.Left Then
            Dim DisplayPicForm As New DisplayPics()
            DisplayPicForm.AutoSize = True
            DisplayPicForm.StartPosition = FormStartPosition.Manual
            Me.StartPosition = FormStartPosition.Manual

            ' Calculate the X position to center the form horizontally
            Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
            Dim formWidth As Integer = Me.Width
            Dim xPosition As Integer = (screenWidth - formWidth) \ 2

            ' Set the Y position to 0 (top of the screen)
            Dim yPosition As Integer = 0

            If CMovie(SfileName) Then

                DisplayPicForm.SFileName = SfileName
                DisplayPicForm.Show()
                '
            Else
                DisplayPicForm.SFileName = SfileName
                DisplayPicForm.Show()
                ' Add more logic here for left-click actions
            End If
        ElseIf e.Button = MouseButtons.Right Then
            Dim di As New Displayinfo()
            di.SFileName = SfileName
            di.Show()

        End If
    End Sub
    Private Sub InitializeFlowLayoutPanel()
        flowPanel.Name = "flowLayoutPanel1"
        flowPanel.Size = New Size(1000, 1000)
        flowPanel.Dock = DockStyle.Fill ' Adjust as needed
        flowPanel.AutoScroll = True
    End Sub
    Private Function CheckPicture(Names As String, cnt As Integer) As Boolean
        Dim j As Integer
        If NamesSelected(0) <> "Exclusive" Then
            CheckPicture = True
            Exit Function
        End If
        For i = 1 To 5
            If NamesSelected(i) = Names Then j += 1
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

End Class
