
Imports System.Data.SQLite
Public Class Start
    Private NamesSelected As String()
    Private WithEvents btnContinue As New Button
    Private WithEvents cbNamesOnFile As New ComboBox()
    Private lvNamesSelected As New ListBox
    Public cnt As Integer = 0
    Public DefaultDir As String
    Dim Manager As New ConnectionManager(GetConnectionString())
    Dim connection As New SQLiteConnection
    Private WithEvents Exclusive As New CheckBox
    Dim lb2 As New Label
    Private menuStrip As New MenuStrip()
    Private Sub Start_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.AutoScroll = True
            Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
            Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
            Dim cnt As Integer = 0
            Me.WindowState = FormWindowState.Maximized
            ReDim NamesSelected(5)
            NamesSelected(0) = "Al:Old"
            NamesSelected(1) = "99999"
            NamesSelected(2) = "99999"
            NamesSelected(3) = "99999"
            NamesSelected(4) = "99999"
            NamesSelected(5) = "99999"

            Me.Controls.Add(Exclusive)
            Me.menuStrip = fmenus()
            Dim menuItemSelectPeople As New ToolStripMenuItem("Select People") With {
             .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
            Dim SortOldNew As New ToolStripMenuItem("Sort Old to New")
            Dim SortNewOld As New ToolStripMenuItem("Sort New to Old")
            Dim ClearSelected As New ToolStripMenuItem("Clear Selected")
            menuStrip.Items.Add(menuItemSelectPeople)
            menuItemSelectPeople.DropDownItems.Add(SortOldNew)
            menuItemSelectPeople.DropDownItems.Add(SortNewOld)
            menuItemSelectPeople.DropDownItems.Add(ClearSelected)

            AddHandler SortOldNew.Click, AddressOf Sortold_click
            AddHandler SortNewOld.Click, AddressOf SortNew_click
            AddHandler ClearSelected.Click, AddressOf ClearSelected_Click
            AddHandler Exclusive.CheckedChanged, AddressOf Exclusive_checked
            Me.Controls.Add(menuStrip)
            menuStrip.Items.RemoveAt(1)
            menuStrip.Items.Insert(1, menuItemSelectPeople)

            Dim menuItemExit As New ToolStripMenuItem("Exit") With {
             .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
            AddHandler menuItemExit.Click, AddressOf Exitapp
            menuStrip.Items.RemoveAt(0)
            menuStrip.Items.Insert(0, menuItemExit)

            Dim title As Label = New Label() With {
                .Text = "Family Album",
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("segoe", 45),
                .Size = New Size(600, 55),
                .Location = New Point(CInt(screenWidth / 2 - 300), 22)
            }

            Me.Controls.Add(title)

            Dim subtitle As Label = New Label() With {
                .Text = "Select People",
                .Font = New Font("segoe", 24),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Size = New Size(600, 40),
                .Location = New Point(CInt(screenWidth / 2 - 300), 85)
            }
            Me.Controls.Add(subtitle)
            Dim lb1 As New Label() With {
                .Text = "Choose up to 5 People",
            .Location = New Point(CInt((screenWidth - 400) / 2), 150),
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .Size = New Size(400, 30),
            .TextAlign = ContentAlignment.MiddleCenter
            }
            Controls.Add(lb1)
            With lb2
                .Text = "(Old to New)"
                .Location = New Point(CInt((screenWidth - 400) / 2), 180)
                .Font = New Font("Arial", 10, FontStyle.Bold)
                .Size = New Size(400, 20)
                .TextAlign = ContentAlignment.MiddleCenter
            End With
            Controls.Add(lb2)
            With cbNamesOnFile
                .Location = New Point(CInt(screenWidth / 2 - 400), 200)
                .Size = New Size(800, 18)
                .Font = New Font(cbNamesOnFile.Font.FontFamily, 12)
                .MaxDropDownItems = 6
                .DropDownHeight = 255
            End With
            AddHandler cbNamesOnFile.SelectionChangeCommitted, AddressOf cbNamesOnFile_SelectedIndexChanged
            CenterControl(cbNamesOnFile, 200)
            Me.Controls.Add(cbNamesOnFile)

            With lvNamesSelected
                .ForeColor = Color.DarkBlue
                .Font = New Font("Arial", 12, FontStyle.Bold)
                .Size = New Size(800, 100)
                .ForeColor = Color.DarkBlue
            End With
            Controls.Add(lvNamesSelected)
            CenterControl(lvNamesSelected, 490)

            Dim lbEx As Label = New Label With {
            .Size = New Size(400, 21),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .Text = "Only these people: ",
            .TextAlign = ContentAlignment.MiddleRight,
            .Location = New Point(CInt((screenWidth - 400) / 2 - 150), 600)
            }
            Me.Controls.Add(lbEx)
            With Exclusive
                .Size = New Size(30, 20)
                .TextAlign = ContentAlignment.MiddleRight
                .Location = New Point(CInt((screenWidth - 400) / 2 + 258), 600)
            End With

            With btnContinue
                .Text = "Continue"
                .BackColor = Color.LightBlue
                .ForeColor = Color.DarkBlue
                .Font = New Font("Arial", 12, FontStyle.Bold)
                .Size = New Size(250, 40)
                .Enabled = False
            End With
            AddHandler btnContinue.Click, AddressOf btnContinue_Click
            CenterControl(btnContinue, 630)
            Dim dt As New DataTable()
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("ID", GetType(String))
            Controls.Add(lvNamesSelected)
            Me.Controls.Add(btnContinue)
            ' Add cbNamesOnFile to the form's controls
            Me.Controls.Add(cbNamesOnFile)

            Dim qryNameCnt As String = "Select neName,ID from NameEvent where neType ='N' order by neName"
            connection = Manager.GetConnection()
            Using connection
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
            End Using

            cbNamesOnFile.DataSource = dt
            cbNamesOnFile.DisplayMember = "Name"
            cbNamesOnFile.ValueMember = "ID"
            Me.Cursor = Cursors.Default
            cbNamesOnFile.Focus()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub cbNamesOnFile_SelectedIndexChanged(sender As Object, e As EventArgs)
        ' Get selected item(s) from ComboBox and store them in the array
        cnt += 1
        If cnt >= 6 Then
            MessageBox.Show("Only 5 names can be selected")
            Exit Sub
        End If
        Dim drv As DataRowView = CType(cbNamesOnFile.SelectedItem, DataRowView)
        Dim nameValue As String = drv("Name").ToString()

        If Not String.IsNullOrWhiteSpace(nameValue) Then
            NamesSelected(cnt) = nameValue
            lvNamesSelected.Items.Add(nameValue)
            NamesSelected(cnt) = CStr(drv("ID"))

            btnContinue.Enabled = True
        End If
        cbNamesOnFile.DroppedDown = True
    End Sub
    Private Sub CenterControl(ctrl As Control, y As Integer)
        ' Calculate the position to center the control
        Dim x As Integer = (Me.ClientSize.Width - ctrl.Width) \ 2
        'Dim y As Integer = () \ 2

        ' Set the control's position
        ctrl.Location = New Point(x, y)
    End Sub
    Private Sub btnContinue_Click(sender As Object, e As EventArgs)
        If Exclusive.Checked = True Then
            Mid(NamesSelected(0), 1, 2) = "Ex"
        End If

        Dim thumbForm As New Sthumb()
        thumbForm.NamesSelected = NamesSelected
        thumbForm.Show()
    End Sub
    Private Sub MenuItemPeople_Click(sender As Object, e As EventArgs)
        Dim strt As New Start()
        strt.Show()
    End Sub
    Private Sub MenuItemClear_Click(sender As Object, e As EventArgs)
        lvNamesSelected.Items.Clear()
        For i As Integer = 1 To NamesSelected.Length - 1
            NamesSelected(i) = CStr(-1)
        Next
        Me.Refresh()
    End Sub
    Private Sub MenuitemAdd_Click(sender As Object, e As EventArgs)
        Dim AddP As New AddPhoto()
        AddP.Show()
    End Sub
    Private Sub Exclusive_checked(sender As Object, e As EventArgs)
        Mid(NamesSelected(0), 1, 2) = "Ex"
        If Exclusive.Checked Then
            Mid(NamesSelected(0), 1, 2) = "Ex"
        Else
            Mid(NamesSelected(0), 1, 2) = "Al"
        End If
    End Sub
    Private Sub ClearSelected_Click(sender As Object, e As EventArgs)
        lvNamesSelected.Items.Clear()
        NamesSelected(1) = "99999"
        NamesSelected(2) = "99999"
        NamesSelected(3) = "99999"
        NamesSelected(4) = "99999"
        NamesSelected(5) = "99999"
        cnt = 0
    End Sub
    Private Sub Sortold_click(sender As Object, e As EventArgs)
        Mid(NamesSelected(0), 3, 4) = ":Old"
        lb2.Text = "(Old to New)"
        cbNamesOnFile.DroppedDown = True
    End Sub
    Private Sub SortNew_click(sender As Object, e As EventArgs)
        Mid(NamesSelected(0), 3, 4) = ":New"
        lb2.Text = "(New to Old)"
        cbNamesOnFile.DroppedDown = True
    End Sub
    Private Sub Exitapp(sender As Object, e As EventArgs)
        Dim qry As String = "select PDateEntered from Pictures order by PDateEntered desc limit 1"
        connection = Manager.GetConnection()
        Dim LastDate As Date = CDate("1 / 1 / 2000")
        Using connection
            Dim command As New SQLiteCommand(qry, connection)
            LastDate = CDate(command.ExecuteScalar())
        End Using

        If LastDate.Date = Today.Date Then
            Dim result As Integer = MsgBox("Do you want to backup the database", vbYesNo, "Confirmation")
            If result = vbYes Then
                Dim connectionString = GetConnectionString()
                Dim databaseName As String = "FamilyAlbum"
                Dim filename As String = "FamAlbum" & Today.Month & Today.Day & Today.Year & ".bak"
                Dim BackupPath As String = GetBackupPath() & filename
                BackupSQLiteDatabase()
            End If
        End If
        Application.Exit()
    End Sub
End Class


