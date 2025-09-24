Imports System.Data.SQLite
Imports System.IO
Imports System.Security.Cryptography
Imports AxWMPLib


Public Class Displayinfo
    Dim WithEvents lvNames As New ListView()
    Public Property SFileName As String
    Dim picBox As New PictureBox
    Private menuStrip As New MenuStrip()
    Dim NameCount As Integer
    Dim NameArray As List(Of String)
    Dim WithEvents btnAdd As New System.Windows.Forms.Button()
    Dim WithEvents btnNew As New System.Windows.Forms.Button()
    Dim WithEvents btnUpdateDate As New Button()
    Dim WithEvents BtnDelete As New Button()
    Dim WithEvents BtnUpdateThumb As New Button()
    Dim WithEvents btnSavedesc As New Button()

    Dim WithEvents btnEmbed As New Button()
    Dim Tposition As New TextBox()
    Dim combobox1 As New ComboBox
    Dim lblPosition As New Label()
    Dim dt As New DataTable()


    Dim Manager As New ConnectionManager(GetConnectionString)
    Dim connection As New SQLiteConnection
    Dim cnt As Integer = 1
    Dim mfilename As String
    Dim MemberID As String
    Dim TypeI As Integer
    Dim state As Integer

    Dim Formchanged As Boolean = False
    Public Label1 As New Label() With {
    .Size = New Size(130, 50),
    .Font = New Font("Arial", 12, FontStyle.Bold),
    .Visible = False,
    .Location = New Point(1500, 700)}

    Dim WithEvents BtnRestart As New Button()
    Dim WithEvents txtRelation As New TextBox() With {
            .Location = New Point(20, 812),
            .Size = New Size(800, 300),
            .Font = New Font("Arial", 12, FontStyle.Regular),
            .Multiline = True,
            .BorderStyle = BorderStyle.None,
            .Visible = False,
                    .AutoSize = True
}
    Dim lblRelate As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "Relation to Morton Family ",
                        .Location = New Point(20, 890),
                        .Visible = False,
                        .AutoSize = True
                      }
    Dim WithEvents TxtFullName As New TextBox() With {
            .Location = New Point(20, 750),
            .Size = New Size(800, 30),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .Multiline = False,
            .Visible = False,
                    .AutoSize = True}

    Dim lblName1 As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Size = New Size(800, 30),
                        .Text = "Name",
                        .Location = New Point(20, 720),
                        .Visible = False,
                        .AutoSize = True
                      }
    Dim WithEvents btnUpdate As New Button() With {
                    .Text = "Update",
                    .BackColor = Color.LightBlue,
                    .ForeColor = Color.DarkBlue,
                    .Font = New Font("Arial", 12, FontStyle.Bold),
                    .Size = New Size(220, 40),
                    .Location = New Point(252, 1130),
                    .Visible = False,
                    .AutoSize = True
                    }


    Dim WithEvents btnCancel As New Button() With {
                    .Text = "Cancel",
                    .BackColor = Color.LightBlue,
                    .ForeColor = Color.DarkBlue,
                    .Font = New Font("Arial", 12, FontStyle.Bold),
                    .Size = New Size(220, 40),
                    .Location = New Point(472, 1130),
                    .Visible = False,
                    .AutoSize = True
                    }

    Dim WithEvents btnCopyFile As New Button

    Dim TxtMonth As New TextBox()
    Dim txtYear As New TextBox()
    Dim lblDescription As New Label
    Dim WithEvents txtDescription As New TextBox
    Dim WithEvents txtEvent As New TextBox()
    Dim txtEventDetails As New TextBox()
    Dim lblEvent As New Label()
    Dim ImageData As Image()
    Dim DDir As String
    Dim pl As New AxWindowsMediaPlayer()
    Dim lhp As New Panel()
    Dim rhp As New Panel()
    Public Event SubFormClosing(sender As Object, e As FormClosedEventArgs)

    Sub DisplayInfo_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.WindowState = FormWindowState.Maximized
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.IsMdiContainer = True
        Dim screenWidth As Integer = CInt(Screen.PrimaryScreen.Bounds.Width)
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        Me.menuStrip = fmenus()
        Dim menuItemExit As New ToolStripMenuItem("Exit")
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_Click
        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        menuStrip.Items.RemoveAt(0)
        menuStrip.Items.Insert(0, menuItemExit)
        Me.Controls.Add(menuStrip)
        dt.Columns.Add("Names", GetType(String))
        dt.Columns.Add("Relation", GetType(String))


        With lhp
            .Left = 0
            .Top = 80
            .Width = screenWidth \ 2
            .Height = screenHeight - 80
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
            .AutoScroll = True
        End With
        Me.Controls.Add(lhp)

        With rhp
            .Left = lhp.Right ' ensures it starts right after lhp
            .Top = 80
            .Width = screenWidth - lhp.Width
            .Height = screenHeight - 80
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            .AutoScroll = True
        End With
        Me.Controls.Add(rhp)

        Dim lpw As Integer = lhp.Width
        Dim lph As Integer = lhp.Height
        Me.Controls.Add(rhp)
        Dim title As New Label() With {
            .Text = "Family Album",
            .Font = New Font("segoe", 40),
            .Size = New Size(screenWidth, 60),
            .Location = New Point(0, 20),
            .TextAlign = ContentAlignment.MiddleCenter}

        DDir = GetDefaultDir()
        With lvNames
            .Size = New Drawing.Size(CInt(lpw * 0.6), 450)
            .Location = New Drawing.Point(10, 270)
            .View = View.Details
            .FullRowSelect = True
            .Font = New Font("Arial", 12, FontStyle.Regular)
        End With
        lvNames.Columns.Add("FullName", CInt(lvNames.Width / 2 - 5), HorizontalAlignment.Left)
        lvNames.Columns.Add("Relation", CInt(lvNames.Width / 2 - 5), HorizontalAlignment.Left)

        With Tposition
            .Location = New Point(1005, lvNames.Top + lvNames.Height + 30)
            .Size = New Size(40, 40)
            .Visible = False
        End With

        With lblPosition
            .Location = New Point(800, lvNames.Top + lvNames.Height + 30)
            .Size = New Size(200, 40)
            .Text = "Position of new Person"
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Visible = False
        End With
        With combobox1
            .Location = New Point(20, lvNames.Top + lvNames.Height + 30)
            .Size = New Size(CInt(lpw * 0.6), 21)
            .Font = New Font("Arial", 12)
            .Visible = False
        End With

        Dim Rpont = lvNames.Left + lvNames.Width + 20

        With btnAdd
            .Text = "Add Names"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(220, 40)
            .Location = New Point(Rpont, 300)
        End With
        lhp.Controls.Add(btnAdd)

        With btnNew
            .Text = "Add Name not in List"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(220, 40)
            .Location = New Point(Rpont, 350)
        End With
        lhp.Controls.Add(btnNew)

        With btnCopyFile
            .Text = "Copy Image"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(220, 40)
            .Location = New Point(Rpont, 400)
            .Visible = True
        End With
        With BtnDelete
            .Text = "Delete Image"
            .Location = New Point(Rpont, 450)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(220, 40)
            .AutoSize = True
        End With
        With btnEmbed
        End With
        With BtnUpdateThumb
            .Text = "Update Thumbnail"
            .Location = New Point(Rpont, 550)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(220, 40)
            .AutoSize = True
        End With


        connection = Manager.GetConnection()

        Dim fileName As String = (SFileName)
        Dim command As New SQLiteCommand("SELECT  PDescription,PPeoplelist, PMonth, PYear,Pheight, Pwidth,PType FROM Pictures WHERE Pfilename = @name", connection)
        command.Parameters.AddWithValue("@name", fileName)
        Using reader As SQLiteDataReader = command.ExecuteReader()
            While reader.Read()
                TypeI = CInt(reader("PType"))
                If TypeI <> 1 Then
                    picBox.Visible = False

                    mfilename = DDir & SFileName
                    With pl
                        .Width = CInt((screenWidth / 2))
                        .Height = CInt(0.95 * (screenHeight / 2))
                        .Location = New Point(lhp.Width - pl.Width - 50, 50)
                        .Visible = True
                    End With
                    BtnRestart.Visible = True
                    rhp.Controls.Add(BtnRestart)
                    rhp.Controls.Add(pl)
                    Playvideo(DDir & SFileName)
                Else
                    Dim width As Integer = CInt(reader("Pwidth"))
                    Dim height As Integer = CInt(reader("Pheight"))
                    If SFileName.Contains(DDir) Then
                        SFileName = SFileName.Replace(DDir, "")
                    End If
                    ' Display dimensions
                    ' Force a true decoupling from the file stream
                    Dim imgBytes As Byte() = Nothing
                    Try
                        imgBytes = File.ReadAllBytes(DDir & SFileName)
                        Using ms As New MemoryStream(imgBytes)
                            Dim img = Image.FromStream(ms)
                            picBox.Image = New Bitmap(img) ' Forces it into memory and closes file
                        End Using

                        With picBox
                            .SizeMode = PictureBoxSizeMode.Zoom
                            .Width = CInt((screenWidth / 2))
                            .Location = New Point(lhp.Width - picBox.Width, 50)
                            .Height = CInt(0.95 * (screenHeight / 2))
                            .Visible = True
                        End With
                    Catch ex As Exception
                        Debug.WriteLine(ex.Message)
                    End Try
                End If


                Dim metadata As ParsedMetadata = ReadJsonFromMediaFile(DDir & SFileName)

                If metadata IsNot Nothing Then
                    Console.WriteLine("Event: " & metadata.EventName)
                    Console.WriteLine("Details: " & metadata.EventDetails)
                    For Each p In metadata.People
                        Console.WriteLine($"- {p.Name}: {p.Relationship}")
                    Next
                Else
                    Console.WriteLine("No metadata found in image.")
                End If


                mfilename = ""

                Dim PList As String
                Dim nm As String = CStr(reader("PPeoplelist"))
                If nm Is Nothing OrElse nm Is "" Then
                    'NameArray(0) = "No names on file"
                Else
                    PList = CStr(reader("PPeoplelist"))
                    NameArray = PList.Split(","c).ToList
                End If

                Dim lblPeoplelist As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "People In Picture",
                        .Location = New Point(50, 240),
                        .Width = 100,
                        .Height = 30,
                        .AutoSize = True
                }
                Dim lblMonth As New Label With {
                            .Font = New Font(.Font.FontFamily, 12),
                            .Text = "Month of Picture: ",
                            .Location = New Point(50, 40),
                            .Width = 100,
                            .Height = 30,
                            .AutoSize = True
                            }
                With TxtMonth
                    .Font = New Font(.Font.FontFamily, 12)
                    .Text = If(reader.IsDBNull(reader.GetOrdinal("PMonth")), "0", reader("PMonth").ToString())
                    .Location = New Point(260, 40)
                    .Width = 100
                    .Height = 30
                    .AutoSize = True
                End With

                Dim lblYear As New Label With {
                                .Font = New Font(.Font.FontFamily, 12),
                                .Text = "Year of Picture:",
                                .Location = New Point(50, 90),
                .Width = 200}

                With txtYear
                    .Font = New Font(.Font.FontFamily, 12)
                    .Text = If(reader.IsDBNull(reader.GetOrdinal("Pyear")), "0", reader("Pyear").ToString())
                    .Location = New Point(260, 90)
                    .Size = New Size(100, 30)
                End With

                With btnUpdateDate
                    .Text = "Update Date"
                    .BackColor = Color.LightBlue
                    .ForeColor = Color.DarkBlue
                    .Font = New Font("Arial", 12, FontStyle.Bold)
                    .Size = New Size(220, 40)
                    .Location = New Point(360, 65)
                    .Visible = False
                End With

                Dim lblFile As New Label With {
                            .Font = New Font(.Font.FontFamily, 12),
                            .Text = "Name && location  of Picture  file: " & SFileName,
                            .Location = New Point(50, 150),
                            .AutoSize = True
                            }
                Dim lblDimension As New Label With {
                                .Font = New Font(.Font.FontFamily, 12),
                                .Text = "Height && Width of Picture: " & CInt(reader("Pheight")) & "/" & CInt(reader("Pwidth")),
                                .Location = New Point(50, 200),
                                .AutoSize = True
                                }
                txtDescription.Text = reader("PDescription").ToString


                AddHandler lvNames.MouseUp, AddressOf Listbox_Mouseup
                AddHandler txtEvent.MouseUp, AddressOf Event_Mouseup
                AddHandler btnUpdateDate.Click, AddressOf btnUpdateDate_click
                AddHandler btnCancel.Click, AddressOf btnCancel_click
                AddHandler btnEmbed.Click, AddressOf btnEmbed_Click
                AddHandler BtnUpdateThumb.Click, AddressOf btnUpdateThumb_click
                AddHandler btnUpdate.Click, AddressOf btnUpdate_click
                AddHandler BtnDelete.Click, AddressOf btnAdd_click
                AddHandler btnNew.Click, AddressOf BtnNew_click
                AddHandler BtnRestart.Click, AddressOf btnRestart_click
                AddHandler BtnDelete.Click, AddressOf btnDelete_click
                AddHandler Me.SubFormClosing, AddressOf SubformClosed
                AddHandler btnCopyFile.Click, AddressOf btnCopyFile_Click
                AddHandler txtDescription.TextChanged, Sub(se As Object, ev As EventArgs)
                                                           btnSavedesc.Visible = True
                                                       End Sub
                Me.Controls.Add(title)
                lhp.Controls.Add(lblPeoplelist)
                lhp.Controls.Add(lvNames)
                lhp.Controls.Add(lblMonth)
                lhp.Controls.Add(lblYear)
                lhp.Controls.Add(lblFile)
                lhp.Controls.Add(lblDimension)
                lhp.Controls.Add(Label1)
                lhp.Controls.Add(txtRelation)
                lhp.Controls.Add(lblRelate)
                lhp.Controls.Add(TxtFullName)
                lhp.Controls.Add(lblName1)
                lhp.Controls.Add(lblPosition)
                lhp.Controls.Add(Tposition)
                rhp.Controls.Add(txtDescription)
                rhp.Controls.Add(lblDescription)
                rhp.Controls.Add(txtEvent)
                rhp.Controls.Add(lblEvent)
                rhp.Controls.Add(txtEventDetails)
                lhp.Controls.Add(TxtMonth)
                lhp.Controls.Add(txtYear)
                lhp.Controls.Add(btnUpdateDate)
                lhp.Controls.Add(btnCancel)
                'lhp.Controls.Add(btnEmbed)
                'lhp.Controls.Add(BtnUpdateThumb)
                rhp.Controls.Add(btnSavedesc)
                If TypeI = 1 Then
                    rhp.Controls.Add(picBox)
                Else
                    rhp.Controls.Add(BtnRestart)
                End If
                lhp.Controls.Add(btnCopyFile)
                If Not mfilename = "" Then
                    '    PlayVideo(mfilename)
                End If
            End While
        End Using
        'reader.Close()

        FilllvNames()
        lhp.Controls.Add(btnUpdate)

        Dim EventName As String = ""
        Dim EventDetail As String = ""
        connection = Manager.GetConnection()
        Using connection
            Dim command1 As New SQLiteCommand("select neName,neRelation from NamePhoto inner join NameEvent on ID = npID where neType = 'E' and npFilename = @filename", connection)
            command1.Parameters.AddWithValue("@filename", fileName)
            txtEvent.Text = "Add Event"
            txtEvent.Visible = True
            Using reader1 As SQLiteDataReader = command1.ExecuteReader()
                While reader1.Read()
                    txtEvent.Text = CStr(reader1("neName"))
                    Dim relationValue As String = If(reader1.IsDBNull(reader1.GetOrdinal("neRelation")), "", reader1("neRelation").ToString().Trim())

                    If relationValue = "NULL" Then
                        txtEventDetails.Visible = False
                    Else
                        txtEventDetails.Text = reader1("neRelation").ToString
                        txtEventDetails.Visible = True
                    End If
                End While
            End Using
        End Using
        Dim xCenter As Integer
        Dim yPosition As Integer
        If TypeI = 1 Then
            ' Calculate horizontal center of picbox
            xCenter = picBox.Location.X + (picBox.Width \ 2) - (800 \ 2)
            ' Set the vertical position just below picbox
            yPosition = picBox.Location.Y + picBox.Height + 10 ' Adjust 10 for spacing
        Else
            xCenter = pl.Location.X + (pl.Width \ 2) - (800 \ 2)
            yPosition = pl.Location.Y + pl.Height + 10 ' Adjust 10 for spacing
        End If
        ' Apply the new location to Control2
        With lblEvent
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(xCenter, yPosition + 2)
            .AutoSize = True
            .Visible = False
            .Text = "Event:"
        End With
        With lblDescription
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(CInt(lpw / 2 - 400), CInt(lph / 2) + 85)
            .AutoSize = True
            .Visible = True
            .Size = New Size(40, 80)
            .Text = "Description:"
        End With
        With txtDescription
            .Location = New Point()
            .Visible = True
            .Width = 750
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 60
            .Location = New Point(CInt(lpw / 2 - 300), CInt(lph / 2) + 60)

            .Multiline = True
            .AutoSize = True
            .BorderStyle = BorderStyle.FixedSingle
        End With
        With btnSavedesc
            .Text = "Save"
            .Location = New Point(CInt(lpw / 2 + 460), CInt(lph / 2) + 70)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(150, 40)
            .Visible = False
        End With
        AddHandler btnSavedesc.Click, AddressOf btnSavedesc_click
        With txtEvent
            .Location = New Point(xCenter + 75, yPosition + 70)
            .Visible = True
            .Width = 750
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 40
            .ReadOnly = True
            .AutoSize = True
            .BorderStyle = BorderStyle.None
        End With
        With txtEventDetails
            .Location = New Point(xCenter + 75, yPosition + 120)
            .Width = 750
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 100
            '.AutoSize = True
            .BorderStyle = BorderStyle.None
            .ReadOnly = True
            .Multiline = True
        End With

        If TypeI <> 1 Then
            picBox.Visible = False

            mfilename = DDir & SFileName
            With pl
                .Width = CInt((screenWidth / 2))
                .Height = CInt(0.95 * (screenHeight / 2))
                .Location = New Point(lhp.Width - pl.Width - 50, 50)
                .Visible = True
            End With

            With BtnRestart
                .Text = "Restart"
                .BackColor = Color.LightBlue
                .ForeColor = Color.DarkBlue
                .Font = New Font("Arial", 12, FontStyle.Bold)
                .Size = New Size(220, 40)
                .Location = New Point(lpw \ 2 - 110, txtEventDetails.Top + txtEventDetails.Height + 20)
                .Visible = False
                .AutoSize = True
            End With

            BtnRestart.Visible = True
            rhp.Controls.Add(BtnRestart)
            rhp.Controls.Add(pl)
            Playvideo(DDir & SFileName)
        Else
            With picBox
                .SizeMode = PictureBoxSizeMode.Zoom
                .Width = CInt((screenWidth / 2))
                .Location = New Point(lhp.Width - picBox.Width, 50)
                .Height = CInt(0.95 * (screenHeight / 2))
                .Visible = True
            End With
            Dim img As Image
            Try
                Using fs As New FileStream(DDir & SFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    img = Image.FromStream(fs)
                End Using
            Catch
                ShowTextInPictureBox(picBox, "Picture Deleted")
            End Try



            mfilename = ""
        End If
        lhp.Controls.Add(BtnDelete)
        AddHandler TxtMonth.TextChanged, AddressOf Datechanged
        AddHandler txtYear.TextChanged, AddressOf Datechanged

    End Sub

    Private Sub FilllvNames(sender As Object, e As EventArgs)
        lvNames.Items.Clear()
        Dim qryfixname As String = "select neName,neRelation  from NameEvent where ID= @Name "
        If NameArray Is Nothing Then Exit Sub
        connection = Manager.GetConnection()
        For I = 0 To NameArray.Count - 1
            Try
                If Not (NameArray(I) = "0") Then
                    Dim command1 As New SQLiteCommand(qryfixname, connection)
                    command1.Parameters.AddWithValue("@Name", NameArray(I))
                    Dim reader1 As SQLiteDataReader = command1.ExecuteReader()
                    reader1.Read()

                    Dim item1 As New ListViewItem(reader1("neName").ToString)
                    If Not reader1.IsDBNull(reader1.GetOrdinal("neRelation")) Then
                        item1.SubItems.Add(Convert.ToString(reader1("neRelation")))
                        lvNames.Items.Add(item1)
                        reader1.Close()
                    Else
                        item1.SubItems.Add(" ")
                        lvNames.Items.Add(item1)
                        reader1.Close()
                    End If
                End If
            Catch ex As Exception
                'MessageBox.Show(ex.Message)
            End Try
            Tposition.Text = CStr(NameArray.Count)
        Next
        connection.Close()
    End Sub
    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)

        Me.Close()
    End Sub
    Private Sub btnAdd_click(sender As Object, e As EventArgs)
        combobox1.Visible = True
        combobox1.Focus()
        'combobox1.DroppedDown = True
        lblPosition.Visible = True
        Tposition.Visible = True

        FillNamesAvail()
    End Sub
    Private Sub FillNamesAvail()
        combobox1.DataSource = Nothing
        combobox1.Items.Clear()
        dt = FillNames()
        RemoveHandler combobox1.SelectionChangeCommitted, AddressOf NameCombo_SelectionChangeCommitted

        combobox1.DataSource = dt
        combobox1.DisplayMember = "Names"
        combobox1.ValueMember = "ID"
        lhp.Controls.Add(combobox1)
        lhp.Controls.Add(lblPosition)
        lhp.Controls.Add(Tposition)
        combobox1.SelectedIndex = -1
        AddHandler combobox1.SelectionChangeCommitted, AddressOf NameCombo_SelectionChangeCommitted
        combobox1.Focus()
    End Sub
    Private Sub NameCombo_SelectionChangeCommitted(sender As Object, e As EventArgs)

        ' Cast the sender to a ComboBox
        Dim comboBox As System.Windows.Forms.ComboBox = CType(sender, System.Windows.Forms.ComboBox)
        Dim selectedRow As DataRowView = CType(comboBox.SelectedItem, DataRowView)
        If selectedRow Is Nothing Then Exit Sub
        Dim selectedPerson As String = selectedRow("ID").ToString()
        Dim insertIndex As Integer
        If comboBox.SelectedItem IsNot Nothing Then
            Try
                insertIndex = CInt(Tposition.Text)

            Catch
                MessageBox.Show("Position has to be an Integer  ")
                Exit Sub
            End Try
            NameArray = ModifyPeopleList(String.Join(",", NameArray), insertIndex, 1, selectedPerson).Split(","c).ToList
            SavePhoto()
            FilllvNames()
            Formchanged = True
            Tposition.Text = CStr(NameArray.Count + 1)
        Else
            MessageBox.Show("No selection made.")
        End If
    End Sub

    Private Sub Listbox_Mouseup(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            ' Handle left-click event
            If lvNames.SelectedItems.Count > 0 Then
                Dim selectedItem As ListViewItem = lvNames.SelectedItems(0)
                txtRelation.Text = (selectedItem.SubItems(1).Text)
                txtRelation.Visible = True
                lblRelate.Visible = True
                btnUpdate.Visible = True
                btnCancel.Visible = True
                TxtFullName.Visible = True
                TxtFullName.Text = selectedItem.Text
                lblName1.Visible = True
                MemberID = GetMemberID(TxtFullName.Text, "")
                AddHandler txtRelation.TextChanged, AddressOf TxtReslationTextChanged
                AddHandler TxtFullName.TextChanged, AddressOf TxtReslationTextChanged
            End If
        ElseIf e.Button = MouseButtons.Right Then
            DeletePerson()
        End If
    End Sub
    Private Sub Event_Mouseup(sender As Object, e As MouseEventArgs)

        If txtEvent.Text <> "Add Event" Then
            Dim EventID = GeteventID(txtEvent.Text, txtEventDetails.Text)

            ' Handle left-click event
            Dim result As Integer = MsgBox("Are you sure you want to delete the event from this picture?", MsgBoxStyle.YesNo, "Delete Event")
            If result = vbYes Then
                DeleteAPerson(EventID, SFileName, NameArray.Count)
                lblEvent.Visible = False
                txtEvent.Text = "Add Event"
                txtEventDetails.Visible = False
            End If
        Else


            Dim input As String = InputBox("Enter the EventID:")

            If String.IsNullOrEmpty(input) Then
                ' User clicked Cancel or entered nothing
                Exit Sub
            Else
                Dim EventID As Integer
                If Integer.TryParse(input, EventID) Then
                    Dim connection As SQLiteConnection = Manager.GetConnection()
                    Try
                        Using connection
                            Using command As New SQLiteCommand("INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)", connection)
                                command.Parameters.AddWithValue("@Filename", SFileName)
                                command.Parameters.AddWithValue("@ID", EventID)
                                command.ExecuteNonQuery()
                            End Using
                        End Using
                    Catch ex As SQLiteException
                        MessageBox.Show(ex.Message)
                    End Try

                    Dim Eventinfo = GetEvent(CStr(EventID))
                    txtEvent.Text = Eventinfo.EventName
                    txtEventDetails.Text = Eventinfo.Eventdetail
                    lblEvent.Visible = True
                    txtEvent.Visible = True
                    txtEventDetails.Visible = True
                Else
                    MessageBox.Show("Invalid input. Please enter a number.")
                End If
            End If
        End If
    End Sub

    Private Sub TxtReslationTextChanged(sender As Object, e As EventArgs)
        btnUpdate.Visible = True
    End Sub
    Private Sub DeletePerson()
        Dim selectedname As String
        Dim response As Integer
        Dim newplist As String
        If lvNames.SelectedItems.Count > 0 Then
            Dim selectedItem As ListViewItem = lvNames.SelectedItems(0)
            response = MsgBox("Are you sure you want to delete - " & selectedItem.Text, vbYesNo, "Confirmation")
        End If
        If response = vbNo Then
            Exit Sub
        Else
            selectedname = GetMemberID(lvNames.SelectedItems(0).Text, "")
            newplist = DeleteAPerson(selectedname, SFileName, NameCount)
            NameArray = newplist.Split(",").ToList
            connection = Manager.GetConnection()
            Using connection
                Try
                    Dim Command = New SQLiteCommand("Delete from NamePhoto where npID =@name and npFilename =@filename", connection)
                    Command.Parameters.AddWithValue("@name", CInt(selectedname))
                    Command.Parameters.AddWithValue("@filename", SFileName)
                    Dim result As Integer = Command.ExecuteNonQuery()
                    Debug.WriteLine(result)
                Catch ex As Exception
                    Debug.WriteLine(ex.Message)
                End Try
            End Using
        End If
        Tposition.Text = CStr(NameArray.Count + 1)

        Formchanged = True
        FilllvNames()
    End Sub

    Private Sub BtnNew_click(sender As Object, e As EventArgs)
        Dim New1 As New NewName(SFileName, "DisplayInfo")
        New1.TopLevel = False
        AddHandler New1.FormClosed, AddressOf SubformClosed
        lhp.Controls.Add(New1)
        New1.BringToFront()
        New1.Show()
    End Sub


    Private Sub SubformClosed(sender As Object, e As FormClosedEventArgs)
        NameArray = (Label1.Text).Split(","c).ToList()
        FilllvNames()
        FillNamesAvail()
    End Sub
    Private Sub btnUpdate_click(sender As Object, e As EventArgs)
        connection = Manager.GetConnection()
        Dim qryUpdate = "update NameEvent set neName =@name1, neRelation = @relation where ID=@ID"
        Using connection
            Try
                Dim command As New SQLiteCommand(qryUpdate, connection)
                command.Parameters.AddWithValue("@name1", TxtFullName.Text)
                command.Parameters.AddWithValue("@relation", txtRelation.Text)
                command.Parameters.AddWithValue("@ID", MemberID)
                command.ExecuteNonQuery()
            Catch ex As Exception
                MessageBox.Show("Update error" & ex.Message)
            End Try
        End Using
        Formchanged = True
        FilllvNames()
        txtRelation.Visible = False
        lblRelate.Visible = False
        btnUpdate.Visible = False
        btnCancel.Visible = False
        TxtFullName.Visible = False
        lblName1.Visible = False
    End Sub
    Private Sub btnRestart_click(sender As Object, e As EventArgs)
        If pl.Ctlcontrols.currentPosition = 0 Then
            pl.Ctlcontrols.play()
        End If
    End Sub
    Private Sub btnDelete_click(sender As Object, e As EventArgs)
        Dim Response As Integer = MsgBox("Are you sure you want to delete this picture/Video., Deleteing a picture or video cannot be undone.", vbYesNo, "Confirmation")

        If Response = vbNo Then
            Exit Sub
        Else
            Try
                connection = Manager.GetConnection()
                Using connection
                    ' Begin a transaction
                    Dim transaction As SQLiteTransaction = connection.BeginTransaction()

                    Try
                        Using command As New SQLiteCommand("Delete from Pictures  WHERE PfileName = @filename", connection)
                            command.Transaction = transaction
                            System.Threading.Thread.Sleep(100)
                            command.Parameters.AddWithValue("@filename", SFileName)
                            command.ExecuteNonQuery()

                            ' Delete all event and people records
                            command.CommandText = "Delete from NamePhoto where  npfilename=@filename"
                            command.ExecuteNonQuery()
                            ' Delete Pictures record
                            If picBox.Image IsNot Nothing Then
                                picBox.Image.Dispose()
                            End If
                            picBox.Dispose()
                            GC.Collect()
                            GC.WaitForPendingFinalizers()
                            Dim filePath As String = GetDefaultDir() & SFileName ' Delete the image
                            If System.IO.File.Exists(filePath) Then
                                System.IO.File.Delete(filePath)
                            End If
                            ' Commit the transaction if all updates succeed
                            transaction.Commit()
                            Formchanged = True
                        End Using

                        Console.WriteLine("Transaction committed successfully.")
                    Catch ex As Exception
                        ' Rollback the transaction if an error occurs
                        transaction.Rollback()
                        MessageBox.Show("Delete Name Transaction rolled back. Error: " & ex.Message)
                    End Try
                    Me.Close()


                End Using
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub
    Private Sub FilllvNames()
        lvNames.Items.Clear()
        Dim qryfixname As String = "select neName,neRelation  from NameEvent where ID= @Name "
        If NameArray Is Nothing Then Exit Sub
        connection = Manager.GetConnection()

        For I = 0 To NameArray.Count - 1
            Try
                If Not (NameArray(I) = "0") Then
                    Dim command1 As New SQLiteCommand(qryfixname, connection)
                    command1.Parameters.AddWithValue("@Name", NameArray(I))

                    Dim reader1 As SQLiteDataReader = command1.ExecuteReader()
                    reader1.Read()

                    Dim item1 As New ListViewItem(reader1("neName").ToString)
                    If Not reader1.IsDBNull(reader1.GetOrdinal("neRelation")) Then
                        item1.SubItems.Add(Convert.ToString(reader1("neRelation")))
                        lvNames.Items.Add(item1)
                        reader1.Close()
                    Else
                        item1.SubItems.Add(" ")
                        lvNames.Items.Add(item1)
                        reader1.Close()
                    End If
                End If
            Catch ex As Exception
                'MessageBox.Show(ex.Message)
            End Try
            Tposition.Text = CStr(NameArray.Count)
        Next


        connection.Close()
    End Sub

    Private Sub btnUpdateDate_click(sender As Object, e As EventArgs)
        connection = Manager.GetConnection()
        Dim re As Integer
        Using connection
            ' Begin a transaction
            Dim transaction As SQLiteTransaction = connection.BeginTransaction()

            Try
                Using command As New SQLiteCommand("Update Pictures set PMonth =@month, Pyear=@year WHERE PfileName = @filename", connection)
                    command.Transaction = transaction
                    command.Parameters.AddWithValue("@filename", SFileName)
                    command.Parameters.AddWithValue("@month", CInt(TxtMonth.Text))
                    command.Parameters.AddWithValue("@year", CInt(txtYear.Text))
                    re = command.ExecuteNonQuery()
                    transaction.Commit()
                    Formchanged = True
                End Using
            Catch ex As Exception
                ' Rollback the transaction if an error occurs
                transaction.Rollback()
                MessageBox.Show("Update Date Transaction rolled back. Error: " & ex.Message)
            End Try
            btnUpdateDate.Visible = False
        End Using
    End Sub
    Private Sub btnSavedesc_click(sender As Object, e As EventArgs)
        connection = Manager.GetConnection()
        Dim re As Integer
        Using connection
            ' Begin a transaction
            Dim transaction As SQLiteTransaction = connection.BeginTransaction()

            Try
                Using command As New SQLiteCommand("Update Pictures set Pdescription = @description WHERE PfileName = @filename", connection)
                    command.Transaction = transaction
                    command.Parameters.AddWithValue("@filename", SFileName)
                    command.Parameters.AddWithValue("@description", txtDescription.Text)
                    re = command.ExecuteNonQuery()
                    transaction.Commit()
                    Formchanged = True
                End Using
            Catch ex As Exception
                ' Rollback the transaction if an error occurs
                transaction.Rollback()
                MessageBox.Show("Update Date Transaction rolled back. Error: " & ex.Message)
            End Try
        End Using
    End Sub
    Private Sub Datechanged(sender As Object, e As EventArgs)
        btnUpdateDate.Visible = True
    End Sub

    Private Sub formclosinhg()
        If Formchanged = True Then
            Dim jsonMetadata As String = BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames)
            WriteJsonMetadataToMediaFile(DDir & SFileName, jsonMetadata)
        End If

    End Sub
    Private Sub Playvideo(videopath As String)
        ' Ensure the control is created before modifying properties
        If Not pl.Created Then
            pl.CreateControl()
        End If
        Dim processes = Process.GetProcessesByName("wmplayer") ' For classic Windows Media Player
        For Each proc As Process In processes
            Try
                proc.Kill()
                proc.WaitForExit()
                Console.WriteLine("Windows Media Player closed.")
            Catch ex As Exception
                Console.WriteLine("Error closing Windows Media Player: " & ex.Message)
            End Try
        Next
        ' Set video path and play
        pl.URL = videopath
        pl.uiMode = "none"
        pl.Ctlcontrols.play()


    End Sub
    Private Sub btnCopyFile_Click(sender As Object, e As EventArgs)
        Dim folderDialog As New FolderBrowserDialog()
        Dim DDir = GetDefaultDir()
        If folderDialog.ShowDialog() = DialogResult.OK Then
            Dim destinationPath As String = Path.Combine(folderDialog.SelectedPath, Path.GetFileName(SFileName))
            Try
                File.Copy(DDir & SFileName, destinationPath, True)
                MessageBox.Show("File copied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Error copying file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub
    Private Sub btnEmbed_Click(sender As Object, e As EventArgs)
        Dim jsonMetadata As String
        If TypeI = 1 Then
            If picBox.Image IsNot Nothing Then
                picBox.Image.Dispose()
                picBox.Image = Nothing
            End If
            jsonMetadata = BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames)
            WriteJsonMetadataToMediaFile(DDir & SFileName, jsonMetadata)
            Dim imgBytes As Byte() = File.ReadAllBytes(DDir & SFileName)
            Using ms As New MemoryStream(imgBytes)
                Dim img = Image.FromStream(ms)
                picBox.Image = New Bitmap(img)
            End Using
        Else
            AddHandler pl.PlayStateChange, Sub(senderObj As Object, eArgs As AxWMPLib._WMPOCXEvents_PlayStateChangeEvent)
                                               If eArgs.newState = WMPLib.WMPPlayState.wmppsMediaEnded Then
                                                   pl.Ctlcontrols.stop()
                                                   pl.URL = String.Empty
                                                   pl.close()

                                                   Try
                                                       jsonMetadata = BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames)
                                                       WriteJsonMetadataToMediaFile(DDir & SFileName, jsonMetadata)
                                                   Catch ex As Exception
                                                       Debug.WriteLine("Metadata write failed: " & ex.Message)
                                                   End Try

                                                   RemoveHandler pl.PlayStateChange, Nothing ' Optional cleanup
                                               End If
                                           End Sub
        End If
    End Sub
    Private Sub btnCancel_click(sender As Object, e As EventArgs)
        txtRelation.Visible = False
        lblRelate.Visible = False
        btnUpdate.Visible = False
        btnCancel.Visible = False
        TxtFullName.Visible = False
        lblName1.Visible = False
    End Sub
    Private Sub btnUpdateThumb_click(sender As Object, e As EventArgs)
        Updatethumb(DDir & SFileName)

    End Sub
    Private void SavePhoto(Object sender, EventArgs e)
        {
            String plist = createPeoplelist();
            var connection = Manager.GetConnection();


            Using (connection)
            {


                var transaction = connection.BeginTransaction();

                Try
                {
                    var command = New SQLiteCommand() {connection = connection, transaction = transaction};

                    // Create Picture Record
                    InsertPictureRecord(command, plist);
                    // 🔎 Save thumbnail from DB for visual verification

                    InsertNamePhotoRecords(command, plist);

                    // Handle Event Type Record
                    If (Etype!= "No")
                        InsertEventRecord(command);

                    // Remove from unindexedFiles
                    command.CommandText = "DELETE FROM unindexedFiles WHERE uiFilename=@Filename";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Filename",DDir + SFileName);
                    int result = command.ExecuteNonQuery();

                    transaction.Commit();
                }
                Catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Save Date Transaction rolled back. Error: " + ex.Message);
                }
            }

            String jsonMetadata = jsonlist.BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames);
            jsonlist.WriteJsonMetadataToMediaFile(DDir + SFileName, jsonMetadata);

            // Cleanup UI
            ResetUI();
        }

        // Encapsulated SQL Methods
        Private void InsertPictureRecord(SQLiteCommand command, String plist)
        {
            command.CommandText = @"INSERT INTO Pictures ([Pfilename], [PfileDirectory], [PDescription], [PHeight], [PWidth], 
        [PPeopleList], [PMonth], [PYear], [PSoundFile], [PDateEntered], [PType], [PLastModifiedDate], 
        [PReviewed], [PTime], [PThumbnail], [PNameCount]) 
        VALUES (@Pfilename, @PfileDirectory,@PDescription, @PHeight, @PWidth, @PPeopleList, @PMonth, @PYear, 
        @PSoundFile, @PDateEntered, @PType, @PLastModifiedDate, @PReviewed, @PTime, @PThumbnail, @PNameCount)";

            {
                var withBlock = command.Parameters;
                withBlock.AddWithValue("@Pfilename", SFileName);
                withBlock.AddWithValue("@PfileDirectory", FileDirectory);
                withBlock.AddWithValue("@PDescription", txtDescription.Text);
                withBlock.AddWithValue("@PHeight", Mheight);
                withBlock.AddWithValue("@PWidth", Mwidth);
                withBlock.AddWithValue("@PPeopleList", plist);
                withBlock.AddWithValue("@PMonth", TxtMonth.Text);
                withBlock.AddWithValue("@PYear", txtYear.Text);
                withBlock.AddWithValue("@PSoundFile", "");
                withBlock.AddWithValue("@PDateEntered", DateTime.Today);
                withBlock.AddWithValue("@PType", itype);
                withBlock.AddWithValue("@PLastModifiedDate", DateTime.Today);
                withBlock.AddWithValue("@PReviewed", 0);
                withBlock.AddWithValue("@PTime", Playtime);
                withBlock.Add(New SQLiteParameter("@PThumbnail", DbType.Binary) { Value = thumb });
                withBlock.AddWithValue("@PNameCount", lvNames.Items.Count);
            }
            command.ExecuteNonQuery();
        }

        Private void InsertNamePhotoRecords(SQLiteCommand command, String pList)
        {
            var pitems = pList.Split(',').ToList();
            command.CommandText = "INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)";
            command.Parameters.AddWithValue("@Filename", SFileName);
            command.Parameters.Add("@ID", DbType.Int32);

            foreach (string pitem in pitems)
            {
                command.Parameters["@ID"].Value = Conversions.ToInteger(pitem);
                command.ExecuteNonQuery();
            }
        }

        Private void InsertEventRecord(SQLiteCommand command)
        {
            command.CommandText = "INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)";
            command.Parameters["@ID"].Value = EventID;
            command.ExecuteNonQuery();
        }

End Class

