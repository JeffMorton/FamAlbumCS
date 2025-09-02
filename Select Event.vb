Imports System.Data.SQLite
Imports System.IO
Public Class Select_Event
    Public Property Etype As String
    Dim txtEvent As New TextBox()
    Dim txtEventDetails As New TextBox()
    Private NamesSelected As String()
    Private WithEvents btnContinue As New Button With {
    .Text = "Continue",
     .BackColor = Color.LightBlue,
    .ForeColor = Color.DarkBlue,
    .Font = New Font("Arial", 12, FontStyle.Bold),
    .Size = New Size(250, 40),
    .Enabled = False
    }
    Private WithEvents cbEventsOnFile As New ComboBox()
    Public cnt As Integer = 0
    Public DefaultDir As String
    Dim Manager As New ConnectionManager(GetConnectionString())
    Dim connection As New SQLiteConnection
    Dim strEvent As String
    Private menuStrip As New MenuStrip()
    Private lvNamesSelected As New ListBox
    Private selPanel As New Panel() With {
        .Dock = DockStyle.Fill}

    Private Sub Selected_Event_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        Me.WindowState = FormWindowState.Maximized
        cnt = 0
        ReDim NamesSelected(5)
        NamesSelected(0) = "-2"
        NamesSelected(1) = "99999"
        NamesSelected(2) = "99999"
        NamesSelected(3) = "99999"
        NamesSelected(4) = "99999"
        NamesSelected(5) = "99999"

        Me.Controls.Add(selPanel)
        'Me.AutoSize = True
        Dim title As New Label() With {
                .Text = "Family Album",
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("segoe", 45),
                .Size = New Size(600, 55),
                .Location = New Point(CInt(screenWidth / 2 - 300), 22)
            }
        Me.Controls.Add(title)
        title.BringToFront()
        Dim subtitle As New Label() With {
                .Text = "Select People",
                .Font = New Font("segoe", 24),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Size = New Size(600, 40),
                .Location = New Point(CInt(screenWidth / 2 - 300), 100)
            }
        Me.Controls.Add(subtitle)
        Select Case Etype
            Case "Old"
                subtitle.Text = "Select Event for New Images"
            Case "New"
                subtitle.Text = "Enter New Event"
                btnContinue.Enabled = True
            Case Else
                subtitle.Text = "Select Events"
        End Select
        selPanel.Controls.Add(subtitle)
        Me.menuStrip = fmenus()
        selPanel.Controls.Add(menuStrip)

        If Etype = "Ign" Then
            menuStrip.Items.RemoveAt(2)
            Dim menuItemSelectEvent As New ToolStripMenuItem("Select Event") With {
             .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
            menuStrip.Items.Insert(2, menuItemSelectEvent)
            Dim ClearSelected As New ToolStripMenuItem("Clear Selected Events")
            AddHandler ClearSelected.Click, AddressOf ClearSelected_Click
            menuItemSelectEvent.DropDownItems.Add(ClearSelected)
        End If

        With cbEventsOnFile
            .Location = New Point(CInt((screenWidth / 2 - 400)), 150)
            .Size = New Size(800, 18)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .MaxDropDownItems = 6
            .DropDownHeight = 150
        End With

        With lvNamesSelected
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(800, 130)
            .Location = New Point(CInt((screenWidth / 2 - 400)), 400)
        End With
        selPanel.Controls.Add(lvNamesSelected)

        Dim lb1 As New Label() With {
                .Text = "Choose up to 5 Events",
            .Location = New Point(CInt((screenWidth - 400) / 2), 350),
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .Size = New Size(400, 39),
            .TextAlign = ContentAlignment.MiddleCenter}

        CenterControl(btnContinue, 580)
        selPanel.Controls.Add(btnContinue)

        With txtEvent
            .Location = New Point(CInt(screenWidth / 2 - 400), 150)
            .Size = New Size(800, 30)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Multiline = False
            .Visible = False
            .AutoSize = True

        End With
        Dim lblEvent As New Label With {
           .Text = "Event:",
           .TextAlign = ContentAlignment.MiddleRight,
            .Location = New Point(CInt(screenWidth / 2 - 600), 150),
            .Size = New Size(200, 30),
            .Font = New Font("Arial", 12, FontStyle.Regular),
        .Visible = False,
        .AutoSize = False
        }
        With txtEventDetails
            .Location = New Point(CInt(screenWidth / 2 - 400), 230)
            .Size = New Size(800, 350)
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Multiline = True
            .Visible = False
            .AutoSize = True
        End With
        Dim lblEventDetails As New Label With {
            .Text = "Event Details:",
            .TextAlign = ContentAlignment.MiddleRight,
            .Location = New Point(CInt(screenWidth / 2 - 600), 230),
            .Size = New Size(200, 30),
            .Font = New Font("Arial", 12, FontStyle.Regular),
        .Visible = False,
            .AutoSize = False
        }
        selPanel.Controls.Add(txtEvent)
        selPanel.Controls.Add(txtEventDetails)
        selPanel.Controls.Add(lblEvent)
        selPanel.Controls.Add(lblEventDetails)
        AddHandler btnContinue.Click, AddressOf btnContinue_click
        AddHandler cbEventsOnFile.SelectionChangeCommitted, AddressOf cbEventsOnFile_SelectedIndexChanged
        If Etype = "New" Then
            cbEventsOnFile.Visible = False
            txtEvent.Visible = True
            txtEventDetails.Visible = True
            lblEvent.Visible = True
            lblEventDetails.Visible = True
        End If

        selPanel.Controls.Add(lb1)
        If Etype = "Old" Then
            lvNamesSelected.Visible = False
            btnContinue.Visible = False
            lb1.Visible = False
        ElseIf Etype = "New" Then
            lvNamesSelected.Visible = False
            btnContinue.Visible = True
            lb1.Visible = False
        End If

        selPanel.Controls.Add(cbEventsOnFile)


        Dim dt As New DataTable()
        dt = FillEvents()

        cbEventsOnFile.DataSource = dt
        cbEventsOnFile.DisplayMember = "Event"
        cbEventsOnFile.ValueMember = "ID"
        cbEventsOnFile.Focus()
        'cbEventsOnFile.DroppedDown = True
        cnt = 0
    End Sub
    Private Sub cbEventsOnFile_SelectedIndexChanged(sender As Object, e As EventArgs)
        cnt += 1
        If cnt >= 6 Then
            MessageBox.Show("Only 5 events can be selected")
            Exit Sub
        End If

        Dim drv As DataRowView = TryCast(cbEventsOnFile.SelectedItem, DataRowView)
        If drv IsNot Nothing Then
            If Not String.IsNullOrEmpty(Trim(drv(0).ToString())) Then
                If Etype = "Old" Then
                    Dim EventID As String = drv(1).ToString()
                    Dim ad As New AddPhoto
                    ad.EventID = CInt(EventID)
                    ad.Show()
                    Me.Close()
                End If

                NamesSelected(cnt) = CStr(drv(1))
                lvNamesSelected.Items.Add(CStr(drv(0)))
                btnContinue.Enabled = True
            End If
        End If


    End Sub

    Private Sub btnContinue_click(sender As Object, e As EventArgs)
        If Etype = "New" Then
            SaveNewEvent(txtEvent.Text, txtEventDetails.Text)
            '           Dim NewImage As New ImagesNotinDatabase
            'NewImage.Show()
        Else
            Dim thumbForm As New Sthumb() With {
               .NamesSelected = NamesSelected
           }
            thumbForm.Show()
        End If
    End Sub
    Private Sub CenterControl(ctrl As Control, y As Integer)
        ' Calculate the position to center the control
        Dim x As Integer = (Me.ClientSize.Width - ctrl.Width) \ 2
        'Dim y As Integer = (Me.ClientSize.Height - ctrl.Height) \ 2

        ' Set the control's position

        ctrl.Location = New Point(x, y)
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
    Private Sub MenuItemPeople_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

End Class



