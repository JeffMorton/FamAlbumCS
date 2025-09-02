Imports System.Data.SQLite
Imports System.IO
Public Class EventManagment

    Dim txtEvent As New TextBox()
    Dim txtEventDetails As New TextBox()
    Dim txtSearch As New TextBox()
    Dim lblEventCount As New Label
    Dim lblEventID As New Label
    Dim lblEvent As New Label()
    Dim lblFindEvent As New Label()
    Dim lblEventDetails As New Label()
    Dim lblSearch As New Label()
    Private menuStrip As New MenuStrip()
    Private WithEvents lvSearch As New ListView()
    Private WithEvents cbEventsOnFile As New ComboBox()
    Dim Manager As New ConnectionManager(GetConnectionString())
    Dim connection As New SQLiteConnection
    Dim Id As Integer
    Dim Count As Integer
    Private WithEvents btnSave As New Button()
    Private WithEvents btnDelete As New Button()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnCopy As New Button()
    Dim rhp As New Panel
    Dim lhp As New Panel
    Dim rpw As Integer
    Dim dt As New DataTable()
    Private Sub EventManagment(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        Me.WindowState = FormWindowState.Maximized
        With lhp
            .Left = 0
            .Top = 125
            .Width = screenWidth \ 2
            .Height = screenHeight - 125
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
            .AutoScroll = True
        End With
        Me.Controls.Add(lhp)

        With rhp
            .Left = lhp.Right ' ensures it starts right after lhp
            .Top = 125
            .Width = screenWidth - lhp.Width
            .Height = screenHeight - 125
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            .AutoScroll = True
        End With
        rpw = rhp.Width
        Me.Controls.Add(rhp)

        Dim lpw As Integer = lhp.Width
        Dim lph As Integer = lhp.Height

        Dim title As New Label() With {
            .Text = "Family Album",
            .Font = New Font("segoe", 40),
            .Size = New Size(screenWidth, 60),
            .Location = New Point(0, 20),
            .TextAlign = ContentAlignment.MiddleCenter}

        Dim subtitle As New Label() With {
            .Text = "Event  Manager",
            .Font = New Font("segoe", 20),
            .Size = New Size(screenWidth, 40),
            .Location = New Point(0, 80),
        .TextAlign = ContentAlignment.MiddleCenter}

        Me.Controls.Add(title)
        Me.Controls.Add(subtitle)

        Me.menuStrip = fmenus()
        Me.Controls.Add(menuStrip)

        Dim menuItemExit As New ToolStripMenuItem("Exit") With {
             .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_Click
        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        Me.Controls.Add(menuStrip)
        menuStrip.Items.RemoveAt(0)
        menuStrip.Items.Insert(0, menuItemExit)


        With lblFindEvent
            .Height = 20
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(10, 100)
            .AutoSize = True
            .Visible = True
            .Text = "Select Event:"
        End With

        With cbEventsOnFile
            .Location = New Point(1, 130)
            .Size = New Size(CInt(0.8 * rpw), 18)
            .Font = New Font(cbEventsOnFile.Font.FontFamily, 12)
            .MaxDropDownItems = 6
            .DropDownHeight = 150
        End With
        rhp.Controls.Add(cbEventsOnFile)

        dt.Columns.Add("Event", GetType(String))
        dt.Columns.Add("ID", GetType(String))

        With lblEvent
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(10, 280)
            .AutoSize = True
            .Visible = True
            .Text = "Event:"
        End With
        With txtEvent
            .Location = New Point(1, 305)
            .Visible = True
            .Width = CInt(0.8 * rpw)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 40
            .AutoSize = True
        End With

        With lblEventDetails
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(1, 340)
            .AutoSize = True
            .Visible = True
            .Text = "Event Details:"
        End With
        With txtEventDetails
            .Location = New Point(1, 370)
            .Width = CInt(0.8 * rpw)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 100
            '.AutoSize = True
            .Multiline = True
        End With
        With lblEventID
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(201, 470)
            .AutoSize = True
            .Visible = True
        End With
        With lblEventCount
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(1, 470)
            .AutoSize = True
            .Visible = True
        End With
        With lblSearch
            .Height = 20
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(10, 100)
            .AutoSize = True
            .Visible = True
            .Text = "Enter search term:"
        End With
        With txtSearch
            .Location = New Point(10, 130)
            .Visible = True
            .Width = CInt(0.8 * rpw)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 30
            .AutoSize = True
        End With
        With lvSearch
            .Location = New Point(10, 265)
            .Size = New Size(CInt(0.8 * rpw), 400)
            .Font = New Font("Arial", 12)
            .View = View.Details
            .Columns.Add("Event", CInt(0.4 * rpw))
            .Columns.Add("Event Detail", CInt(0.4 * rpw))
        End With

        With btnSearch
            .Text = "Search"
            .Location = New Point(240, 175)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(250, 50)
            .Enabled = True
        End With
        Dim btp As Integer = CInt(txtEvent.Left + txtEvent.Width / 2 - 125)
        With btnSave
            .Text = "Save"
            .Location = New Point(btp, 495)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(250, 50)
            .Enabled = True
        End With
        With btnDelete
            .Text = "Delete"
            .Location = New Point(btp, 545)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(250, 50)
            .Enabled = True
        End With
        With btnCopy
            .Text = "Copy Event Files"
            .Location = New Point(btp, 595)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(250, 50)
            .Enabled = True
        End With

        FillcbEventsOnFile()
        rhp.Controls.Add(lblFindEvent)
        rhp.Controls.Add(lblEvent)
        rhp.Controls.Add(lblEventDetails)
        rhp.Controls.Add(txtEvent)
        rhp.Controls.Add(txtEventDetails)
        rhp.Controls.Add(lblEventCount)
        rhp.Controls.Add(btnSave)
        rhp.Controls.Add(lblEventID)
        rhp.Controls.Add(btnCopy)
        rhp.Controls.Add(btnDelete)
        lhp.Controls.Add(lblSearch)
        lhp.Controls.Add(txtSearch)
        lhp.Controls.Add(btnSearch)
        lhp.Controls.Add(lvSearch)
        AddHandler btnSave.Click, AddressOf btnSave_click
        AddHandler btnCopy.Click, AddressOf btnCopy_click
        AddHandler btnDelete.Click, AddressOf btnDelete_click
        AddHandler btnSearch.Click, AddressOf btnSearch_click
        AddHandler cbEventsOnFile.SelectionChangeCommitted, AddressOf cbEventsOnFile_SelectedChanedCommitted
    End Sub

    Private Sub cbEventsOnFile_SelectedChanedCommitted(sender As Object, e As EventArgs)
        ' Get selected item(s) from ComboBox and store them in the array

        Dim drv As DataRowView = TryCast(cbEventsOnFile.SelectedItem, DataRowView)
        If drv IsNot Nothing AndAlso Not String.IsNullOrEmpty(Trim(drv("Event").ToString())) Then
            Id = CInt(drv("Id"))
        End If

        connection = Manager.GetConnection()
        Dim qryEvent As String = "Select ID,neName,neRelation from NameEvent where neType ='E' and ID=@ID"

        Using connection
            Dim command As New SQLiteCommand(qryEvent, connection)
            command.Parameters.AddWithValue("@ID", Id)
            Try
                Dim reader As SQLiteDataReader = command.ExecuteReader()

                While reader.Read()
                    txtEvent.Text = reader("neName").ToString()
                    If Not reader.IsDBNull(reader.GetOrdinal("neRelation")) Then
                        txtEventDetails.Text = reader("neRelation").ToString
                    Else
                        txtEventDetails.Text = ""
                    End If
                End While

                reader.Close()
                Dim qryCnt As String = "select count(npID) from NamePhoto where npID= @ID"
                Dim command1 As New SQLiteCommand(qryCnt, connection)
                command1.Parameters.AddWithValue("@ID", Id)
                Count = CInt(command1.ExecuteScalar())

                lblEventCount.Text = "Number of image: " & Count
                lblEventID.Text = "Event ID: " & CStr(Id)
            Catch ex As SQLiteException
                MessageBox.Show("Database error: " & ex.Message)
            Catch ex As Exception
                MessageBox.Show("An error occurred: " & ex.Message)
            End Try
        End Using

    End Sub
    Private Sub btnSave_click(sender As Object, e As EventArgs)
        connection = Manager.GetConnection()
        Dim qryEvent As String = "Update NameEvent set neName =@event, neRelation =@details where ID = @ID"
        Dim re As Integer
        Using connection
            Dim command As New SQLiteCommand(qryEvent, connection)
            command.Parameters.AddWithValue("@ID", Id)
            command.Parameters.AddWithValue("@event", txtEvent.Text)
            command.Parameters.AddWithValue("@details", txtEventDetails.Text)
            Try
                re = command.ExecuteNonQuery()
            Catch ex As SQLiteException
                MessageBox.Show("Database error: " & ex.Message)
            Catch ex As Exception
                MessageBox.Show("An error occurred: " & ex.Message)
            End Try
        End Using

    End Sub
    Private Sub FillcbEventsOnFile()
        dt.Clear()
        Dim qryEvent As String = "Select neName,ID from NameEvent where neType ='E' order by neName"

        connection = Manager.GetConnection()
        Using connection
            Dim command As New SQLiteCommand(qryEvent, connection)
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

        cbEventsOnFile.DataSource = dt
        cbEventsOnFile.DisplayMember = "Event"
        cbEventsOnFile.ValueMember = "ID"
        txtEvent.Text = ""
        txtEventDetails.Text = ""
    End Sub
    Private Sub btnDelete_click(sender As Object, e As EventArgs)
        If Count <> 0 Then
            MessageBox.Show("Cannot delete Event with Pictures")
            Exit Sub
        Else
            connection = Manager.GetConnection()
            Dim qryEvent As String = "Delete from NameEvent  where ID = @ID"
            Dim re As Integer
            Using connection
                Dim command As New SQLiteCommand(qryEvent, connection)
                command.Parameters.AddWithValue("@ID", Id)

                Try
                    re = command.ExecuteNonQuery()
                Catch ex As SQLiteException
                    MessageBox.Show("Database error: " & ex.Message)
                Catch ex As Exception
                    MessageBox.Show("An error occurred: " & ex.Message)
                End Try
            End Using
        End If
        FillcbEventsOnFile()
    End Sub
    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)

        Me.Close()
    End Sub
    Private Sub btnSearch_click(sender As Object, e As EventArgs)
        Dim searchText As String = txtSearch.Text.Trim()
        If String.IsNullOrEmpty(searchText) Then
            MessageBox.Show("Search terms cannot be empty")
            Exit Sub
        End If
        lvSearch.Items.Clear()
        connection = Manager.GetConnection()

        Dim qrySearch As String = "SELECT neName, neRelation FROM NameEvent WHERE neType='E' AND neName LIKE @term"
        Using command As New SQLiteCommand(qrySearch, connection)
            command.Parameters.AddWithValue("@term", "%" & txtSearch.Text & "%")
            Dim reader As SQLiteDataReader = command.ExecuteReader()
            Try
                While reader.Read()
                    Dim nameVal = reader("neName").ToString()
                    Dim relVal = reader("neRelation").ToString()

                    Dim item As New ListViewItem(nameVal)
                    item.SubItems.Add(relVal)
                    lvSearch.Items.Add(item)
                End While
            Catch ex As Exception
                MessageBox.Show("ERROR: " & ex.Message)
            End Try
        End Using
    End Sub
    Private Sub btnCopy_click(sender As Object, e As EventArgs)
        Dim folderDialog As New FolderBrowserDialog()
        Dim DDir = GetDefaultDir()
        Dim destinationpath As String
        If folderDialog.ShowDialog() = DialogResult.OK Then
            Try
                Dim connection As SQLiteConnection = Manager.GetConnection()
                Dim qry As String = "Select npFilename from NamePhoto where npID =@EventID"
                Using connection
                    Dim command As New SQLiteCommand(qry, connection)
                    command.Parameters.AddWithValue("@EventID", Id)
                    Dim reader As SQLiteDataReader = command.ExecuteReader()
                    While reader.Read()
                        destinationpath = Path.Combine(folderDialog.SelectedPath, Path.GetFileName(reader("npFilename").ToString))
                        File.Copy(DDir & reader("npFilename").ToString, destinationpath, True)
                    End While
                End Using
                MessageBox.Show("Files copied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Error copying file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub
End Class