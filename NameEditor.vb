Imports System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder
Imports System.Data.SQLite

Public Class NameEditor
    Dim lfp As New Panel
    Dim rhp As New Panel
    Dim txtName As New TextBox()
    Dim txtRelation As New TextBox()
    Dim txtSearch As New TextBox()
    Dim lblNameCount As New Label
    Dim lblName As New Label()
    Dim lblFindName As New Label()
    Dim lblRelation As New Label()
    Dim lblSearch As New Label()
    Private menuStrip As New MenuStrip()
    Private WithEvents lvSearch As New ListView()
    Private WithEvents cbNamesOnFile As New ComboBox()
    Dim Manager As New ConnectionManager(GetConnectionString())
    Dim connection As New SQLiteConnection
    Dim Id As Integer
    Dim Count As Integer
    Private WithEvents btnSave As New Button()
    Private WithEvents btnDelete As New Button()
    Private WithEvents btnSearch As New Button()
    Dim dt As New DataTable()
    Private Sub NameManagment(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        Me.WindowState = FormWindowState.Maximized
        Dim lhp As New Panel With {
            .Left = 0,
            .Top = 123,
            .Width = screenWidth \ 2,
            .Height = screenHeight - 123,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left,
            .Visible = True,
            .AutoScroll = True}

        Me.Controls.Add(lhp)

        Dim rhp As New Panel With {
        .Left = lhp.Right, ' ensures it starts right after lhp
            .Top = 123,
            .Width = screenWidth - lhp.Width,
            .Height = screenHeight - 123,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .Visible = True,
            .AutoScroll = True}
        Me.Controls.Add(rhp)
        Dim rpw = rhp.Width
        Dim lpw As Integer = lhp.Width
        Dim lph As Integer = lhp.Height
        Me.Controls.Add(rhp)
        Dim title As New Label() With {
            .Text = "Family Album",
            .Font = New Font("segoe", 40),
            .Size = New Size(screenWidth, 60),
            .Location = New Point(0, 20),
            .TextAlign = ContentAlignment.MiddleCenter}
        Me.Controls.Add(title)
        Dim subtitle As New Label() With {
            .Text = "Name  Manager",
            .Font = New Font("segoe", 30),
            .Size = New Size(screenWidth, 30),
            .Location = New Point(0, 82),
            .TextAlign = ContentAlignment.MiddleCenter}
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

        With lblFindName
            .Height = 20
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(10, 100)
            .AutoSize = True
            .Visible = True
            .Text = "Select Name:"
        End With


        With cbNamesOnFile
            .Location = New Point(1, 130)
            .Size = New Size(CInt(0.8 * rpw), 18)
            .Font = New Font(cbNamesOnFile.Font.FontFamily, 12)
            .MaxDropDownItems = 6
            .DropDownHeight = 150
        End With
        rhp.Controls.Add(cbNamesOnFile)

        dt.Columns.Add("Name", GetType(String))
        dt.Columns.Add("ID", GetType(String))


        With lblName
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(10, 270)
            .AutoSize = True
            .Visible = True
            .Text = "Name:"
        End With
        With txtName
            .Location = New Point(1, 300)
            .Visible = True
            .Width = CInt(0.8 * rpw)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 40
            .AutoSize = True
        End With

        With lblRelation
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(1, 325)
            .AutoSize = True
            .Visible = True
            .Text = "Relationship:"
        End With
        With txtRelation
            .Location = New Point(1, 350)
            .Width = CInt(0.8 * rpw)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 100
            '.AutoSize = True
            .Multiline = True
        End With

        With lblNameCount
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
            .Columns.Add("Name", CInt(0.4 * rpw))
            .Columns.Add("Relation", CInt(0.4 * rpw))
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
        Dim btp As Integer = CInt(txtName.Left + txtName.Width / 2 - 125)
        With btnSave
            .Text = "Save"
            .Location = New Point(btp, 490)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(250, 50)
            .Enabled = True
        End With
        With btnDelete
            .Text = "Delete"
            .Location = New Point(btp, 540)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(250, 50)
            .Enabled = True
        End With
        FillcbNamesOnFile()
        rhp.Controls.Add(lblFindName)
        rhp.Controls.Add(lblName)
        rhp.Controls.Add(lblRelation)
        rhp.Controls.Add(txtName)
        rhp.Controls.Add(txtRelation)
        rhp.Controls.Add(lblNameCount)
        rhp.Controls.Add(btnSave)
        rhp.Controls.Add(btnDelete)
        lhp.Controls.Add(lblSearch)
        lhp.Controls.Add(txtSearch)
        lhp.Controls.Add(btnSearch)
        lhp.Controls.Add(lvSearch)
        AddHandler btnSave.Click, AddressOf btnSave_click
        AddHandler btnDelete.Click, AddressOf btnDelete_click
        AddHandler btnSearch.Click, AddressOf btnSearch_click

    End Sub
    Private Sub cbNamesOnFile_SelectedChanedgeVommitted(sender As Object, e As EventArgs) Handles cbNamesOnFile.SelectionChangeCommitted
        ' Get selected item(s) from ComboBox and store them in the array

        Dim drv As DataRowView = TryCast(cbNamesOnFile.SelectedItem, DataRowView)
        If drv IsNot Nothing AndAlso Not String.IsNullOrEmpty(Trim(drv("Name").ToString())) Then
            Id = CInt(drv("Id"))
        End If
        connection = Manager.GetConnection()
        Dim qryName As String = "Select ID,neName,neRelation from NameEvent where neType ='N' and ID=@ID"

        Using connection
            Dim command As New SQLiteCommand(qryName, connection)
            command.Parameters.AddWithValue("@ID", Id)
            Try
                Dim reader As SQLiteDataReader = command.ExecuteReader()

                While reader.Read()
                    txtName.Text = reader("neName").ToString()
                    If Not reader.IsDBNull(reader.GetOrdinal("neRelation")) Then
                        txtRelation.Text = reader("neRelation").ToString
                    Else
                        txtRelation.Text = ""
                    End If
                End While

                reader.Close()
                Dim qryCnt As String = "select count(npID) from NamePhoto where npID= @ID"
                Dim command1 As New SQLiteCommand(qryCnt, connection)
                command1.Parameters.AddWithValue("@ID", Id)
                Count = CInt(command1.ExecuteScalar())

                lblNameCount.Text = "Number of images: " & Count

            Catch ex As SQLiteException
                MessageBox.Show("Database error: " & ex.Message)
            Catch ex As Exception
                MessageBox.Show("An error occurred: " & ex.Message)
            End Try
        End Using

    End Sub
    Private Sub btnSave_click(sender As Object, e As EventArgs)
        connection = Manager.GetConnection()
        Dim qryName As String = "Update NameEvent set neName =@Name, neRelation =@details where ID = @ID"
        Dim re As Integer
        Using connection
            Dim command As New SQLiteCommand(qryName, connection)
            command.Parameters.AddWithValue("@ID", Id)
            command.Parameters.AddWithValue("@Name", txtName.Text)
            command.Parameters.AddWithValue("@details", txtRelation.Text)
            Try
                re = command.ExecuteNonQuery()
            Catch ex As SQLiteException
                MessageBox.Show("Database error: " & ex.Message)
            Catch ex As Exception
                MessageBox.Show("An error occurred: " & ex.Message)
            End Try
        End Using

    End Sub
    Private Sub FillcbNamesOnFile()
        dt.Clear()
        Dim qryName As String = "Select neName,ID from NameEvent where neType ='N' order by neName"

        connection = Manager.GetConnection()
        Using connection
            Dim command As New SQLiteCommand(qryName, connection)
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
        txtName.Text = ""
        txtRelation.Text = ""
    End Sub
    Private Sub btnDelete_click(sender As Object, e As EventArgs)
        If Count <> 0 Then
            MessageBox.Show("Cannot delete Name with Pictures")
            Exit Sub
        Else
            connection = Manager.GetConnection()
            Dim qryName As String = "Delete from NameEvent  where ID = @ID"
            Dim re As Integer
            Using connection
                Dim command As New SQLiteCommand(qryName, connection)
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
        FillcbNamesOnFile()
    End Sub
    Private Sub btnSearch_click(sender As Object, e As EventArgs)
        Dim searchText As String = txtSearch.Text.Trim()
        If String.IsNullOrEmpty(searchText) Then
            MessageBox.Show("Search terms cannot be empty")
            Exit Sub
        End If
        lvSearch.Items.Clear()
        connection = Manager.GetConnection()

        Dim qrySearch As String = "SELECT neName, neRelation FROM NameEvent WHERE neType='N' AND neName LIKE @term"
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
    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub
End Class